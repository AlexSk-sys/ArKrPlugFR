using System;
using System.Globalization;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Concrete
{
    /// <summary>
    /// A rectangle measured across one direction of passage: two edges on one axis
    /// across, two on the other. All four are millimetres.
    /// <para>
    /// The same shape describes where a service crosses a floor or a column and the
    /// opening cut around that crossing, because the opening is the crossing grown by a
    /// clearance and clamped to the element.
    /// </para>
    /// </summary>
    public readonly struct AxisRectangle
    {
        /// <summary>Creates a rectangle, ordering the edges on both axes.</summary>
        /// <param name="minU">First edge across, in millimetres.</param>
        /// <param name="maxU">Opposite edge across, in millimetres.</param>
        /// <param name="minV">First edge along the second axis, in millimetres.</param>
        /// <param name="maxV">Opposite edge along the second axis, in millimetres.</param>
        public AxisRectangle(double minU, double maxU, double minV, double maxV)
        {
            MinU = Math.Min(minU, maxU);
            MaxU = Math.Max(minU, maxU);
            MinV = Math.Min(minV, maxV);
            MaxV = Math.Max(minV, maxV);
        }

        /// <summary>Gets the lower edge across, in millimetres.</summary>
        public double MinU { get; }

        /// <summary>Gets the upper edge across, in millimetres.</summary>
        public double MaxU { get; }

        /// <summary>Gets the lower edge along the second axis, in millimetres.</summary>
        public double MinV { get; }

        /// <summary>Gets the upper edge along the second axis, in millimetres.</summary>
        public double MaxV { get; }

        /// <summary>Gets the size across.</summary>
        public Length Width => Length.FromMillimeters(MaxU - MinU);

        /// <summary>Gets the size along the second axis.</summary>
        public Length Height => Length.FromMillimeters(MaxV - MinV);

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.#}x{1:0.#} mm at {2:0.#}..{3:0.#}",
                Width.Millimeters,
                Height.Millimeters,
                MinU,
                MaxU);
        }
    }
}
