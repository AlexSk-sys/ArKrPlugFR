using System;

namespace ArKrPlugFR.Core.Parameters
{
    /// <summary>
    /// One layer of a compound structure as the text transfer sees it: the material and
    /// the width in millimetres.
    /// </summary>
    public sealed class LayerFact
    {
        /// <summary>Creates a layer fact.</summary>
        /// <param name="material">Name of the material, or <c>null</c> when the layer has none.</param>
        /// <param name="widthMillimeters">Width of the layer in millimetres.</param>
        public LayerFact(string? material, double widthMillimeters)
        {
            Material = material;
            WidthMillimeters = widthMillimeters;
        }

        /// <summary>Gets the name of the material, or <c>null</c>.</summary>
        public string? Material { get; }

        /// <summary>Gets the width of the layer in millimetres.</summary>
        public double WidthMillimeters { get; }
    }
}
