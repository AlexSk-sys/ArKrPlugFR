using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Couplers
{
    /// <summary>
    /// The measurement behind the coupler marks: which bar lies nearest to the central
    /// axis of a coupler, and whether it lies close enough to count as connected.
    /// </summary>
    public static class CouplerRules
    {
        /// <summary>Tolerance the tool starts with, in millimetres.</summary>
        public const double DefaultToleranceMillimeters = 200d;

        /// <summary>Smallest tolerance a run accepts, in millimetres.</summary>
        public const double MinimumToleranceMillimeters = 1d;

        /// <summary>Largest tolerance a run accepts, in millimetres.</summary>
        public const double MaximumToleranceMillimeters = 2000d;

        /// <summary>Determines whether a tolerance can be used.</summary>
        /// <param name="toleranceMillimeters">Tolerance to check.</param>
        public static bool IsUsableTolerance(double toleranceMillimeters)
        {
            return toleranceMillimeters >= MinimumToleranceMillimeters
                && toleranceMillimeters <= MaximumToleranceMillimeters;
        }

        /// <summary>
        /// Finds the centre nearest to the axis within the tolerance.
        /// </summary>
        /// <param name="axis">Central axis of the coupler.</param>
        /// <param name="centres">Centres of the bars, in reading order.</param>
        /// <param name="toleranceMillimeters">Distance beyond which a bar does not count.</param>
        /// <returns>The index of the nearest centre, or <c>-1</c> when none is in reach.</returns>
        public static int FindNearest(Segment3 axis, IReadOnlyList<Point3> centres, double toleranceMillimeters)
        {
            if (centres == null)
            {
                throw new ArgumentNullException(nameof(centres));
            }

            if (toleranceMillimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(toleranceMillimeters), toleranceMillimeters, "The tolerance cannot be negative.");
            }

            int nearest = -1;
            double nearestDistance = toleranceMillimeters;

            for (int index = 0; index < centres.Count; index++)
            {
                double distance = axis.DistanceTo(centres[index]);

                if (distance <= toleranceMillimeters && (nearest == -1 || distance < nearestDistance))
                {
                    nearest = index;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }
    }
}
