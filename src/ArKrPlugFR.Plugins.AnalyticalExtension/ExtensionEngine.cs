using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Analytical;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>What one run did.</summary>
    internal sealed class ExtensionOutcome
    {
        internal ExtensionOutcome(
            string? stopped,
            int sources,
            int extended,
            int noTarget,
            int noLine,
            int attached,
            int refused,
            IReadOnlyList<string> notes)
        {
            Stopped = stopped;
            Sources = sources;
            Extended = extended;
            NoTarget = noTarget;
            NoLine = noLine;
            Attached = attached;
            Refused = refused;
            Notes = notes ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal int Sources { get; }

        internal int Extended { get; }

        internal int NoTarget { get; }

        internal int NoLine { get; }

        internal int Attached { get; }

        internal int Refused { get; }

        internal IReadOnlyList<string> Notes { get; }
    }

    /// <summary>
    /// Extends the analytical model of the picked columns and walls up to the nearest floor
    /// or beam above them.
    /// <para>
    /// The choice of the target follows <see cref="ExtensionRules"/>, which is tested without
    /// Revit. The extension writes the analytical line where the release allows it and falls
    /// back to raising the physical column through its top level where it does not, so the
    /// same command works on Revit 2022 and 2024. The whole run is one undoable step.
    /// </para>
    /// </summary>
    internal static class ExtensionEngine
    {
        /// <summary>Runs the extension for the picked sources and targets.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="sources">The columns and walls to extend.</param>
        /// <param name="targetElements">The floors and beams to extend to.</param>
        /// <param name="reachMillimeters">How far in plan a target may lie.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static ExtensionOutcome Run(
            Document document,
            IReadOnlyList<Element> sources,
            IReadOnlyList<Element> targetElements,
            double reachMillimeters,
            ExtensionText text,
            Log log)
        {
            var notes = new List<string>();
            var targets = new List<ExtensionTarget>();
            var targetIds = new List<ElementId>();

            foreach (Element element in targetElements)
            {
                if (TryReadBox(element, out BoundingBox bounds))
                {
                    targets.Add(new ExtensionTarget(bounds));
                    targetIds.Add(element.Id);
                }
            }

            if (targets.Count == 0)
            {
                return new ExtensionOutcome(text.NoTargetBoxes, sources.Count, 0, 0, 0, 0, 0, notes);
            }

            int extended = 0;
            int noTarget = 0;
            int noLine = 0;
            int attached = 0;
            int refused = 0;

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                foreach (Element source in sources)
                {
                    string description = ElementDescription.Describe(source);

                    try
                    {
                        if (!TryReadAnalyticalLine(document, source, out Element? analytical, out Curve? curve))
                        {
                            noLine++;
                            notes.Add(text.NoAnalyticalLine(description));

                            continue;
                        }

                        XYZ first = curve!.GetEndPoint(0);
                        XYZ second = curve.GetEndPoint(1);
                        XYZ top = first.Z >= second.Z ? first : second;
                        XYZ bottom = first.Z >= second.Z ? second : first;

                        Point3 topMillimeters = ToMillimeters(top);
                        Point3 bottomMillimeters = ToMillimeters(bottom);

                        int best = ExtensionRules.FindBestTarget(topMillimeters, targets, reachMillimeters);

                        if (best < 0)
                        {
                            noTarget++;
                            notes.Add(text.NoTargetAbove(description));

                            continue;
                        }

                        double targetZ = targets[best].BottomZ;
                        Point3 newTop = ExtensionRules.ExtendedTop(topMillimeters, targetZ);

                        if (TryWriteAnalyticalLine(analytical!, bottomMillimeters, newTop))
                        {
                            extended++;

                            log.Info(
                                "AnalyticalExtension: extended " + description + " to " +
                                targetZ.ToString("0.###") + " mm.",
                                nameof(ExtensionEngine));

                            continue;
                        }

                        if (IsAttachedAtTop(source))
                        {
                            attached++;
                            notes.Add(text.Attached(description));

                            continue;
                        }

                        if (TryRaiseColumn(document, source, targetZ))
                        {
                            extended++;
                            notes.Add(text.RaisedColumn(description));

                            continue;
                        }

                        refused++;
                        notes.Add(text.Refused(description, "The platform did not accept the new line."));
                    }
                    catch (Exception exception)
                    {
                        refused++;
                        notes.Add(text.Refused(description, exception.Message));

                        log.Warn(
                            "AnalyticalExtension: " + description + " was refused: " + exception.Message,
                            nameof(ExtensionEngine),
                            exception);
                    }
                }

                return extended > 0;
            });

            log.Info(
                "AnalyticalExtension: " + extended + " extended, " + noTarget + " without a target, " +
                noLine + " without an analytical line, " + attached + " attached, " + refused + " refused.",
                nameof(ExtensionEngine));

            return new ExtensionOutcome(null, sources.Count, extended, noTarget, noLine, attached, refused, notes);
        }

        private static bool TryReadAnalyticalLine(
            Document document,
            Element source,
            out Element? analytical,
            out Curve? curve)
        {
            analytical = null;
            curve = null;

            if (AnalyticalCompat.IsAnalyticalElement(source) && source.Location is LocationCurve own && own.Curve != null)
            {
                analytical = source;
                curve = own.Curve;

                return true;
            }

            ElementId analyticalId = AnalyticalCompat.FindAnalyticalId(document, source.Id);

            if (analyticalId == ElementId.InvalidElementId)
            {
                return false;
            }

            Element? element = document.GetElement(analyticalId);

            if (element == null || !(element.Location is LocationCurve location) || location.Curve == null)
            {
                return false;
            }

            analytical = element;
            curve = location.Curve;

            return true;
        }

        private static bool TryWriteAnalyticalLine(Element analytical, Point3 bottom, Point3 top)
        {
            if (!(analytical.Location is LocationCurve location) || location.Curve == null)
            {
                return false;
            }

            try
            {
                location.Curve = Line.CreateBound(ToXyz(bottom), ToXyz(top));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsAttachedAtTop(Element source)
        {
            Parameter? attached = source.get_Parameter(BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM);

            return attached != null && attached.AsInteger() != 0;
        }

        private static bool TryRaiseColumn(Document document, Element source, double targetZMillimeters)
        {
            Parameter? topLevel = source.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
            Parameter? topOffset = source.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

            if (topLevel == null || topOffset == null || topOffset.IsReadOnly)
            {
                return false;
            }

            Level? level = document.GetElement(topLevel.AsElementId()) as Level;

            if (level == null)
            {
                return false;
            }

            double offsetFeet = Length.FromMillimeters(targetZMillimeters).Feet - level.Elevation;

            return topOffset.Set(offsetFeet);
        }

        private static bool TryReadBox(Element element, out BoundingBox bounds)
        {
            bounds = default;

            Autodesk.Revit.DB.BoundingBoxXYZ? box = element.get_BoundingBox(null);

            if (box == null)
            {
                return false;
            }

            bounds = new BoundingBox(ToMillimeters(box.Min), ToMillimeters(box.Max));

            return true;
        }

        private static Point3 ToMillimeters(XYZ point)
        {
            return new Point3(
                Length.FromFeet(point.X).Millimeters,
                Length.FromFeet(point.Y).Millimeters,
                Length.FromFeet(point.Z).Millimeters);
        }

        private static XYZ ToXyz(Point3 point)
        {
            return new XYZ(
                Length.FromMillimeters(point.X).Feet,
                Length.FromMillimeters(point.Y).Feet,
                Length.FromMillimeters(point.Z).Feet);
        }
    }
}
