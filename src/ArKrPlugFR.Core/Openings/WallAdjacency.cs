using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Openings
{
    /// <summary>
    /// The rules that decide whether one wall lines another.
    /// <para>
    /// A wall lines another when it runs the same way and stands close enough to it. The
    /// two may run the same way round or opposite: a partition can line a bearing wall
    /// from either side, and which of the two ends its location curve starts from is not
    /// a property the draughtsman controls.
    /// </para>
    /// </summary>
    public static class WallAdjacency
    {
        /// <summary>Widest gap between a wall and the wall it lines, in millimetres.</summary>
        public const double DefaultGapMillimeters = 150d;

        /// <summary>How far two directions may differ and still count as parallel, in degrees.</summary>
        public const double DefaultAngleToleranceDegrees = 1d;

        /// <summary>Gets the gap within which a wall counts as lining another.</summary>
        public static Length DefaultGap => Length.FromMillimeters(DefaultGapMillimeters);

        /// <summary>
        /// Tells whether a wall runs the same way as another, within the allowed angle.
        /// </summary>
        /// <param name="direction">Direction of the first wall, in millimetres.</param>
        /// <param name="other">Direction of the second wall, in millimetres.</param>
        /// <param name="angleToleranceDegrees">Widest angle still counted as parallel.</param>
        /// <returns><c>true</c> when the two run alike.</returns>
        public static bool RunsAlike(
            Point3 direction,
            Point3 other,
            double angleToleranceDegrees = DefaultAngleToleranceDegrees)
        {
            double length = Length2D(direction);
            double otherLength = Length2D(other);

            if (length <= 0d || otherLength <= 0d)
            {
                return false;
            }

            double cross = ((direction.X / length) * (other.Y / otherLength))
                - ((direction.Y / length) * (other.X / otherLength));

            // Either way round counts, so only the size of the cross product matters.
            double limit = Math.Sin(angleToleranceDegrees * Math.PI / 180d);

            return Math.Abs(cross) <= limit;
        }

        /// <summary>
        /// Reads how far a point stands from the line of a wall, across it.
        /// </summary>
        /// <param name="basePoint">A point of the wall's own line, in millimetres.</param>
        /// <param name="direction">Direction of the wall, in millimetres; not necessarily a unit vector.</param>
        /// <param name="otherBasePoint">Point to measure, in millimetres.</param>
        /// <returns>The distance across the wall.</returns>
        public static Length Gap(Point3 basePoint, Point3 direction, Point3 otherBasePoint)
        {
            double length = Length2D(direction);

            if (length <= 0d)
            {
                throw new ArgumentException(
                    "The direction of the wall must not be a single point.",
                    nameof(direction));
            }

            double offsetX = otherBasePoint.X - basePoint.X;
            double offsetY = otherBasePoint.Y - basePoint.Y;

            // What is left of the offset once the part along the wall is taken away.
            double across = ((offsetX * -direction.Y) + (offsetY * direction.X)) / length;

            return Length.FromMillimeters(Math.Abs(across));
        }

        /// <summary>
        /// Tells whether one wall lines another: running alike and standing within the gap.
        /// </summary>
        /// <param name="basePoint">A point of the first wall's line, in millimetres.</param>
        /// <param name="direction">Direction of the first wall, in millimetres.</param>
        /// <param name="otherBasePoint">A point of the second wall's line, in millimetres.</param>
        /// <param name="otherDirection">Direction of the second wall, in millimetres.</param>
        /// <param name="gap">Widest distance across still counted as lining.</param>
        /// <param name="angleToleranceDegrees">Widest angle still counted as parallel.</param>
        /// <returns><c>true</c> when the second wall lines the first.</returns>
        public static bool Lines(
            Point3 basePoint,
            Point3 direction,
            Point3 otherBasePoint,
            Point3 otherDirection,
            Length gap,
            double angleToleranceDegrees = DefaultAngleToleranceDegrees)
        {
            return RunsAlike(direction, otherDirection, angleToleranceDegrees)
                && Gap(basePoint, direction, otherBasePoint) <= gap;
        }

        private static double Length2D(Point3 vector)
        {
            return Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
        }
    }
}
