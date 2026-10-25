using System;

namespace ArKrPlugFR.Core.Parameters
{
    /// <summary>
    /// How a parameter name is compared and when it is usable.
    /// <para>
    /// Revit treats the parameter names of one element as unique without case, so the
    /// tool does the same: a name the user typed finds the parameter whatever case it
    /// was written in, and a name that is empty or spaces is no target at all.
    /// </para>
    /// </summary>
    public static class ParameterNameRules
    {
        /// <summary>Determines whether a typed name can be used at all.</summary>
        /// <param name="name">Name typed by the user.</param>
        public static bool IsUsable(string? name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        /// <summary>Determines whether two parameter names mean the same parameter.</summary>
        /// <param name="wanted">Name the user typed.</param>
        /// <param name="candidate">Name read from an element.</param>
        public static bool Matches(string? wanted, string? candidate)
        {
            if (wanted == null || candidate == null)
            {
                return false;
            }

            return string.Equals(wanted.Trim(), candidate.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
