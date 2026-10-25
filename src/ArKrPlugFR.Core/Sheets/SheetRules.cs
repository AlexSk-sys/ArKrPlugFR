using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// Rules that decide which sheets may be duplicated and how their numbers
    /// are ordered in the list.
    /// <para>
    /// A placeholder of a sheet list is a row, not a sheet: it has no content to
    /// copy, so it is never offered and never processed. A sheet whose element
    /// no longer resolves is reported by the reader as not alive and is skipped
    /// for the same reason.
    /// </para>
    /// </summary>
    public static class SheetRules
    {
        /// <summary>Determines whether the sheet may take part in a duplication run.</summary>
        /// <param name="sheet">Facts of the sheet; may be <c>null</c>.</param>
        /// <returns><c>true</c> when the sheet is a real, live sheet with a number.</returns>
        public static bool IsDuplicable(SheetFacts? sheet)
        {
            if (sheet == null)
            {
                return false;
            }

            return sheet.IsAlive
                && !sheet.IsPlaceholder
                && !string.IsNullOrWhiteSpace(sheet.Number);
        }

        /// <summary>Keeps only the sheets that may be duplicated, in their original order.</summary>
        /// <param name="sheets">Facts of every collected sheet.</param>
        /// <returns>The duplicable sheets.</returns>
        public static IReadOnlyList<SheetFacts> SelectDuplicable(IEnumerable<SheetFacts> sheets)
        {
            if (sheets == null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            var selected = new List<SheetFacts>();

            foreach (SheetFacts sheet in sheets)
            {
                if (IsDuplicable(sheet))
                {
                    selected.Add(sheet);
                }
            }

            return selected;
        }

        /// <summary>
        /// Compares two strings the way people order sheet numbers: digit runs
        /// compare numerically, everything else compares letter by letter
        /// ignoring case, so A-1 sorts before A-10.
        /// </summary>
        /// <param name="left">First string; a <c>null</c> value is treated as empty.</param>
        /// <param name="right">Second string; a <c>null</c> value is treated as empty.</param>
        /// <returns>Less than zero, zero or greater than zero, as with any comparison.</returns>
        public static int NaturalCompare(string? left, string? right)
        {
            string first = left ?? string.Empty;
            string second = right ?? string.Empty;

            int i = 0;
            int j = 0;

            while (i < first.Length && j < second.Length)
            {
                if (char.IsDigit(first[i]) && char.IsDigit(second[j]))
                {
                    int runStartA = i;
                    int runStartB = j;

                    while (i < first.Length && char.IsDigit(first[i]))
                    {
                        i++;
                    }

                    while (j < second.Length && char.IsDigit(second[j]))
                    {
                        j++;
                    }

                    string runA = TrimLeadingZeros(first.Substring(runStartA, i - runStartA));
                    string runB = TrimLeadingZeros(second.Substring(runStartB, j - runStartB));

                    if (runA.Length != runB.Length)
                    {
                        return runA.Length - runB.Length;
                    }

                    int runCompare = string.CompareOrdinal(runA, runB);

                    if (runCompare != 0)
                    {
                        return runCompare;
                    }

                    continue;
                }

                char charA = char.ToUpperInvariant(first[i]);
                char charB = char.ToUpperInvariant(second[j]);

                if (charA != charB)
                {
                    return charA - charB;
                }

                i++;
                j++;
            }

            return (first.Length - i) - (second.Length - j);
        }

        private static string TrimLeadingZeros(string digits)
        {
            int start = 0;

            while (start < digits.Length - 1 && digits[start] == '0')
            {
                start++;
            }

            return digits.Substring(start);
        }
    }
}
