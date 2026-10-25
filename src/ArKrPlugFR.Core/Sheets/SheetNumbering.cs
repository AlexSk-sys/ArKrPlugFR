using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// The numbering rule for the copies of one sheet.
    /// <para>
    /// A copy is numbered by appending an underscore and a running number to the
    /// source number: source A-01 gives A-01_1, A-01_2 and so on. The base of
    /// the run is computed once per source sheet from the numbers the document
    /// already carries, so a project that already has A-01_3 gets A-01_4 next.
    /// The source sheet itself is never renamed.
    /// </para>
    /// </summary>
    public static class SheetNumbering
    {
        /// <summary>
        /// Finds the highest underscore suffix among the numbers that extend the
        /// source number; zero when the project has none.
        /// </summary>
        /// <param name="sourceNumber">Number of the sheet being copied.</param>
        /// <param name="existingNumbers">Every sheet number of the document.</param>
        /// <returns>The highest numeric suffix found, or zero.</returns>
        public static int FindHighestSuffix(string? sourceNumber, IEnumerable<string> existingNumbers)
        {
            if (sourceNumber == null || sourceNumber.Length == 0)
            {
                throw new ArgumentException("The source sheet number must not be empty.", nameof(sourceNumber));
            }

            if (existingNumbers == null)
            {
                throw new ArgumentNullException(nameof(existingNumbers));
            }

            int highest = 0;

            foreach (string number in existingNumbers)
            {
                if (number == null || !number.StartsWith(sourceNumber, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string remainder = number.Substring(sourceNumber.Length);

                if (remainder.Length < 2 || remainder[0] != '_')
                {
                    continue;
                }

                string suffix = remainder.Substring(1);

                if (!int.TryParse(suffix, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int value))
                {
                    continue;
                }

                if (value > highest)
                {
                    highest = value;
                }
            }

            return highest;
        }

        /// <summary>Builds the numbers of a run of copies, starting after the highest existing suffix.</summary>
        /// <param name="sourceNumber">Number of the sheet being copied.</param>
        /// <param name="existingNumbers">Every sheet number of the document.</param>
        /// <param name="count">How many copies need a number.</param>
        /// <returns>One number per copy, in copy order.</returns>
        public static IReadOnlyList<string> GenerateNumbers(
            string? sourceNumber,
            IEnumerable<string> existingNumbers,
            int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "The copy count must not be negative.");
            }

            int baseSuffix = FindHighestSuffix(sourceNumber, existingNumbers);
            var numbers = new List<string>(count);

            for (int offset = 1; offset <= count; offset++)
            {
                numbers.Add(WithSuffix(sourceNumber!, baseSuffix + offset));
            }

            return numbers;
        }

        /// <summary>Appends an underscore and the given suffix to a sheet number.</summary>
        /// <param name="sourceNumber">Number of the sheet being copied.</param>
        /// <param name="suffix">Positive suffix to append.</param>
        /// <returns>The generated number.</returns>
        public static string WithSuffix(string sourceNumber, int suffix)
        {
            if (string.IsNullOrEmpty(sourceNumber))
            {
                throw new ArgumentException("The source sheet number must not be empty.", nameof(sourceNumber));
            }

            if (suffix < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(suffix), "The suffix starts at one.");
            }

            return sourceNumber + "_" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
