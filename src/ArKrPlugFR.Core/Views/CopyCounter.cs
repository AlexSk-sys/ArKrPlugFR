using System;
using System.Globalization;
using System.Text;

namespace ArKrPlugFR.Core.Views
{
    /// <summary>
    /// The copy counter of one row: digits only, at least one copy, at most the cap.
    /// <para>
    /// The field under the counter is typed into, so what the user types has to be
    /// normalised before it is read: everything but digits is dropped, leading zeros
    /// disappear, and a number above the cap snaps back to the cap.
    /// </para>
    /// </summary>
    public static class CopyCounter
    {
        /// <summary>Fewest copies a row can ask for.</summary>
        public const int Minimum = 1;

        /// <summary>Most copies a row can ask for.</summary>
        public const int Maximum = 99;

        /// <summary>Gets the text a counter starts with.</summary>
        public static string DefaultText => Minimum.ToString(CultureInfo.InvariantCulture);

        /// <summary>Normalises what the user typed into digits within the allowed range.</summary>
        /// <param name="value">Typed text; may be <c>null</c>.</param>
        /// <returns>Digits only, without leading zeros, never above the cap.</returns>
        public static string Normalize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var digits = new StringBuilder(value!.Length);

            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    digits.Append(character);
                }
            }

            string trimmed = digits.ToString().TrimStart('0');

            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            if (!int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed))
            {
                // A number too large to hold is still too large for the run.
                return Format(Maximum);
            }

            return parsed > Maximum ? Format(Maximum) : trimmed;
        }

        /// <summary>Reads a counter text as a number, never below the minimum.</summary>
        /// <param name="text">Text of the counter; may be <c>null</c> or empty.</param>
        /// <returns>The number of copies the run should make.</returns>
        public static int ValueOf(string? text)
        {
            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                && value >= Minimum)
            {
                return value > Maximum ? Maximum : value;
            }

            return Minimum;
        }

        /// <summary>Adds one copy, up to the cap.</summary>
        /// <param name="text">Text of the counter; may be <c>null</c>.</param>
        /// <returns>The text after the increase.</returns>
        public static string Increase(string? text)
        {
            int value = ValueOf(text);

            return Format(value >= Maximum ? Maximum : value + 1);
        }

        /// <summary>Removes one copy, down to one copy.</summary>
        /// <param name="text">Text of the counter; may be <c>null</c>.</param>
        /// <returns>The text after the decrease.</returns>
        public static string Decrease(string? text)
        {
            int value = ValueOf(text);

            return Format(value <= Minimum ? Minimum : value - 1);
        }

        private static string Format(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
