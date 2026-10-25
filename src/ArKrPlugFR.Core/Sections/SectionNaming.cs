using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ArKrPlugFR.Core.Sections
{
    /// <summary>
    /// Reads the section of a structure out of its type name.
    /// <para>
    /// Type names carry the section in the shapes people write them: "400х600",
    /// "400 x 600", "D20", "200 мм". The first pair of numbers is the section of a
    /// rectangular profile, a single "D" number is a diameter, a lone number is a
    /// thickness. The name is never guessed at: a name without numbers has no section.
    /// </para>
    /// </summary>
    public static class SectionNaming
    {
        private static readonly Regex PairPattern = new Regex(
            @"(\d+(?:[.,]\d+)?)\s*[xх×*]\s*(\d+(?:[.,]\d+)?)",
            RegexOptions.CultureInvariant);

        private static readonly Regex DiameterPattern = new Regex(
            @"[dD]\s*(\d+(?:[.,]\d+)?)",
            RegexOptions.CultureInvariant);

        private static readonly Regex SinglePattern = new Regex(
            @"(\d+(?:[.,]\d+)?)",
            RegexOptions.CultureInvariant);

        /// <summary>Reads the section from a type name.</summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>The section as text, for example "400×600 мм"; <c>null</c> when the name carries none.</returns>
        public static string? Parse(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            Match pair = PairPattern.Match(typeName);

            if (pair.Success)
            {
                return Number(pair.Groups[1].Value) + "×" + Number(pair.Groups[2].Value) + " мм";
            }

            Match diameter = DiameterPattern.Match(typeName);

            if (diameter.Success)
            {
                return "D" + Number(diameter.Groups[1].Value) + " мм";
            }

            Match single = SinglePattern.Match(typeName);

            if (single.Success)
            {
                return Number(single.Groups[1].Value) + " мм";
            }

            return null;
        }

        private static string Number(string value)
        {
            return value.Replace(',', '.');
        }
    }
}
