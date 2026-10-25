using System;
using System.Text;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// Naming rules for the copies a run creates.
    /// <para>
    /// Sheet numbers must stay unique, so copies get an underscore suffix; sheet
    /// names do not have to be unique but several copies of one source should
    /// still be told apart, so each copy name carries its copy number. Element
    /// names must not contain the characters the modelling software refuses, so
    /// every generated name is stripped first.
    /// </para>
    /// </summary>
    public static class SheetCopyNames
    {
        /// <summary>
        /// Characters that are not permitted inside an element name; they are
        /// removed from every name this tool generates.
        /// </summary>
        private const string DisallowedCharacters = "{}[]|;<>?`~\\/:*\"";

        /// <summary>Removes the characters that are not permitted in element names.</summary>
        /// <param name="name">Proposed name; may be <c>null</c>.</param>
        /// <returns>The name with every disallowed character removed.</returns>
        public static string StripDisallowed(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(name!.Length);

            foreach (char character in name)
            {
                if (DisallowedCharacters.IndexOf(character) < 0)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        /// <summary>Builds the name of one copy so several copies stay separable.</summary>
        /// <param name="sourceName">Name of the sheet being copied.</param>
        /// <param name="copyNumber">One-based number of the copy.</param>
        /// <returns>The stripped source name with the copy number appended.</returns>
        public static string MarkedSheetName(string? sourceName, int copyNumber)
        {
            if (copyNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(copyNumber), "Copy numbers start at one.");
            }

            string stripped = StripDisallowed(sourceName).TrimEnd();

            return stripped + " (" + copyNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
        }

        /// <summary>
        /// Returns the first name in the sequence desired, desired (2), desired (3)
        /// and so on that nothing in <paramref name="isTaken"/> claims.
        /// </summary>
        /// <param name="desired">The name the copy would like to have; already stripped.</param>
        /// <param name="isTaken">Tells whether a candidate name is used elsewhere.</param>
        /// <returns>The first free candidate; never empty when the desired name is not empty.</returns>
        public static string NextFreeName(string? desired, Func<string, bool> isTaken)
        {
            if (isTaken == null)
            {
                throw new ArgumentNullException(nameof(isTaken));
            }

            string candidate = StripDisallowed(desired).TrimEnd();

            if (candidate.Length == 0)
            {
                return string.Empty;
            }

            if (!isTaken(candidate))
            {
                return candidate;
            }

            for (int counter = 2; counter < int.MaxValue; counter++)
            {
                string next = candidate + " (" + counter.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";

                if (!isTaken(next))
                {
                    return next;
                }
            }

            return candidate;
        }
    }
}
