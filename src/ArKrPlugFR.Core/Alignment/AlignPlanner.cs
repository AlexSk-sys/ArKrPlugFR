using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Alignment
{
    /// <summary>Where a column should go, and what the rule found on the way.</summary>
    public readonly struct AlignTarget
    {
        /// <summary>Creates the outcome of planning one column.</summary>
        /// <param name="position">The target position, in millimetres.</param>
        /// <param name="lines">How many grid lines the target uses; zero when none was near.</param>
        /// <param name="moved">Whether the target differs from where the column is.</param>
        public AlignTarget(Point3 position, int lines, bool moved)
        {
            Position = position;
            Lines = lines;
            Moved = moved;
        }

        /// <summary>Gets the target position, in millimetres.</summary>
        public Point3 Position { get; }

        /// <summary>Gets how many grid lines the target uses; zero when none was near.</summary>
        public int Lines { get; }

        /// <summary>Gets a value indicating whether the column has to move.</summary>
        public bool Moved { get; }
    }

    /// <summary>
    /// The rule that pulls a column onto its grids.
    /// <para>
    /// A column belongs where its own grids cross: the nearest line within the capture
    /// distance gives one coordinate, the nearest line of another direction gives the
    /// second, and the crossing of the two is the target. With only one line in reach the
    /// column lands on its foot; with none it stays where it is.
    /// </para>
    /// </summary>
    public static class AlignPlanner
    {
        /// <summary>Movements smaller than this are no movements at all, in millimetres.</summary>
        public const double MovementEpsilonMillimeters = 0.1d;

        /// <summary>Works out where one column should go.</summary>
        /// <param name="center">Centre of the column, in millimetres.</param>
        /// <param name="lines">The grid lines to align to, in millimetres.</param>
        /// <param name="capture">How far a line may lie and still be considered; must not be negative.</param>
        /// <returns>The target, how many lines it uses, and whether the column has to move.</returns>
        public static AlignTarget Plan(Point3 center, IReadOnlyList<GridLine> lines, Length capture)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (capture.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capture),
                    capture,
                    "The capture distance cannot be negative.");
            }

            double reach = capture.Millimeters;

            GridLine? nearest = null;
            double nearestDistance = 0d;

            foreach (GridLine line in lines)
            {
                double distance = GridSnap.Distance(center, line);

                if (distance <= reach && (nearest == null || distance < nearestDistance))
                {
                    nearest = line;
                    nearestDistance = distance;
                }
            }

            if (nearest == null)
            {
                return new AlignTarget(center, 0, false);
            }

            GridLine? second = null;
            double secondDistance = 0d;

            foreach (GridLine line in lines)
            {
                if (GridSnap.IsParallel(nearest.Value, line))
                {
                    continue;
                }

                double distance = GridSnap.Distance(center, line);

                if (distance <= reach && (second == null || distance < secondDistance))
                {
                    second = line;
                    secondDistance = distance;
                }
            }

            Point3 target;
            int count;

            if (second != null && GridSnap.Intersection(nearest.Value, second.Value) is Point3 crossing)
            {
                target = crossing;
                count = 2;
            }
            else
            {
                target = GridSnap.Foot(center, nearest.Value);
                count = 1;
            }

            target = new Point3(target.X, target.Y, center.Z);

            bool moved = Math.Abs(center.X - target.X) > MovementEpsilonMillimeters
                || Math.Abs(center.Y - target.Y) > MovementEpsilonMillimeters;

            return new AlignTarget(target, count, moved);
        }
    }
}
