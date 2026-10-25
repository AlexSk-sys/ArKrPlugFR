using System;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Alignment
{
    /// <summary>
    /// The geometry of snapping to a grid: how far a point lies from a line, where its
    /// foot lies, and where two lines cross.
    /// <para>
    /// Everything happens in the horizontal plane; the vertical coordinate travels
    /// through untouched, because aligning a column to a grid never changes its height.
    /// </para>
    /// </summary>
    public static class GridSnap
    {
        /// <summary>How parallel two directions may be and still count as parallel.</summary>
        private const double ParallelTolerance = 1e-6d;

        /// <summary>Measures how far a point lies from a line, across its direction.</summary>
        /// <param name="point">The point, in millimetres.</param>
        /// <param name="line">The line, in millimetres.</param>
        /// <returns>The perpendicular distance, in millimetres.</returns>
        public static double Distance(Point3 point, GridLine line)
        {
            double dx = line.Direction.X;
            double dy = line.Direction.Y;
            double length = Math.Sqrt((dx * dx) + (dy * dy));

            double cross = (dx * (point.Y - line.Origin.Y)) - (dy * (point.X - line.Origin.X));

            return Math.Abs(cross) / length;
        }

        /// <summary>Projects a point onto a line.</summary>
        /// <param name="point">The point, in millimetres.</param>
        /// <param name="line">The line, in millimetres.</param>
        /// <returns>The foot of the perpendicular, with the point's own height.</returns>
        public static Point3 Foot(Point3 point, GridLine line)
        {
            double dx = line.Direction.X;
            double dy = line.Direction.Y;
            double square = (dx * dx) + (dy * dy);

            double t = (((point.X - line.Origin.X) * dx) + ((point.Y - line.Origin.Y) * dy)) / square;

            return new Point3(
                line.Origin.X + (t * dx),
                line.Origin.Y + (t * dy),
                point.Z);
        }

        /// <summary>Tells whether two grid lines run in the same direction.</summary>
        /// <param name="first">First line.</param>
        /// <param name="second">Second line.</param>
        /// <returns><c>true</c> when the directions differ by less than a millionth of the sine.</returns>
        public static bool IsParallel(GridLine first, GridLine second)
        {
            double firstLength = Math.Sqrt(
                (first.Direction.X * first.Direction.X) + (first.Direction.Y * first.Direction.Y));
            double secondLength = Math.Sqrt(
                (second.Direction.X * second.Direction.X) + (second.Direction.Y * second.Direction.Y));

            double cross =
                ((first.Direction.X / firstLength) * (second.Direction.Y / secondLength))
                - ((first.Direction.Y / firstLength) * (second.Direction.X / secondLength));

            return Math.Abs(cross) <= ParallelTolerance;
        }

        /// <summary>Finds where two grid lines cross.</summary>
        /// <param name="first">First line.</param>
        /// <param name="second">Second line.</param>
        /// <returns>The crossing in the horizontal plane, or <c>null</c> for parallel lines.</returns>
        public static Point3? Intersection(GridLine first, GridLine second)
        {
            if (IsParallel(first, second))
            {
                return null;
            }

            double d1x = first.Direction.X;
            double d1y = first.Direction.Y;
            double d2x = second.Direction.X;
            double d2y = second.Direction.Y;

            double denominator = (d1x * d2y) - (d1y * d2x);
            double t = (((second.Origin.X - first.Origin.X) * d2y) - ((second.Origin.Y - first.Origin.Y) * d2x))
                / denominator;

            return new Point3(first.Origin.X + (t * d1x), first.Origin.Y + (t * d1y), 0d);
        }
    }
}
