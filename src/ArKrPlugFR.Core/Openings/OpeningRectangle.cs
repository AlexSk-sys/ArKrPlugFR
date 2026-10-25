using System;
using System.Globalization;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Openings
{
    /// <summary>
    /// A rectangle in the frame of a wall: how far it reaches along the wall from the
    /// wall's base point, and how far it reaches up from that point. Both are
    /// millimetres.
    /// <para>
    /// The same shape describes where a service crosses a wall and the opening cut
    /// around that crossing, because the opening is the crossing grown by a clearance.
    /// </para>
    /// </summary>
    public readonly struct OpeningRectangle
    {
        /// <summary>Creates a rectangle, ordering the corners.</summary>
        /// <param name="alongMin">Nearer edge along the wall, in millimetres.</param>
        /// <param name="alongMax">Further edge along the wall, in millimetres.</param>
        /// <param name="heightMin">Lower edge above the wall's base point, in millimetres.</param>
        /// <param name="heightMax">Upper edge above the wall's base point, in millimetres.</param>
        public OpeningRectangle(double alongMin, double alongMax, double heightMin, double heightMax)
        {
            AlongMin = Math.Min(alongMin, alongMax);
            AlongMax = Math.Max(alongMin, alongMax);
            HeightMin = Math.Min(heightMin, heightMax);
            HeightMax = Math.Max(heightMin, heightMax);
        }

        /// <summary>Gets the nearer edge along the wall, in millimetres.</summary>
        public double AlongMin { get; }

        /// <summary>Gets the further edge along the wall, in millimetres.</summary>
        public double AlongMax { get; }

        /// <summary>Gets the lower edge above the wall's base point, in millimetres.</summary>
        public double HeightMin { get; }
        /// <summary>Gets the upper edge above the wall's base point, in millimetres.</summary>
        public double HeightMax { get; }

        /// <summary>Gets the size of the rectangle along the wall.</summary>
        public Length Width => Length.FromMillimeters(AlongMax - AlongMin);

        /// <summary>Gets the size of the rectangle across the wall.</summary>
        public Length Height => Length.FromMillimeters(HeightMax - HeightMin);

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.#}x{1:0.#} mm at {2:0.#}..{3:0.#}",
                Width.Millimeters,
                Height.Millimeters,
                AlongMin,
                AlongMax);
        }
    }
}
