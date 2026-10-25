using System;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// The kinds of rule a workset run understands.
    /// <para>
    /// A kind is stored as a short word rather than a number, so a settings file stays
    /// readable and a word from a newer version can be skipped instead of turning into
    /// an arbitrary rule.
    /// </para>
    /// </summary>
    public static class WorksetRuleKinds
    {
        /// <summary>Matches the category of an element.</summary>
        public const string Category = "category";

        /// <summary>Matches the name of the family an element comes from.</summary>
        public const string FamilyPrefix = "family";

        /// <summary>Matches the name of the type of an element.</summary>
        public const string TypePrefix = "type";

        /// <summary>Takes the workset name from the value of a parameter.</summary>
        public const string ParameterValue = "parameter";

        /// <summary>Every kind, in the order the window offers them.</summary>
        public static readonly string[] All = { Category, FamilyPrefix, TypePrefix, ParameterValue };

        /// <summary>Determines whether a kind is one this build understands.</summary>
        /// <param name="kind">Kind of a rule.</param>
        public static bool IsKnown(string? kind)
        {
            foreach (string known in All)
            {
                if (string.Equals(known, kind, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
