using System;
using ArKrPlugFR.Core.Geometry;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads a wall as a frame — a base point and a direction in the project's horizontal
    /// plane — and turns a rectangle in that frame back into the points the platform's
    /// opening commands expect.
    /// <para>
    /// Openings are measured along a wall and above its base point, which is the only
    /// reading that stays true whichever way the wall runs; every length crossing this
    /// boundary is in millimetres on the domain side and in feet on the platform side.
    /// </para>
    /// </summary>
    public static class WallFrame
    {
        /// <summary>
        /// Reads the frame of a straight wall: a point of its line and the direction it
        /// runs in, both in millimetres.
        /// </summary>
        /// <param name="wall">Wall to read.</param>
        /// <param name="basePoint">Point of the wall's own line, in millimetres.</param>
        /// <param name="direction">Direction of the wall in the horizontal plane, in millimetres.</param>
        /// <returns><c>false</c> for a wall whose line is not straight.</returns>
        public static bool TryRead(Wall wall, out Point3 basePoint, out Point3 direction)
        {
            if (wall == null)
            {
                throw new ArgumentNullException(nameof(wall));
            }

            basePoint = Point3.Origin;
            direction = new Point3(1d, 0d, 0d);

            if (wall.Location is not LocationCurve location || location.Curve is not Line line)
            {
                return false;
            }

            basePoint = ModelGeometry.ToPoint3(line.Origin);
            direction = new Point3(line.Direction.X, line.Direction.Y, 0d);

            return true;
        }

        /// <summary>
        /// Measures how far a point lies along a wall, from the wall's base point.
        /// </summary>
        /// <param name="basePoint">Point of the wall's line, in millimetres.</param>
        /// <param name="direction">Direction of the wall, in millimetres; not necessarily a unit vector.</param>
        /// <param name="point">Point to measure, in millimetres.</param>
        /// <returns>The distance along the wall; negative before its base point.</returns>
        public static double Along(Point3 basePoint, Point3 direction, Point3 point)
        {
            double length = UnitLength(direction);

            return (((point.X - basePoint.X) * direction.X) + ((point.Y - basePoint.Y) * direction.Y)) / length;
        }

        /// <summary>
        /// Builds the point of a wall that lies a given distance along it and a given
        /// height above its base point.
        /// </summary>
        /// <param name="basePoint">Point of the wall's line, in millimetres.</param>
        /// <param name="direction">Direction of the wall, in millimetres.</param>
        /// <param name="alongMillimeters">Distance along the wall, in millimetres.</param>
        /// <param name="heightMillimeters">Height above the wall's base point, in millimetres.</param>
        /// <returns>The point, in the platform's feet.</returns>
        public static XYZ PointAt(
            Point3 basePoint,
            Point3 direction,
            double alongMillimeters,
            double heightMillimeters)
        {
            double length = UnitLength(direction);
            double unitX = direction.X / length;
            double unitY = direction.Y / length;

            return ModelGeometry.ToXyz(new Point3(
                basePoint.X + (unitX * alongMillimeters),
                basePoint.Y + (unitY * alongMillimeters),
                basePoint.Z + heightMillimeters));
        }

        /// <summary>Measures a direction in the horizontal plane.</summary>
        /// <param name="direction">Direction to measure.</param>
        /// <returns>Its length in millimetres; zero for a direction that is a single point.</returns>
        public static double LengthOf(Point3 direction)
        {
            return Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        }

        private static double UnitLength(Point3 direction)
        {
            double length = LengthOf(direction);

            if (length <= 0d)
            {
                throw new ArgumentException(
                    "The direction of the wall must not be a single point.",
                    nameof(direction));
            }

            return length;
        }
    }
}
