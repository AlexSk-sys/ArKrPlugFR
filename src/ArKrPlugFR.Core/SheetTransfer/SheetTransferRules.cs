using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArKrPlugFR.Core.SheetTransfer
{
    /// <summary>
    /// The pure rules of the sheet transfer: what may move, how sheet numbers and
    /// names are compared, cleaned and made unique.
    /// <para>
    /// Everything here works on plain values, so the rules are unit-tested without
    /// a running Revit; the plugin only reads elements and applies these decisions.
    /// </para>
    /// </summary>
    public static class SheetTransferRules
    {
        /// <summary>Separator appended to a sheet number that had to be renumbered.</summary>
        public const string NumberSeparator = "_";

        /// <summary>
        /// Characters Revit does not accept in element names; they are stripped
        /// from every generated sheet and view name.
        /// </summary>
        private const string InvalidNameCharacters = "{}[]|;<>?`~^";

        /// <summary>Determines whether a sheet may be copied into another project.</summary>
        /// <param name="sheet">Sheet to check; may be <c>null</c>.</param>
        /// <returns>
        /// <c>true</c> for a real sheet with a usable number; placeholder sheets of
        /// a sheet list are never transferable.
        /// </returns>
        public static bool IsTransferable(SheetCandidate? sheet)
        {
            return sheet is not null
                && !sheet.IsPlaceholder
                && !string.IsNullOrWhiteSpace(sheet.Number);
        }

        /// <summary>Determines whether a view may be counted as content of a copied sheet.</summary>
        /// <param name="view">View to check; may be <c>null</c>.</param>
        /// <returns><c>true</c> for a placed, non-template view.</returns>
        public static bool IsTransferableView(ViewCandidate? view)
        {
            return view is not null && !view.IsTemplate;
        }

        /// <summary>
        /// Filters the transferable sheets out and orders them with a natural
        /// comparison of the sheet numbers, so A-1 comes before A-10.
        /// </summary>
        /// <param name="sheets">Sheets in any order; duplicates are kept once.</param>
        /// <returns>The transferable sheets, one per element id, in sheet-number order.</returns>
        public static IReadOnlyList<SheetCandidate> SelectTransferable(IEnumerable<SheetCandidate> sheets)
        {
            if (sheets is null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            var seen = new HashSet<long>();
            var list = new List<SheetCandidate>();

            foreach (SheetCandidate sheet in sheets)
            {
                if (!IsTransferable(sheet) || !seen.Add(sheet.Id))
                {
                    continue;
                }

                list.Add(sheet);
            }

            list.Sort(new NaturalSheetNumberComparer());

            return list;
        }

        /// <summary>
        /// Compares two sheet numbers so that the digits count as one number:
        /// A-1 comes before A-10, and the letter case is ignored.
        /// </summary>
        /// <param name="left">First number; a missing value counts as empty.</param>
        /// <param name="right">Second number; a missing value counts as empty.</param>
        /// <returns>Negative, zero or positive as the first number sorts first, equal or after the second.</returns>
        public static int CompareSheetNumbers(string? left, string? right)
        {
            string first = left ?? string.Empty;
            string second = right ?? string.Empty;

            int i = 0;
            int j = 0;

            while (i < first.Length && j < second.Length)
            {
                if (char.IsDigit(first[i]) && char.IsDigit(second[j]))
                {
                    int firstStart = i;
                    int secondStart = j;

                    while (i < first.Length && char.IsDigit(first[i]))
                    {
                        i++;
                    }

                    while (j < second.Length && char.IsDigit(second[j]))
                    {
                        j++;
                    }

                    // Leading zeroes do not change the value: A-01 and A-1 are the
                    // same number for ordering purposes.
                    while (firstStart < i - 1 && first[firstStart] == '0')
                    {
                        firstStart++;
                    }

                    while (secondStart < j - 1 && second[secondStart] == '0')
                    {
                        secondStart++;
                    }

                    int firstLength = i - firstStart;
                    int secondLength = j - secondStart;

                    if (firstLength != secondLength)
                    {
                        return firstLength < secondLength ? -1 : 1;
                    }

                    for (int k = 0; k < firstLength; k++)
                    {
                        if (first[firstStart + k] != second[secondStart + k])
                        {
                            return first[firstStart + k] < second[secondStart + k] ? -1 : 1;
                        }
                    }
                }
                else
                {
                    char firstChar = char.ToUpperInvariant(first[i]);
                    char secondChar = char.ToUpperInvariant(second[j]);

                    if (firstChar != secondChar)
                    {
                        return firstChar < secondChar ? -1 : 1;
                    }

                    i++;
                    j++;
                }
            }

            return (first.Length - i).CompareTo(second.Length - j);
        }

        /// <summary>
        /// Removes the characters Revit refuses in element names and trims the ends.
        /// </summary>
        /// <param name="name">Name to clean; a missing value becomes empty.</param>
        /// <returns>The name without forbidden characters.</returns>
        public static string SanitizeName(string? name)
        {
            if (name is null || name.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(name.Length);

            foreach (char character in name)
            {
                if (character < ' ' || InvalidNameCharacters.IndexOf(character) >= 0)
                {
                    continue;
                }

                builder.Append(character);
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Builds the sheet name of a copy: the cleaned source name plus the marker
        /// that keeps several copies of one sheet apart.
        /// </summary>
        /// <param name="name">Source sheet name.</param>
        /// <param name="marker">Distinguishing marker; an empty marker changes nothing.</param>
        /// <returns>The marked name; a name that already ends with the marker is kept as it is.</returns>
        public static string MarkSheetName(string? name, string? marker)
        {
            string clean = SanitizeName(name);
            string mark = (marker ?? string.Empty).Trim();

            if (mark.Length == 0)
            {
                return clean;
            }

            if (clean.EndsWith(mark, StringComparison.OrdinalIgnoreCase))
            {
                return clean;
            }

            return SanitizeName(clean + " " + mark);
        }

        /// <summary>
        /// Picks the first free sheet number: the desired number when the target
        /// project does not use it yet, otherwise the number with the lowest free
        /// underscore suffix — A-01 gives A-01_1, A-01_2, and so on.
        /// </summary>
        /// <param name="desired">Number taken from the source sheet.</param>
        /// <param name="taken">Numbers that already exist or were assigned in this run; compared without letter case.</param>
        /// <returns>A number that is not among the taken ones.</returns>
        public static string MakeUniqueNumber(string? desired, IEnumerable<string> taken)
        {
            if (taken is null)
            {
                throw new ArgumentNullException(nameof(taken));
            }

            string baseNumber = SanitizeName(desired);
            var occupied = new HashSet<string>(taken, StringComparer.OrdinalIgnoreCase);

            if (!occupied.Contains(baseNumber))
            {
                return baseNumber;
            }

            long suffix = 1;

            while (true)
            {
                string candidate = baseNumber + NumberSeparator + suffix.ToString(CultureInfo.InvariantCulture);

                if (!occupied.Contains(candidate))
                {
                    return candidate;
                }

                suffix++;
            }
        }

        /// <summary>
        /// Picks the first free view name: the source name with the copy marker,
        /// then the same with a numeric tail that grows until the name is free.
        /// </summary>
        /// <param name="desired">View name taken from the source project.</param>
        /// <param name="marker">Marker that marks the copy; an empty marker keeps the source name.</param>
        /// <param name="taken">Names that already exist in the target project; compared without letter case.</param>
        /// <returns>A name that is not among the taken ones.</returns>
        public static string MakeUniqueViewName(string? desired, string? marker, IEnumerable<string> taken)
        {
            if (taken is null)
            {
                throw new ArgumentNullException(nameof(taken));
            }

            string marked = MarkSheetName(desired, marker);
            var occupied = new HashSet<string>(taken, StringComparer.OrdinalIgnoreCase);

            if (!occupied.Contains(marked))
            {
                return marked;
            }

            long tail = 2;

            while (true)
            {
                string candidate = marked + " " + tail.ToString(CultureInfo.InvariantCulture);

                if (!occupied.Contains(candidate))
                {
                    return candidate;
                }

                tail++;
            }
        }

        /// <summary>Orders sheet candidates by their sheet numbers, naturally and without letter case.</summary>
        private sealed class NaturalSheetNumberComparer : IComparer<SheetCandidate>
        {
            /// <inheritdoc />
            public int Compare(SheetCandidate? x, SheetCandidate? y)
            {
                return CompareSheetNumbers(x?.Number, y?.Number);
            }
        }
    }
}
