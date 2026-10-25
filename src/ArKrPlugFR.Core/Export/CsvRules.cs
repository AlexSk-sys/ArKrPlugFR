using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Parameters;

namespace ArKrPlugFR.Core.Export
{
    /// <summary>
    /// Builds the lines of a comma-separated file.
    /// <para>
    /// A cell is quoted when it carries the separator, a quote or a line break, and a quote
    /// inside a cell is doubled; numbers are written with a dot and no grouping, whatever
    /// the culture of the machine, because the file is read by programs.
    /// </para>
    /// </summary>
    public static class CsvRules
    {
        /// <summary>What separates the cells of a row.</summary>
        public const string Separator = ",";

        /// <summary>Quotes a cell when it needs it.</summary>
        /// <param name="value">Value of the cell; <c>null</c> reads as empty.</param>
        public static string Escape(string? value)
        {
            string text = value ?? string.Empty;

            if (text.IndexOfAny(new[] { ',', ';', '"', '\n', '\r' }) < 0)
            {
                return text;
            }

            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Builds a row of cells.</summary>
        /// <param name="cells">Values of the row.</param>
        public static string Row(IReadOnlyList<string?> cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            var builder = new StringBuilder();

            for (int index = 0; index < cells.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(Separator);
                }

                builder.Append(Escape(cells[index]));
            }

            return builder.ToString();
        }

        /// <summary>Writes a number the way a data file wants it.</summary>
        /// <param name="value">Number to write.</param>
        /// <param name="format">Number format; trailing zeros are dropped by the default.</param>
        public static string Number(double value, string format = "0.###")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
