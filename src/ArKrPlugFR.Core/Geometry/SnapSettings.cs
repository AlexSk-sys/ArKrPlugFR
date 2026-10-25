using System;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Geometry
{
    /// <summary>
    /// User-configurable settings for <see cref="StepSnap"/>.
    /// <para>
    /// Every value is expressed in millimetres and is stored in the project
    /// configuration file, so nothing is hard-coded to a particular office
    /// standard.
    /// </para>
    /// </summary>
    public sealed class SnapSettings
    {
        /// <summary>Creates settings with a step, a tolerance and an optional origin.</summary>
        /// <param name="step">The grid step; must be greater than zero.</param>
        /// <param name="tolerance">The maximum distance at which snapping is allowed.</param>
        /// <param name="origin">An optional offset of the grid, measured from zero.</param>
        public SnapSettings(Length step, Length tolerance, Length? origin = null)
        {
            if (step.Millimeters <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(step), step, "The snap step must be greater than zero.");
            }

            if (tolerance.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance cannot be negative.");
            }

            Step = step;
            Tolerance = tolerance;
            Origin = origin;
        }

        /// <summary>Gets the grid step.</summary>
        public Length Step { get; }

        /// <summary>Gets the maximum distance at which a value is moved onto the grid.</summary>
        public Length Tolerance { get; }

        /// <summary>Gets the optional origin of the grid.</summary>
        public Length? Origin { get; }
    }
}
