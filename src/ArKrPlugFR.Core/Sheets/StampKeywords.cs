using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// The words that give a stamp away when the sheet does not carry a real one.
    /// <para>
    /// Sheets are sometimes stamped with a generic annotation instead of a title
    /// block. Such a stand-in is recognised by what its family, its type and the
    /// element itself are called. The list holds plain wording only — the two words
    /// for the thing in each language of the tool — and deliberately no name of a
    /// specific organisation or standard, so the rule reads the same in any office.
    /// </para>
    /// </summary>
    public static class StampKeywords
    {
        /// <summary>Words that mark a stamp, in English and in Russian.</summary>
        public static readonly IReadOnlyList<string> Words = new[]
        {
            "stamp",
            "title block",
            "titleblock",
            "штамп",
            "титул",
            "основная надпись"
        };

        /// <summary>
        /// Tells whether any of the given names reads like a stamp.
        /// </summary>
        /// <param name="names">
        /// Names of the family, the type and the element itself; missing entries are
        /// simply skipped, and a missing collection reads as "no stamp".
        /// </param>
        /// <returns><c>true</c> when one of the names carries a stamp word.</returns>
        public static bool LooksLikeStamp(IEnumerable<string?>? names)
        {
            if (names == null)
            {
                return false;
            }

            foreach (string? name in names)
            {
                if (name == null || name.Length == 0)
                {
                    continue;
                }

                foreach (string word in Words)
                {
                    if (name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
