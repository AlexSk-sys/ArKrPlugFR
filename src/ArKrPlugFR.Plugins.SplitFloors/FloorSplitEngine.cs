using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Floors;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SplitFloors
{
    /// <summary>
    /// What one run did: the counts the report shows, the lines about what was skipped and
    /// the refusals.
    /// </summary>
    internal sealed class FloorSplitOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many floors the run looked at.</summary>
        internal int Considered { get; set; }

        /// <summary>Gets or sets how many floors were split.</summary>
        internal int Split { get; set; }

        /// <summary>Gets or sets how many floors the run created.</summary>
        internal int Created { get; set; }

        /// <summary>Gets or sets how many floors were skipped as nothing to split.</summary>
        internal int Skipped { get; set; }

        /// <summary>Gets or sets how many floors were refused.</summary>
        internal int Failed { get; set; }

        /// <summary>Gets the lines about the contours and floors that were left out.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Reads the contours of the picked floors, decides their parts by the planar rules
    /// and writes the split, one undoable step per floor.
    /// <para>
    /// The decision itself is made in <see cref="FloorLoopRules"/>; this class only reads
    /// the contours out of the Revit geometry, creates a floor per part and replaces the
    /// source. A floor that cannot be split rolls its own step back and is reported, and
    /// the other floors continue.
    /// </para>
    /// </summary>
    internal static class FloorSplitEngine
    {
        /// <summary>Splits the picked floors.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="floors">Floors to split.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static FloorSplitOutcome Split(
            Document document,
            IReadOnlyList<Floor> floors,
            SplitText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (floors == null)
            {
                throw new ArgumentNullException(nameof(floors));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new FloorSplitOutcome();
            var jobs = new List<SplitJob>();

            foreach (Floor floor in floors)
            {
                outcome.Considered++;

                string label = ElementDescription.Describe(floor);

                try
                {
                    IReadOnlyList<CurveLoop>? loops = FloorSupport.ReadTopLoops(floor);

                    if (loops == null)
                    {
                        outcome.Skipped++;
                        outcome.Notes.Add(text.NoTopFace(label));

                        continue;
                    }

                    var contours = new List<FloorContour>(loops.Count);

                    foreach (CurveLoop loop in loops)
                    {
                        contours.Add(new FloorContour(FloorSupport.ToPoints(loop)));
                    }

                    FloorLoopPlan plan = FloorLoopRules.Classify(contours);

                    if (plan.HasRejectedContours)
                    {
                        outcome.Notes.Add(text.RejectedContours(
                            label,
                            plan.Degenerate.Count,
                            plan.Nested.Count,
                            plan.Duplicates.Count));
                    }

                    if (plan.Parts.Count < 2)
                    {
                        outcome.Skipped++;
                        outcome.Notes.Add(text.SingleContour(label));

                        continue;
                    }

                    jobs.Add(new SplitJob(floor, loops, plan));
                }
                catch (Exception exception)
                {
                    outcome.Failed++;
                    outcome.Failures.Add(text.SplitFailed(label, exception.Message));
                }
            }

            foreach (SplitJob job in jobs)
            {
                bool committed;

                try
                {
                    committed = DocumentEdit.Run(document, text.TransactionName, () => SplitOne(document, job, text));
                }
                catch (Exception exception)
                {
                    outcome.Failed++;
                    outcome.Failures.Add(text.SplitFailed(ElementDescription.Describe(job.Floor), exception.Message));

                    continue;
                }

                if (committed)
                {
                    outcome.Split++;
                    outcome.Created += job.Plan.Parts.Count;
                }
            }

            log.Info(
                "SplitFloors: read " + outcome.Considered + ", split " + outcome.Split +
                ", created " + outcome.Created + ", skipped " + outcome.Skipped +
                ", refused " + outcome.Failed + ".",
                nameof(FloorSplitEngine));

            return outcome;
        }

        private static bool SplitOne(Document document, SplitJob job, SplitText text)
        {
            foreach (FloorPart part in job.Plan.Parts)
            {
                var profile = new List<CurveLoop> { job.Loops[part.Outer] };

                foreach (int hole in part.Holes)
                {
                    profile.Add(job.Loops[hole]);
                }

                FloorSupport.CreateLike(document, profile, job.Floor);
            }

            ICollection<ElementId> removed = document.Delete(job.Floor.Id);

            if (removed == null || removed.Count == 0)
            {
                throw new InvalidOperationException(text.DeleteRefused);
            }

            return true;
        }

        private sealed class SplitJob
        {
            internal SplitJob(Floor floor, IReadOnlyList<CurveLoop> loops, FloorLoopPlan plan)
            {
                Floor = floor;
                Loops = loops;
                Plan = plan;
            }

            internal Floor Floor { get; }

            internal IReadOnlyList<CurveLoop> Loops { get; }

            internal FloorLoopPlan Plan { get; }
        }
    }
}
