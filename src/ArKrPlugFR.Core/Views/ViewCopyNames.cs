using System;
using System.Globalization;
using System.Text;

namespace ArKrPlugFR.Core.Views
{
    /// <summary>
    /// Naming rules for the copies one run creates.
    /// <para>
    /// Element names in a project are unique, so a copy is named after its source
    /// with an underscore and a number, and the number keeps growing until the name is
    /// free. Characters the modelling software refuses inside a name are removed
    /// first, and the source element itself is never renamed.
    /// </para>
    /// </summary>
    public static class ViewCopyNames
    {
        /// <summary>
        /// Characters that are not permitted inside an element name; they are removed
        /// from every name this tool generates.
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

        /// <summary>
        /// Names one copy: the source name with an underscore and the first free number
        /// from <paramref name="firstNumber"/> upward.
        /// </summary>
        /// <param name="sourceName">Name of the element being copied.</param>
        /// <param name="firstNumber">Number the search starts at; copy numbers start at one.</param>
        /// <param name="isTaken">Tells whether a candidate name is already used in the document.</param>
        /// <returns>The first free copy name, or an empty string when the source name is empty.</returns>
        public static string NextCopyName(string? sourceName, int firstNumber, Func<string, bool> isTaken)
        {
            if (isTaken == null)
            {
                throw new ArgumentNullException(nameof(isTaken));
            }

            string stripped = StripDisallowed(sourceName).TrimEnd();

            if (stripped.Length == 0)
            {
                return string.Empty;
            }

            for (int number = firstNumber < 1 ? 1 : firstNumber; number < int.MaxValue; number++)
            {
                string candidate = stripped + "_" + number.ToString(CultureInfo.InvariantCulture);

                if (!isTaken(candidate))
                {
                    return candidate;
                }
            }

            return stripped;
        }
    }
}
