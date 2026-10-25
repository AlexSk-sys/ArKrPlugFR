using System;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Alignment
{
    /// <summary>
    /// The geometry of bringing analytical elements in line with a target.
    /// <para>
    /// Two rules do the work: the vertical offset that lifts an element into the plane of
    /// the target — measured at the crossing of the two lines in plan, or between their
    /// midpoints when they do not cross — and the point of the target line nearest to a
    /// node. Both are plain geometry in millimetres and are tested without Revit.
    /// </para>
    /// </summary>
    public static class AlignmentRules
    {
        /// <summary>
        /// Works out the vertical offset that brings an element into the plane of the target.
        /// </summary>
        /// <param name="target">The line of the target element.</param>
        /// <param name="current">The line of the element to move.</param>
        /// <returns>The offset along the vertical axis, in millimetres.</returns>
        public static double VerticalOffset(Segment3 target, Segment3 current)
        {
            if (TryCrossing(target, current, out double targetHeight, out double currentHeight))
            {
                return targetHeight - currentHeight;
            }

            double targetMiddle = (target.Start.Z + target.End.Z) / 2d;
            double currentMiddle = (current.Start.Z + current.End.Z) / 2d;

            return targetMiddle - currentMiddle;
        }

        /// <summary>Returns the point of a line nearest to a point, never past either end.</summary>
        /// <param name="point">The point to bring onto the line.</param>
        /// <param name="line">The line to bring it onto.</param>
        /// <returns>The point of the line nearest to the given point.</returns>
        public static Point3 ProjectOntoLine(Point3 point, Segment3 line)
        {
            double dx = line.End.X - line.Start.X;
            double dy = line.End.Y - line.Start.Y;
            double dz = line.End.Z - line.Start.Z;
            double lengthSquared = (dx * dx) + (dy * dy) + (dz * dz);

            if (lengthSquared <= 0d)
            {
                return line.Start;
            }

            double along = (((point.X - line.Start.X) * dx) +
                            ((point.Y - line.Start.Y) * dy) +
                            ((point.Z - line.Start.Z) * dz)) / lengthSquared;

            if (along < 0d)
            {
                along = 0d;
            }
            else if (along > 1d)
            {
                along = 1d;
            }

            return new Point3(
                line.Start.X + (along * dx),
                line.Start.Y + (along * dy),
                line.Start.Z + (along * dz));
        }

        /// <summary>Determines whether one of the ends of a segment sits at a point.</summary>
        /// <param name="segment">The segment to look at.</param>
        /// <param name="point">The point to look for.</param>
        /// <param name="toleranceMillimeters">How far an end may lie from the point and still count.</param>
        /// <returns><c>true</c> when one of the ends is at the point.</returns>
        public static bool HasEndpointAt(Segment3 segment, Point3 point, double toleranceMillimeters)
        {
            if (toleranceMillimeters < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(toleranceMillimeters),
                    toleranceMillimeters,
                    "The tolerance cannot be negative.");
            }

            return segment.Start.DistanceTo(point) <= toleranceMillimeters
                || segment.End.DistanceTo(point) <= toleranceMillimeters;
        }

        private static bool TryCrossing(
            Segment3 target,
            Segment3 current,
            out double targetHeight,
            out double currentHeight)
        {
            targetHeight = 0d;
            currentHeight = 0d;

            double targetDx = target.End.X - target.Start.X;
            double targetDy = target.End.Y - target.Start.Y;
            double currentDx = current.End.X - current.Start.X;
            double currentDy = current.End.Y - current.Start.Y;

            double determinant = (targetDx * currentDy) - (targetDy * currentDx);

            if (Math.Abs(determinant) < 1e-9)
            {
                // The two lines run parallel in plan; the midpoint rule decides.
                return false;
            }

            double offsetX = current.Start.X - target.Start.X;
            double offsetY = current.Start.Y - target.Start.Y;

            double alongTarget = ((offsetX * currentDy) - (offsetY * currentDx)) / determinant;
            double alongCurrent = ((offsetX * targetDy) - (offsetY * targetDx)) / determinant;

            if (alongTarget < 0d || alongTarget > 1d || alongCurrent < 0d || alongCurrent > 1d)
            {
                // The lines cross beyond their ends; the midpoint rule decides.
                return false;
            }

            targetHeight = target.Start.Z + (alongTarget * (target.End.Z - target.Start.Z));
            currentHeight = current.Start.Z + (alongCurrent * (current.End.Z - current.Start.Z));

            return true;
        }
    }
}
