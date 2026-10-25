using System;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// The rule that keeps the sheet's own bookkeeping parameters out of a copy.
    /// <para>
    /// The copy receives its own number and its own name, and a guide grid is
    /// carried over by the annotations switch rather than by the parameters
    /// switch, so parameters whose name speaks about the sheet number, the
    /// sheet name or the guide grid are never copied blindly. The wording is
    /// matched in both languages the add-in supports, because a parameter name
    /// follows the language of the project.
    /// </para>
    /// </summary>
    public static class ParameterCopyRules
    {
        private static readonly string[] ForbiddenWords =
        {
            "sheet number",
            "sheetnumber",
            "sheet name",
            "sheetname",
            "guide grid",
            "guidegrid",
            "номер листа",
            "название листа",
            "имя листа",
            "направляющая сетка",
        };

        /// <summary>Determines whether a parameter with the given name may be copied to a copy.</summary>
        /// <param name="parameterName">Name of the parameter as the project shows it.</param>
        /// <returns><c>true</c> when the parameter must be left alone.</returns>
        public static bool ShouldSkip(string? parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return true;
            }

            foreach (string word in ForbiddenWords)
            {
                if (parameterName!.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a parameter name speaks about guide grids, which the
        /// annotations switch carries over on its own terms.
        /// </summary>
        /// <param name="parameterName">Name of the parameter as the project shows it.</param>
        /// <returns><c>true</c> when the parameter holds the guide-grid reference.</returns>
        public static bool IsGuideGridReference(string? parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            return parameterName!.IndexOf("guide grid", StringComparison.OrdinalIgnoreCase) >= 0
                || parameterName!.IndexOf("guidegrid", StringComparison.OrdinalIgnoreCase) >= 0
                || parameterName!.IndexOf("направляющая сетка", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
