using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArKrPlugFR.Core.Export
{
    /// <summary>
    /// The file names of exported sheets.
    /// <para>
    /// A sheet is named by hand, so its name may hold characters a file name cannot hold and
    /// may be empty; the rule turns the number and the name into something a file system
    /// accepts, and tells the caller when two sheets would land on the same file. It is
    /// plain text work and is tested without Revit.
    /// </para>
    /// </summary>
    public static class SheetFileNames
    {
        /// <summary>The separator between the number and the name of a sheet.</summary>
        public const string Separator = " - ";

        /// <summary>The name used when neither a number nor a name is usable.</summary>
        public const string Fallback = "Sheet";

        /// <summary>How long a produced file name may grow.</summary>
        public const int MaximumLength = 120;

        /// <summary>Builds the file name of a sheet from its number and its name.</summary>
        /// <param name="number">Number of the sheet, as the platform reports it.</param>
        /// <param name="name">Name of the sheet, as the platform reports it.</param>
        /// <returns>A file name without an extension.</returns>
        public static string ForSheet(string? number, string? name)
        {
            string cleanNumber = Sanitize(number);
            string cleanName = Sanitize(name);

            if (cleanNumber.Length == 0 && cleanName.Length == 0)
            {
                return Fallback;
            }

            if (cleanNumber.Length == 0)
            {
                return Trim(cleanName);
            }

            if (cleanName.Length == 0)
            {
                return Trim(cleanNumber);
            }

            return Trim(cleanNumber + Separator + cleanName);
        }

        /// <summary>
        /// Makes a piece of text usable inside a file name: characters a file name cannot
        /// hold become a dash, runs of spaces collapse and the ends are trimmed.
        /// </summary>
        /// <param name="value">The text to clean.</param>
        /// <returns>The cleaned text; empty when there was nothing to keep.</returns>
        public static string Sanitize(string? value)
        {
            if (value == null || value.Trim().Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            char[] invalid = Path.GetInvalidFileNameChars();
            bool lastWasSpace = false;

            foreach (char character in value.Trim())
            {
                if (character == '\r' || character == '\n' || character == '\t')
                {
                    continue;
                }

                if (Array.IndexOf(invalid, character) >= 0)
                {
                    builder.Append('-');
                    lastWasSpace = false;

                    continue;
                }

                if (character == ' ')
                {
                    if (lastWasSpace)
                    {
                        continue;
                    }

                    lastWasSpace = true;
                }
                else
                {
                    lastWasSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString().Trim(' ', '.');
        }

        /// <summary>Finds the file names that more than one sheet would produce.</summary>
        /// <param name="names">The file names of the sheets, in the order of the sheets.</param>
        /// <returns>The repeated names, each one once, in the order they repeat.</returns>
        public static IReadOnlyList<string> Duplicates(IEnumerable<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var repeated = new List<string>();
            var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names)
            {
                string key = name ?? string.Empty;

                if (!seen.Add(key) && reported.Add(key))
                {
                    repeated.Add(key);
                }
            }

            return repeated;
        }

        private static string Trim(string value)
        {
            return value.Length <= MaximumLength ? value : value.Substring(0, MaximumLength).TrimEnd(' ', '.');
        }
    }
}
