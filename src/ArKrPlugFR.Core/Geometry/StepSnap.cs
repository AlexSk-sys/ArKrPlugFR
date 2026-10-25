using System;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Geometry
{
    /// <summary>
    /// Snapping of a scalar value onto a regular grid within a tolerance.
    /// <para>
    /// The grid is defined as <c>origin + k * step</c> for integer <c>k</c>. A
    /// value is only moved when it is already within
    /// <see cref="SnapSettings.Tolerance"/> of the grid, so genuinely free
    /// geometry is never displaced. This is the rule behind the axis tools: fix
    /// the elements that are "almost" aligned, leave the rest alone.
    /// </para>
    /// </summary>
    public static class StepSnap
    {
        private const double Epsilon = 1e-9;

        /// <summary>
        /// Returns the nearest grid value when the distance to it does not exceed
        /// the tolerance; otherwise the input value is returned unchanged.
        /// </summary>
        /// <param name="value">The coordinate to evaluate, in millimetres.</param>
        /// <param name="settings">The grid definition; must not be <c>null</c>.</param>
        public static double Snap(double value, SnapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            double step = settings.Step.Millimeters;
            if (step <= 0d)
            {
                return value;
            }

            double origin = settings.Origin?.Millimeters ?? 0d;
            double candidate = origin + (Math.Round((value - origin) / step, MidpointRounding.AwayFromZero) * step);

            return Math.Abs(candidate - value) > settings.Tolerance.Millimeters ? value : candidate;
        }

        /// <summary>
        /// Determines whether <paramref name="value"/> already lies exactly on
        /// the grid, within a numerical epsilon rather than the snapping
        /// tolerance.
        /// </summary>
        /// <remarks>
        /// This is not the same as "snapping would not move the value": a value
        /// outside the tolerance is also left in place, but it is not on the grid.
        /// </remarks>
        public static bool IsOnGrid(double value, SnapSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            double step = settings.Step.Millimeters;
            if (step <= 0d)
            {
                return true;
            }

            double origin = settings.Origin?.Millimeters ?? 0d;
            double remainder = Math.Abs((value - origin) % step);

            return remainder <= Epsilon || Math.Abs(remainder - step) <= Epsilon;
        }
    }
}
