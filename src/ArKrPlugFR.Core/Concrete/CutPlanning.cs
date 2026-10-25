using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Concrete
{
    /// <summary>
    /// Turns a crossing of a service and a concrete element into the rectangle of an
    /// opening through it.
    /// <para>
    /// An opening based on a direction of passage is measured across that direction:
    /// the crossing's extents on the other two axes are grown by the clearance and then
    /// clamped to the cross-section of the element, so an opening never reaches past the
    /// concrete it is cut in.
    /// </para>
    /// </summary>
    public static class CutPlanning
    {
        /// <summary>
        /// Reads the crossing across one direction of passage: the extents of the box on
        /// the two axes other than the axis of the run.
        /// </summary>
        /// <param name="crossing">Box of the crossing, in millimetres.</param>
        /// <param name="axis">Axis the opening runs along.</param>
        /// <returns>The crossing measured across the run.</returns>
        public static AxisRectangle Profile(BoundingBox crossing, CutAxis axis)
        {
            switch (axis)
            {
                case CutAxis.X:
                    return new AxisRectangle(crossing.Min.Y, crossing.Max.Y, crossing.Min.Z, crossing.Max.Z);
                case CutAxis.Y:
                    return new AxisRectangle(crossing.Min.X, crossing.Max.X, crossing.Min.Z, crossing.Max.Z);
                default:
                    return new AxisRectangle(crossing.Min.X, crossing.Max.X, crossing.Min.Y, crossing.Max.Y);
            }
        }

        /// <summary>
        /// Reads the cross-section of an element across the same direction of passage; an
        /// opening is clamped to this rectangle.
        /// </summary>
        /// <param name="host">Box of the element, in millimetres.</param>
        /// <param name="axis">Axis the opening runs along.</param>
        /// <returns>The cross-section of the element.</returns>
        public static AxisRectangle Face(BoundingBox host, CutAxis axis)
        {
            return Profile(host, axis);
        }

        /// <summary>Measures an element along one direction of passage.</summary>
        /// <param name="box">Box of the element, in millimetres.</param>
        /// <param name="axis">Axis to measure along.</param>
        /// <returns>The extent along the axis, in millimetres.</returns>
        public static double ExtentAlong(BoundingBox box, CutAxis axis)
        {
            switch (axis)
            {
                case CutAxis.X:
                    return box.Width.Millimeters;
                case CutAxis.Y:
                    return box.Depth.Millimeters;
                default:
                    return box.Height.Millimeters;
            }
        }

        /// <summary>Reads the middle of an element along one axis; an opening is based on that plane.</summary>
        /// <param name="box">Box of the element, in millimetres.</param>
        /// <param name="axis">Axis the opening runs along.</param>
        /// <returns>The coordinate of the middle along the axis, in millimetres.</returns>
        public static double CenterAlong(BoundingBox box, CutAxis axis)
        {
            switch (axis)
            {
                case CutAxis.X:
                    return (box.Min.X + box.Max.X) / 2d;
                case CutAxis.Y:
                    return (box.Min.Y + box.Max.Y) / 2d;
                default:
                    return (box.Min.Z + box.Max.Z) / 2d;
            }
        }

        /// <summary>Adds the clearance on all four sides of a crossing.</summary>
        /// <param name="crossing">The crossing measured across the run, in millimetres.</param>
        /// <param name="clearance">Clearance added to each side; a negative value is read as none.</param>
        /// <returns>The rectangle of the opening.</returns>
        public static AxisRectangle WithClearance(AxisRectangle crossing, Length clearance)
        {
            double amount = clearance.Millimeters;

            if (amount < 0d)
            {
                amount = 0d;
            }

            return new AxisRectangle(
                crossing.MinU - amount,
                crossing.MaxU + amount,
                crossing.MinV - amount,
                crossing.MaxV + amount);
        }

        /// <summary>
        /// Clamps a rectangle to the cross-section of the element it is cut in, so an
        /// opening never reaches past the concrete on any side. A rectangle that lies
        /// entirely outside the cross-section collapses onto its nearest edge and is
        /// reported as unusable by <see cref="IsUsable"/>.
        /// </summary>
        /// <param name="rectangle">Rectangle of the opening, in millimetres.</param>
        /// <param name="face">Cross-section of the element, in millimetres.</param>
        /// <returns>The rectangle shortened into the cross-section.</returns>
        public static AxisRectangle Fit(AxisRectangle rectangle, AxisRectangle face)
        {
            double minU = Clamp(rectangle.MinU, rectangle.MaxU, face.MinU, face.MaxU, out double maxU);
            double minV = Clamp(rectangle.MinV, rectangle.MaxV, face.MinV, face.MaxV, out double maxV);

            return new AxisRectangle(minU, maxU, minV, maxV);
        }

        private static double Clamp(double min, double max, double faceMin, double faceMax, out double clampedMax)
        {
            if (max < faceMin)
            {
                clampedMax = faceMin;

                return faceMin;
            }

            if (min > faceMax)
            {
                clampedMax = faceMax;

                return faceMax;
            }

            clampedMax = Math.Min(max, faceMax);

            return Math.Max(min, faceMin);
        }

        /// <summary>
        /// Tells whether two rectangles cover the same place and the same size, within
        /// the tolerance on every edge.
        /// </summary>
        /// <param name="first">First rectangle, in millimetres.</param>
        /// <param name="second">Second rectangle, in millimetres.</param>
        /// <param name="tolerance">Widest difference of an edge still counted as the same.</param>
        /// <returns><c>true</c> when the two describe the same opening.</returns>
        public static bool Coincides(AxisRectangle first, AxisRectangle second, Length tolerance)
        {
            double slack = tolerance.Millimeters;

            if (slack < 0d)
            {
                slack = 0d;
            }

            return Math.Abs(first.MinU - second.MinU) <= slack
                && Math.Abs(first.MaxU - second.MaxU) <= slack
                && Math.Abs(first.MinV - second.MinV) <= slack
                && Math.Abs(first.MaxV - second.MaxV) <= slack;
        }

        /// <summary>
        /// Tells whether a rectangle is worth cutting: both of its edges reach the
        /// shortest edge an opening may have.
        /// </summary>
        /// <param name="rectangle">The rectangle of the opening.</param>
        /// <param name="minimum">Shortest edge allowed.</param>
        /// <returns><c>true</c> when the opening can be cut.</returns>
        public static bool IsUsable(AxisRectangle rectangle, Length minimum)
        {
            return rectangle.Width >= minimum && rectangle.Height >= minimum;
        }

        /// <summary>
        /// Projects a box onto a direction and returns the width of the shadow it casts;
        /// a service crosses a wall only when its crossing covers the wall's thickness
        /// measured along the wall's normal.
        /// </summary>
        /// <param name="box">Box to project, in millimetres.</param>
        /// <param name="direction">Direction to project onto; it does not have to be a unit vector.</param>
        /// <returns>The width of the box along the direction, in millimetres.</returns>
        public static double ProjectedExtent(BoundingBox box, Point3 direction)
        {
            double length = Math.Sqrt(
                (direction.X * direction.X) + (direction.Y * direction.Y) + (direction.Z * direction.Z));

            if (length <= 0d)
            {
                throw new ArgumentException("The direction must not be a single point.", nameof(direction));
            }

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (Point3 corner in Corners(box))
            {
                double projection =
                    ((corner.X * direction.X) + (corner.Y * direction.Y) + (corner.Z * direction.Z)) / length;

                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            return max - min;
        }

        private static IEnumerable<Point3> Corners(BoundingBox box)
        {
            foreach (double x in new[] { box.Min.X, box.Max.X })
            {
                foreach (double y in new[] { box.Min.Y, box.Max.Y })
                {
                    foreach (double z in new[] { box.Min.Z, box.Max.Z })
                    {
                        yield return new Point3(x, y, z);
                    }
                }
            }
        }
    }
}
