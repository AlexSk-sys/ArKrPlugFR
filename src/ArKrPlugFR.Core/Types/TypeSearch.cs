using System;

namespace ArKrPlugFR.Core.Types
{
    /// <summary>
    /// The search of the type tree.
    /// <para>
    /// The query is a piece of a name, not a pattern: an empty query keeps everything, and
    /// anything else is matched without case against the family name and the type name, so
    /// "concrete" finds the families whose name carries the word and the types whose name
    /// does.
    /// </para>
    /// </summary>
    public static class TypeSearch
    {
        /// <summary>Determines whether a type matches the query.</summary>
        /// <param name="family">Name of the family.</param>
        /// <param name="type">Name of the type.</param>
        /// <param name="query">What the user typed; empty keeps everything.</param>
        public static bool Matches(string? family, string? type, string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            string wanted = query!.Trim();

            return Contains(family, wanted) || Contains(type, wanted);
        }

        private static bool Contains(string? value, string wanted)
        {
            return value != null && value.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
