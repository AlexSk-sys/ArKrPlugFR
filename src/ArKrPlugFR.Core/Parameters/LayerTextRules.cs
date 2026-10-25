using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArKrPlugFR.Core.Parameters
{
    /// <summary>
    /// Turns the layers of a compound structure into one text value, one layer per line.
    /// <para>
    /// The text is what a user reads off the wall properties, so it follows the language
    /// of the interface: <c>1. Brick — 250 mm</c> in English and <c>1. Кирпич — 250 мм</c>
    /// in Russian. A layer without a material is printed as a question mark instead of
    /// being left out, because the number of lines is the number of layers.
    /// </para>
    /// </summary>
    public static class LayerTextRules
    {
        /// <summary>Printed in place of the name of a layer that has no material.</summary>
        public const string UnknownMaterial = "?";

        /// <summary>Formats the layers of one wall or floor.</summary>
        /// <param name="layers">Layers from the outside in, as the compound structure holds them.</param>
        /// <param name="culture">Two-letter culture code; anything but <c>ru</c> reads as English.</param>
        /// <returns>The lines of the value, or an empty string when there are no layers.</returns>
        public static string Format(IReadOnlyList<LayerFact> layers, string? culture)
        {
            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers));
            }

            string unit = string.Equals(culture, "ru", StringComparison.OrdinalIgnoreCase) ? "мм" : "mm";
            var builder = new StringBuilder();
            int number = 0;

            for (int index = 0; index < layers.Count; index++)
            {
                LayerFact layer = layers[index];

                if (layer == null)
                {
                    continue;
                }

                number++;

                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                string material = string.IsNullOrWhiteSpace(layer.Material) ? UnknownMaterial : layer.Material!.Trim();
                string width = layer.WidthMillimeters.ToString("0.#", CultureInfo.InvariantCulture);

                builder.Append(number);
                builder.Append(". ");
                builder.Append(material);
                builder.Append(" \u2014 ");
                builder.Append(width);
                builder.Append(' ');
                builder.Append(unit);
            }

            return builder.ToString();
        }
    }
}
