using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Openings
{
    /// <summary>
    /// Rules that turn the place where a service crosses a wall into the rectangle of an
    /// opening.
    /// <para>
    /// The crossing is measured in the project's own frame and has to be read in the
    /// wall's frame first: a service meeting the wall at an angle covers more of the wall
    /// than its own width, and the opening has to cover what the crossing really covers.
    /// The clearance is then added on every side, so the opening is never tighter than
    /// the service passing through it.
    /// </para>
    /// </summary>
    public static class OpeningRules
    {
        /// <summary>Clearance added on every side of a crossing, in millimetres.</summary>
        public const double DefaultClearanceMillimeters = 50d;

        /// <summary>Shortest edge an opening may have, in millimetres.</summary>
        public const double MinimumEdgeMillimeters = 10d;

        /// <summary>Gets the clearance a crossing is grown by on every side.</summary>
        public static Length DefaultClearance => Length.FromMillimeters(DefaultClearanceMillimeters);

        /// <summary>Gets the shortest edge an opening may have.</summary>
        public static Length MinimumEdge => Length.FromMillimeters(MinimumEdgeMillimeters);

        /// <summary>
        /// Reads a crossing, given in the frame of the project, in the frame of the wall:
        /// how far it reaches along the wall from the wall's base point, and how high it
        /// reaches above it. Millimetres in, millimetres out.
        /// </summary>
        /// <param name="crossing">The box where the service and the wall meet, in project millimetres.</param>
        /// <param name="wallBase">A point of the wall's base line, in project millimetres.</param>
        /// <param name="wallDirection">
        /// Direction of the wall in the project's horizontal plane, in millimetres; it does
        /// not have to be a unit vector.
        /// </param>
        /// <returns>The crossing, measured along the wall and above its base point.</returns>
        public static OpeningRectangle InWallFrame(BoundingBox crossing, Point3 wallBase, Point3 wallDirection)
        {
            double directionX = wallDirection.X;
            double directionY = wallDirection.Y;
            double length = Math.Sqrt((directionX * directionX) + (directionY * directionY));

            if (length <= 0d)
            {
                throw new ArgumentException(
                    "The direction of the wall must not be a single point.",
                    nameof(wallDirection));
            }

            directionX /= length;
            directionY /= length;

            double alongMin = double.MaxValue;
            double alongMax = double.MinValue;

            foreach (double x in new[] { crossing.Min.X, crossing.Max.X })
            {
                foreach (double y in new[] { crossing.Min.Y, crossing.Max.Y })
                {
                    double along = ((x - wallBase.X) * directionX) + ((y - wallBase.Y) * directionY);

                    if (along < alongMin)
                    {
                        alongMin = along;
                    }

                    if (along > alongMax)
                    {
                        alongMax = along;
                    }
                }
            }

            return new OpeningRectangle(
                alongMin,
                alongMax,
                crossing.Min.Z - wallBase.Z,
                crossing.Max.Z - wallBase.Z);
        }

        /// <summary>Adds the clearance on all four sides of a crossing.</summary>
        /// <param name="crossing">The crossing in the wall's frame, in millimetres.</param>
        /// <param name="clearance">Clearance added to each side; a negative value is read as none.</param>
        /// <returns>The rectangle of the opening.</returns>
        public static OpeningRectangle WithClearance(OpeningRectangle crossing, Length clearance)
        {
            double amount = clearance.Millimeters;

            if (amount < 0d)
            {
                amount = 0d;
            }

            return new OpeningRectangle(
                crossing.AlongMin - amount,
                crossing.AlongMax + amount,
                crossing.HeightMin - amount,
                crossing.HeightMax + amount);
        }

        /// <summary>
        /// Tells whether two rectangles cover the same place and the same size, within
        /// the tolerance on every edge.
        /// </summary>
        /// <param name="first">First rectangle, in millimetres.</param>
        /// <param name="second">Second rectangle, in millimetres.</param>
        /// <param name="tolerance">Widest difference of an edge still counted as the same.</param>
        /// <returns><c>true</c> when the two describe the same opening.</returns>
        public static bool Coincides(OpeningRectangle first, OpeningRectangle second, Length tolerance)
        {
            double slack = tolerance.Millimeters;

            if (slack < 0d)
            {
                slack = 0d;
            }

            return Math.Abs(first.AlongMin - second.AlongMin) <= slack
                && Math.Abs(first.AlongMax - second.AlongMax) <= slack
                && Math.Abs(first.HeightMin - second.HeightMin) <= slack
                && Math.Abs(first.HeightMax - second.HeightMax) <= slack;
        }

        /// <summary>
        /// Tells whether a rectangle is worth cutting: both of its edges reach the
        /// shortest edge an opening may have.
        /// </summary>
        /// <param name="rectangle">The rectangle of the opening.</param>
        /// <param name="minimum">Shortest edge allowed.</param>
        /// <returns><c>true</c> when the opening can be cut.</returns>
        public static bool IsUsable(OpeningRectangle rectangle, Length minimum)
        {
            return rectangle.Width >= minimum && rectangle.Height >= minimum;
        }
    }
}
