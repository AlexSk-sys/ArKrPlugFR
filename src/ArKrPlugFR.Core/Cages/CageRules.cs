using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Cages
{
    /// <summary>
    /// The arithmetic of a rebar cage: how many positions of inclined and transverse bars
    /// fit along the cage, and how many running metres the three groups of bars add up to.
    /// <para>
    /// The rules are plain numbers: a cage length in metres, a step in metres and bar
    /// lengths in metres, so they run and are tested without Revit. A cage is measured
    /// along its layout lines; a group whose step or bar length is unusable contributes
    /// nothing and is reported by the caller.
    /// </para>
    /// </summary>
    public static class CageRules
    {
        /// <summary>
        /// Counts the positions of bars spaced by a step along a length: the smallest whole
        /// number of steps that covers the length, plus one for the bar at the start.
        /// </summary>
        /// <param name="lengthMetres">Length the bars are spread over, in metres.</param>
        /// <param name="stepMetres">Distance between two bars, in metres.</param>
        /// <returns>The number of positions, or zero when the step or the length cannot be used.</returns>
        public static int Positions(double lengthMetres, double stepMetres)
        {
            if (stepMetres <= 0 || lengthMetres < 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(lengthMetres / stepMetres) + 1;
        }

        /// <summary>Adds up the longitudinal bars: every one of them runs the whole length.</summary>
        /// <param name="lengthMetres">Cage length in metres.</param>
        /// <param name="bars">Number of longitudinal bars; a negative value counts as none.</param>
        /// <returns>Running metres of longitudinal bar.</returns>
        public static double LongitudinalMetres(double lengthMetres, int bars)
        {
            if (lengthMetres <= 0 || bars <= 0)
            {
                return 0;
            }

            return lengthMetres * bars;
        }

        /// <summary>
        /// Adds up the inclined bars: two bars per position, each of the given length.
        /// </summary>
        /// <param name="positions">Number of positions along the cage.</param>
        /// <param name="barLengthMetres">Length of one inclined bar in metres.</param>
        /// <returns>Running metres of inclined bar.</returns>
        public static double InclinedMetres(int positions, double barLengthMetres)
        {
            if (positions <= 0 || barLengthMetres <= 0)
            {
                return 0;
            }

            return positions * 2 * barLengthMetres;
        }

        /// <summary>
        /// Adds up the transverse bars: every distinct length the cage holds contributes
        /// once per position.
        /// </summary>
        /// <param name="positions">Number of positions along the cage.</param>
        /// <param name="distinctBarLengths">The distinct transverse bar lengths, in metres.</param>
        /// <returns>Running metres of transverse bar.</returns>
        public static double TransverseMetres(int positions, IEnumerable<double> distinctBarLengths)
        {
            if (distinctBarLengths == null)
            {
                throw new ArgumentNullException(nameof(distinctBarLengths));
            }

            if (positions <= 0)
            {
                return 0;
            }

            double sum = 0;

            foreach (double length in distinctBarLengths)
            {
                if (length > 0)
                {
                    sum += length;
                }
            }

            return positions * sum;
        }

        /// <summary>Rounds a number of running metres to the precision the report shows.</summary>
        /// <param name="metres">Running metres.</param>
        /// <returns>The value rounded to two decimals.</returns>
        public static double Round(double metres)
        {
            return Math.Round(metres, 2, MidpointRounding.AwayFromZero);
        }
    }
}
