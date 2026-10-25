using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Core.Piles;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// What one action did: the counts the report shows, the lines about what was left out
    /// and the refusals.
    /// </summary>
    internal sealed class PileDataOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many piles the run looked at.</summary>
        internal int Considered { get; set; }

        /// <summary>Gets or sets how many piles were written.</summary>
        internal int Written { get; set; }

        /// <summary>Gets or sets how many piles were skipped.</summary>
        internal int Skipped { get; set; }

        /// <summary>Gets or sets how many piles were refused.</summary>
        internal int Refused { get; set; }

        /// <summary>Gets the first lines about what was left out, one line each.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// The three actions of the tool: numbering, elevations of the raft bottom and the pile
    /// bottoms, and the cut height under the raft.
    /// <para>
    /// Every action writes text parameters only — the instance parameter first, the type
    /// parameter second — and is one undoable step of its own. The numbers come from
    /// <see cref="PileNumbering"/> and the cut heights from <see cref="CutHeightRules"/>.
    /// </para>
    /// </summary>
    internal static class PileDataEngine
    {
        /// <summary>How many lines about one kind are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Numbers the picked piles.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="piles">Picked piles.</param>
        /// <param name="settings">Start number, direction and the parameter name.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        internal static PileDataOutcome Number(
            Document document,
            IReadOnlyList<Element> piles,
            PileSettings settings,
            PileText text,
            Log log)
        {
            Guard(document, text, log);

            var outcome = new PileDataOutcome { Considered = piles.Count };

            if (piles.Count == 0)
            {
                outcome.Stopped = text.NoPiles;

                return outcome;
            }

            if (!ParameterNameRules.IsUsable(settings.NumberParameter))
            {
                outcome.Stopped = text.NumberParameterRequired;

                return outcome;
            }

            var centres = new List<Point3>(piles.Count);

            foreach (Element pile in piles)
            {
                centres.Add(LocationOf(pile));
            }

            IReadOnlyList<int> order = PileNumbering.Order(centres, settings.TopDown);

            DocumentEdit.Run(document, text.NumberTransactionName, () =>
            {
                bool changed = false;
                int number = settings.StartNumber;

                foreach (int index in order)
                {
                    Element pile = piles[index];
                    string label = ElementDescription.Describe(pile);

                    try
                    {
                        string value = number.ToString(CultureInfo.InvariantCulture);

                        if (ParameterSupport.TryWrite(pile, settings.NumberParameter, value))
                        {
                            outcome.Written++;
                            changed = true;
                        }
                        else
                        {
                            outcome.Refused++;
                            Add(outcome.Failures, text.NoTarget(label, settings.NumberParameter));
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Refused++;
                        Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }

                    number++;
                }

                return changed;
            });

            log.Info(
                "PileData/number: read " + outcome.Considered + ", numbered " + outcome.Written +
                ", refused " + outcome.Refused + ".",
                nameof(PileDataEngine));

            return outcome;
        }

        /// <summary>Writes the elevation of the raft bottom and of every pile bottom.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="piles">Picked piles.</param>
        /// <param name="slabs">Picked raft slabs.</param>
        /// <param name="settings">Parameter names.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        internal static PileDataOutcome Elevations(
            Document document,
            IReadOnlyList<Element> piles,
            IReadOnlyList<Element> slabs,
            PileSettings settings,
            PileText text,
            Log log)
        {
            Guard(document, text, log);

            var outcome = new PileDataOutcome { Considered = piles.Count };

            if (piles.Count == 0)
            {
                outcome.Stopped = text.NoPiles;

                return outcome;
            }

            double? slabBottom = LowestOf(document, slabs);

            if (slabBottom == null)
            {
                outcome.Stopped = text.NoSlabs;

                return outcome;
            }

            DocumentEdit.Run(document, text.ElevationTransactionName, () =>
            {
                bool changed = false;

                foreach (Element pile in piles)
                {
                    string label = ElementDescription.Describe(pile);

                    try
                    {
                        BoundingBox? measured = ModelGeometry.GetModelBounds(pile);

                        if (measured == null)
                        {
                            outcome.Skipped++;
                            Add(outcome.Notes, text.NoGeometry(label));

                            continue;
                        }

                        if (!ParameterSupport.CanWrite(pile, settings.SlabBottomParameter)
                            || !ParameterSupport.CanWrite(pile, settings.PileBottomParameter))
                        {
                            outcome.Refused++;
                            Add(outcome.Failures, text.NoTarget(label, settings.SlabBottomParameter));

                            continue;
                        }

                        bool slabWritten = ParameterSupport.TryWrite(
                            pile,
                            settings.SlabBottomParameter,
                            Millimeters(slabBottom.Value));

                        bool pileWritten = ParameterSupport.TryWrite(
                            pile,
                            settings.PileBottomParameter,
                            Millimeters(measured.Value.Min.Z));

                        if (slabWritten && pileWritten)
                        {
                            outcome.Written++;
                            changed = true;
                        }
                        else
                        {
                            outcome.Refused++;
                            Add(outcome.Failures, text.NoTarget(label, settings.PileBottomParameter));
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Refused++;
                        Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "PileData/elevations: read " + outcome.Considered + ", written " + outcome.Written +
                ", skipped " + outcome.Skipped + ", refused " + outcome.Refused + ".",
                nameof(PileDataEngine));

            return outcome;
        }

        /// <summary>Writes the height of the part of every pile cut off under the raft.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="piles">Picked piles.</param>
        /// <param name="slabs">Picked raft slabs.</param>
        /// <param name="settings">Parameter names.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        internal static PileDataOutcome CutHeights(
            Document document,
            IReadOnlyList<Element> piles,
            IReadOnlyList<Element> slabs,
            PileSettings settings,
            PileText text,
            Log log)
        {
            Guard(document, text, log);

            var outcome = new PileDataOutcome { Considered = piles.Count };

            if (piles.Count == 0)
            {
                outcome.Stopped = text.NoPiles;

                return outcome;
            }

            var slabBoxes = new List<BoundingBox>();

            foreach (Element slab in slabs)
            {
                BoundingBox? box = ModelGeometry.GetModelBounds(slab);

                if (box != null)
                {
                    slabBoxes.Add(box.Value);
                }
            }

            if (slabBoxes.Count == 0)
            {
                outcome.Stopped = text.NoSlabs;

                return outcome;
            }

            DocumentEdit.Run(document, text.CutTransactionName, () =>
            {
                bool changed = false;

                foreach (Element pile in piles)
                {
                    string label = ElementDescription.Describe(pile);

                    try
                    {
                        BoundingBox? measured = ModelGeometry.GetModelBounds(pile);

                        if (measured == null)
                        {
                            outcome.Skipped++;
                            Add(outcome.Notes, text.NoGeometry(label));

                            continue;
                        }

                        double? cut = null;

                        foreach (BoundingBox slabBox in slabBoxes)
                        {
                            double? candidate = CutHeightRules.CutHeight(measured.Value, slabBox);

                            if (candidate != null && (cut == null || candidate.Value < cut.Value))
                            {
                                cut = candidate;
                            }
                        }

                        if (cut == null)
                        {
                            outcome.Skipped++;
                            Add(outcome.Notes, text.NoSlabForPile(label));

                            continue;
                        }

                        if (!ParameterSupport.TryWrite(pile, settings.CutHeightParameter, Millimeters(cut.Value)))
                        {
                            outcome.Refused++;
                            Add(outcome.Failures, text.NoTarget(label, settings.CutHeightParameter));

                            continue;
                        }

                        outcome.Written++;
                        changed = true;
                    }
                    catch (Exception exception)
                    {
                        outcome.Refused++;
                        Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "PileData/cut: read " + outcome.Considered + ", written " + outcome.Written +
                ", skipped " + outcome.Skipped + ", refused " + outcome.Refused + ".",
                nameof(PileDataEngine));

            return outcome;
        }

        private static void Guard(Document document, PileText text, Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }
        }

        private static Point3 LocationOf(Element pile)
        {
            if (pile.Location is LocationPoint location && location.Point != null)
            {
                return ModelGeometry.ToPoint3(location.Point);
            }

            BoundingBox? box = ModelGeometry.GetModelBounds(pile);

            return box?.Center ?? Point3.Origin;
        }

        private static double? LowestOf(Document document, IReadOnlyList<Element> slabs)
        {
            double? lowest = null;

            foreach (Element slab in slabs)
            {
                BoundingBox? box = ModelGeometry.GetModelBounds(slab);

                if (box != null && (lowest == null || box.Value.Min.Z < lowest.Value))
                {
                    lowest = box.Value.Min.Z;
                }
            }

            return lowest;
        }

        private static string Millimeters(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void Add(List<string> lines, string line)
        {
            if (lines.Count < LineLimit)
            {
                lines.Add(line);
            }
        }
    }
}
