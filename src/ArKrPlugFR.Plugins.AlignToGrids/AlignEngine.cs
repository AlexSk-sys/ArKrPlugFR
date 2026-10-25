using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Alignment;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>What one run did.</summary>
    internal sealed class AlignOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        /// <param name="columns">Columns that were read.</param>
        /// <param name="moved">Columns that were moved.</param>
        /// <param name="alreadyAligned">Columns already on a grid.</param>
        /// <param name="inGroup">Columns skipped as group members.</param>
        /// <param name="noGrid">Columns without a grid in reach.</param>
        /// <param name="failed">Refusals.</param>
        /// <param name="failures">One line per refusal.</param>
        /// <param name="stopped">Why the run did not start, or <c>null</c> when it ran.</param>
        internal AlignOutcome(
            int columns,
            int moved,
            int alreadyAligned,
            int inGroup,
            int noGrid,
            int failed,
            IReadOnlyList<string> failures,
            string? stopped)
        {
            Columns = columns;
            Moved = moved;
            AlreadyAligned = alreadyAligned;
            InGroup = inGroup;
            NoGrid = noGrid;
            Failed = failed;
            Failures = failures ?? Array.Empty<string>();
            Stopped = stopped;
        }

        /// <summary>Gets the number of columns that were read.</summary>
        internal int Columns { get; }

        /// <summary>Gets the number of columns that were moved.</summary>
        internal int Moved { get; }

        /// <summary>Gets the number of columns already on a grid.</summary>
        internal int AlreadyAligned { get; }

        /// <summary>Gets the number of columns skipped as group members.</summary>
        internal int InGroup { get; }

        /// <summary>Gets the number of columns without a grid in reach.</summary>
        internal int NoGrid { get; }

        /// <summary>Gets the number of refusals.</summary>
        internal int Failed { get; }

        /// <summary>Gets one line per refusal.</summary>
        internal IReadOnlyList<string> Failures { get; }

        /// <summary>Gets why the run did not start, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; }
    }

    /// <summary>
    /// Pulls the picked columns onto the picked grids.
    /// <para>
    /// Grids become lines of the horizontal plane, columns become their location points,
    /// and the rule of the domain decides where each column belongs. The columns move in
    /// one transaction; the grids are never touched, and neither is anything else.
    /// </para>
    /// </summary>
    internal static class AlignEngine
    {
        /// <summary>Aligns the columns and returns what the run did.</summary>
        /// <param name="document">Project the columns are moved in.</param>
        /// <param name="columns">Columns the user picked.</param>
        /// <param name="grids">Grids the user picked.</param>
        /// <param name="capture">How close a grid has to be, in millimetres.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static AlignOutcome Align(
            Document document,
            IReadOnlyList<FamilyInstance> columns,
            IReadOnlyList<Grid> grids,
            Length capture,
            AlignText text,
            Log log)
        {
            var failures = new List<string>();

            List<GridLine> lines = ReadLines(grids, text, failures);

            if (columns.Count == 0)
            {
                return new AlignOutcome(0, 0, 0, 0, 0, failures.Count, failures, text.NoColumns);
            }

            if (lines.Count == 0)
            {
                return new AlignOutcome(columns.Count, 0, 0, 0, 0, failures.Count, failures, text.NoGrids);
            }

            var planned = new List<(FamilyInstance Column, Point3 Target)>();
            int alreadyAligned = 0;
            int inGroup = 0;
            int noGrid = 0;

            foreach (FamilyInstance column in columns)
            {
                if (column.GroupId != ElementId.InvalidElementId)
                {
                    inGroup++;
                    continue;
                }

                if (column.Location is not LocationPoint location)
                {
                    failures.Add(text.NotPointBased(ElementDescription.Describe(column)));
                    continue;
                }

                Point3 center = ModelGeometry.ToPoint3(location.Point);
                AlignTarget target = AlignPlanner.Plan(center, lines, capture);

                if (target.Lines == 0)
                {
                    noGrid++;
                    continue;
                }

                if (!target.Moved)
                {
                    alreadyAligned++;
                    continue;
                }

                planned.Add((column, target.Position));
            }

            int moved = 0;
            int failed = failures.Count;

            if (planned.Count > 0)
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    foreach ((FamilyInstance column, Point3 target) in planned)
                    {
                        try
                        {
                            if (column.Location is LocationPoint location)
                            {
                                location.Point = ModelGeometry.ToXyz(target);
                                moved++;

                                log.Info(
                                    "AlignToGrids: moved " + ElementDescription.Describe(column) +
                                    " to " + target + ".",
                                    nameof(AlignEngine));
                            }
                        }
                        catch (Exception exception)
                        {
                            failed++;

                            failures.Add(text.MoveFailed(
                                ElementDescription.Describe(column),
                                exception.Message));

                            log.Warn(
                                "AlignToGrids: " + ElementDescription.Describe(column) +
                                " refused to move: " + exception.Message,
                                nameof(AlignEngine),
                                exception);
                        }
                    }

                    return moved > 0;
                });
            }

            log.Info(
                "AlignToGrids: " + columns.Count + " column(s), " + moved + " moved, " +
                alreadyAligned + " already on a grid, " + noGrid + " without a grid, " +
                inGroup + " in groups, " + failed + " refused.",
                nameof(AlignEngine));

            return new AlignOutcome(columns.Count, moved, alreadyAligned, inGroup, noGrid, failed, failures, null);
        }

        private static List<GridLine> ReadLines(
            IReadOnlyList<Grid> grids,
            AlignText text,
            List<string> failures)
        {
            var lines = new List<GridLine>();

            foreach (Grid grid in grids)
            {
                Curve? curve = grid.Curve;

                if (curve is not Line line)
                {
                    failures.Add(text.GridNotStraight(grid.Name));
                    continue;
                }

                Point3 origin = ModelGeometry.ToPoint3(line.GetEndPoint(0));
                Point3 direction = new Point3(line.Direction.X, line.Direction.Y, 0d);
                double flat = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));

                if (flat <= 0d)
                {
                    failures.Add(text.GridNotInPlane(grid.Name));
                    continue;
                }

                lines.Add(new GridLine(origin, direction));
            }

            return lines;
        }
    }
}
