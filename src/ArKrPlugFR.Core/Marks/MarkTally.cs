using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Marks
{
    /// <summary>
    /// Counts how many elements carry each mark.
    /// <para>
    /// Marks are written by hand and arrive with stray spaces and mixed case; the tally
    /// looks at them the way a person would — "КМ-1", " км-1 " and "Км-1" are the same
    /// mark — and carries the trimmed form the first time it sees it.
    /// </para>
    /// </summary>
    public static class MarkTally
    {
        /// <summary>Counts the elements of every mark.</summary>
        /// <param name="marks">The marks of the elements; empty marks are left out.</param>
        /// <returns>How many elements carry each mark.</returns>
        public static IReadOnlyDictionary<string, int> Count(IEnumerable<string?> marks)
        {
            if (marks == null)
            {
                throw new ArgumentNullException(nameof(marks));
            }

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string? mark in marks)
            {
                if (!TryKey(mark, out string key))
                {
                    continue;
                }

                counts.TryGetValue(key, out int count);
                counts[key] = count + 1;
            }

            return counts;
        }

        /// <summary>Reads how many elements carry one mark.</summary>
        /// <param name="counts">The tally of a run.</param>
        /// <param name="mark">The mark to look up.</param>
        /// <returns>The number of elements, or zero for a mark that is empty or unknown.</returns>
        public static int Lookup(IReadOnlyDictionary<string, int> counts, string? mark)
        {
            if (counts == null)
            {
                throw new ArgumentNullException(nameof(counts));
            }

            if (!TryKey(mark, out string key))
            {
                return 0;
            }

            return counts.TryGetValue(key, out int count) ? count : 0;
        }

        /// <summary>
        /// Reads a mark as a key: trimmed, and rejected when nothing is left.
        /// </summary>
        /// <param name="mark">The mark to read.</param>
        /// <param name="key">The trimmed mark, or an empty string.</param>
        /// <returns><c>true</c> when the mark can be used as a key.</returns>
        public static bool TryKey(string? mark, out string key)
        {
            key = mark?.Trim() ?? string.Empty;

            return key.Length > 0;
        }
    }
}
