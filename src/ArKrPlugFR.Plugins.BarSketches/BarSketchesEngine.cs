using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Sketches;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>What one run made.</summary>
    internal sealed class SketchOutcome
    {
        internal SketchOutcome(
            string? stopped,
            int bars,
            int made,
            int skipped,
            int refused,
            int remaining,
            IReadOnlyList<string> sheetLines,
            IReadOnlyList<string> notes)
        {
            Stopped = stopped;
            Bars = bars;
            Made = made;
            Skipped = skipped;
            Refused = refused;
            Remaining = remaining;
            SheetLines = sheetLines ?? Array.Empty<string>();
            Notes = notes ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal int Bars { get; }

        internal int Made { get; }

        internal int Skipped { get; }

        internal int Refused { get; }

        internal int Remaining { get; }

        internal IReadOnlyList<string> SheetLines { get; }

        internal IReadOnlyList<string> Notes { get; }
    }

    /// <summary>
    /// Makes one sheet per picked bar: a view cropped to the bar and a note with its facts.
    /// <para>
    /// The bar's own geometry is shown, not a redrawn picture of it: the view is a
    /// three-dimensional view cropped by a section box around the bar, placed on a sheet made
    /// from the title block of the project, and the note states the shape, the diameter, the
    /// length and the mark. The run is capped by <see cref="SketchRules"/> and is one undoable
    /// step.
    /// </para>
    /// </summary>
    internal static class BarSketchesEngine
    {
        /// <summary>How far the section box is grown around a bar, in millimetres.</summary>
        private const double BoxMarginMillimeters = 300;

        /// <summary>Runs the sheet making for the picked bars.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="bars">The bars to sketch.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run made.</returns>
        internal static SketchOutcome Run(
            Document document,
            IReadOnlyList<Rebar> bars,
            SketchText text,
            Log log)
        {
            if (bars.Count == 0)
            {
                return new SketchOutcome(text.NoBars, 0, 0, 0, 0, 0, Array.Empty<string>(), Array.Empty<string>());
            }

            ElementId titleBlock = FirstTitleBlock(document);

            if (titleBlock == ElementId.InvalidElementId)
            {
                return new SketchOutcome(text.NoTitleBlock, bars.Count, 0, 0, 0, 0, Array.Empty<string>(), Array.Empty<string>());
            }

            ElementId viewFamily = FirstThreeDimensionalViewFamily(document);

            if (viewFamily == ElementId.InvalidElementId)
            {
                return new SketchOutcome(text.NoViewFamily, bars.Count, 0, 0, 0, 0, Array.Empty<string>(), Array.Empty<string>());
            }

            int planned = SketchRules.Plan(bars.Count, SketchRules.MaximumSketches);
            int made = 0;
            int skipped = 0;
            int refused = 0;
            var sheetLines = new List<string>();
            var notes = new List<string>();
            HashSet<string> viewNames = ViewNames(document);

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                for (int index = 0; index < planned; index++)
                {
                    Rebar bar = bars[index];
                    string description = ElementDescription.Describe(bar);

                    try
                    {
                        Autodesk.Revit.DB.BoundingBoxXYZ? box = bar.get_BoundingBox(null);

                        if (box == null)
                        {
                            skipped++;
                            notes.Add(text.NoBox(description));

                            continue;
                        }

                        string shape = ShapeName(document, bar);

                        View3D view = View3D.CreateIsometric(document, viewFamily);
                        view.Name = UniqueViewName(viewNames, SketchRules.ViewName(shape, index + 1));
                        view.SetSectionBox(Crop(box));

                        ViewSheet sheet = ViewSheet.Create(document, titleBlock);
                        sheet.Name = view.Name;

                        Viewport.Create(document, sheet.Id, view.Id, new XYZ(0, 0.5d, 0));
                        WriteNote(document, sheet, text.Note(shape, DiameterMillimeters(document, bar), LengthMillimeters(bar), bar.ScheduleMark ?? string.Empty));

                        made++;
                        sheetLines.Add(text.SheetLine(view.Name, sheet.SheetNumber));

                        log.Info(
                            "BarSketches: made the sheet " + sheet.SheetNumber + " for " + description + ".",
                            nameof(BarSketchesEngine));
                    }
                    catch (Exception exception)
                    {
                        refused++;
                        notes.Add(text.Refused(description, exception.Message));

                        log.Warn(
                            "BarSketches: " + description + " was refused: " + exception.Message,
                            nameof(BarSketchesEngine),
                            exception);
                    }
                }

                return made > 0;
            });

            log.Info(
                "BarSketches: " + made + " sheet(s) of " + bars.Count + " bar(s), " + skipped + " skipped, " +
                refused + " refused.",
                nameof(BarSketchesEngine));

            return new SketchOutcome(
                null,
                bars.Count,
                made,
                skipped,
                refused,
                bars.Count - planned,
                sheetLines,
                notes);
        }

        private static HashSet<string> ViewNames(Document document)
        {
            var names = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (View view in new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>())
            {
                names.Add(view.Name);
            }

            return names;
        }

        private static string UniqueViewName(HashSet<string> taken, string wanted)
        {
            string name = wanted;
            int suffix = 2;

            while (!taken.Add(name))
            {
                name = wanted + " (" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
                suffix++;
            }

            return name;
        }

        private static void WriteNote(Document document, ViewSheet sheet, string note)
        {
            ElementId textType = document.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

            if (textType == ElementId.InvalidElementId)
            {
                return;
            }

            TextNote.Create(document, sheet.Id, new XYZ(0, 0, 0), note, textType);
        }

        private static Autodesk.Revit.DB.BoundingBoxXYZ Crop(Autodesk.Revit.DB.BoundingBoxXYZ box)
        {
            double margin = Length.FromMillimeters(BoxMarginMillimeters).Feet;

            var crop = new Autodesk.Revit.DB.BoundingBoxXYZ
            {
                Min = new XYZ(box.Min.X - margin, box.Min.Y - margin, box.Min.Z - margin),
                Max = new XYZ(box.Max.X + margin, box.Max.Y + margin, box.Max.Z + margin),
            };

            return crop;
        }

        private static string ShapeName(Document document, Rebar bar)
        {
            try
            {
                ElementId shapeId = bar.GetShapeId();

                if (shapeId == null || shapeId == ElementId.InvalidElementId)
                {
                    return string.Empty;
                }

                return document.GetElement(shapeId)?.Name ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static double DiameterMillimeters(Document document, Rebar bar)
        {
            if (document.GetElement(bar.GetTypeId()) is RebarBarType type)
            {
                return RebarCompat.NominalDiameterMillimeters(type);
            }

            return 0;
        }

        private static double LengthMillimeters(Rebar bar)
        {
            try
            {
                return Length.FromFeet(bar.TotalLength).Millimeters;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static ElementId FirstTitleBlock(Document document)
        {
            foreach (FamilySymbol symbol in new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .Cast<FamilySymbol>())
            {
                return symbol.Id;
            }

            return ElementId.InvalidElementId;
        }

        private static ElementId FirstThreeDimensionalViewFamily(Document document)
        {
            foreach (ViewFamilyType type in new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>())
            {
                if (type.ViewFamily == ViewFamily.ThreeDimensional)
                {
                    return type.Id;
                }
            }

            return ElementId.InvalidElementId;
        }
    }
}
