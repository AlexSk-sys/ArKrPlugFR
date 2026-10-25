using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// Reduces a parameter value to a form that can be compared between two types.
    /// <para>
    /// Two types that a user calls identical usually differ in whitespace, in
    /// trailing zeros or in number formatting. Those differences must not hide a
    /// duplicate, so every value is trimmed, its inner whitespace is collapsed and
    /// its numbers are compared as numbers.
    /// </para>
    /// </summary>
    public static class ParameterValueNormalizer
    {
        /// <summary>Default number of significant digits used when comparing numbers.</summary>
        public const int DefaultSignificantDigits = 15;

        private static readonly Regex WhitespaceRun = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>Normalises a parameter value.</summary>
        /// <param name="value">Raw value as reported by the modelling software; <c>null</c> means empty.</param>
        /// <param name="significantDigits">Significant digits used to compare numbers.</param>
        public static string Normalize(string? value, int significantDigits = DefaultSignificantDigits)
        {
            if (significantDigits < 1 || significantDigits > 17)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(significantDigits),
                    significantDigits,
                    "The number of significant digits must be between 1 and 17.");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string collapsed = WhitespaceRun.Replace(value!.Trim(), " ");

            if (TryReadNumber(collapsed, out double number))
            {
                return number.ToString("G" + significantDigits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            }

            // Non-numeric values are compared as text and the comparison ignores
            // case, so the canonical form is upper case. Upper rather than lower
            // because normalising to lower case is flagged by the analysers as
            // lossy for identifiers.
            return collapsed.ToUpperInvariant();
        }

        /// <summary>
        /// Reads a number written with either the dot or the comma as decimal
        /// separator, which is what bilingual projects produce.
        /// </summary>
        public static bool TryReadNumber(string value, out double number)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return true;
            }

            // A single comma and no dot is a decimal comma, not a thousands
            // separator: "1,5" is one and a half while "1,234" is written the same
            // way and stays ambiguous. Projects that use the dot never reach this
            // branch.
            int comma = value.IndexOf(',');

            if (value.IndexOf('.') < 0 && comma > 0 && comma == value.LastIndexOf(','))
            {
                string withDot = value.Replace(',', '.');

                if (double.TryParse(withDot, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    number = parsed;
                    return true;
                }
            }

            number = 0d;
            return false;
        }
    }
}
