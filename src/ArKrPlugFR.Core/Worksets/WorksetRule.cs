using System;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// One row of the user's rule list: what to look at, what to match, and where the
    /// element goes.
    /// <para>
    /// A rule is plain data: it is written to and read from the tool's settings file,
    /// so the properties stay settable and a stored file never fails to load. A rule a
    /// run cannot use — an unknown kind, an empty pattern, a missing workset for a
    /// matching kind — is skipped by <see cref="WorksetRules"/> rather than guessed at.
    /// </para>
    /// </summary>
    public sealed class WorksetRule
    {
        /// <summary>Creates an empty rule of the default kind; used when settings are read.</summary>
        public WorksetRule()
        {
        }

        /// <summary>Creates a rule.</summary>
        /// <param name="kind">Kind of rule; one of <see cref="WorksetRuleKinds"/>.</param>
        /// <param name="pattern">What the rule matches; several values are separated by commas.</param>
        /// <param name="target">Workset the elements go to; unused by the parameter kind.</param>
        public WorksetRule(string kind, string pattern, string target)
        {
            Kind = kind ?? WorksetRuleKinds.Category;
            Pattern = pattern ?? string.Empty;
            Target = target ?? string.Empty;
        }

        /// <summary>Gets or sets the kind of rule; one of <see cref="WorksetRuleKinds"/>.</summary>
        public string Kind { get; set; } = WorksetRuleKinds.Category;

        /// <summary>
        /// Gets or sets what the rule matches: category names, family name prefixes, type
        /// name prefixes, or — for the parameter kind — the name of the parameter.
        /// </summary>
        public string Pattern { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the workset the elements go to. The parameter kind ignores it and
        /// takes the workset name from the parameter value.
        /// </summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>Gets a value indicating whether the rule takes its workset from a parameter value.</summary>
        public bool IsParameterKind =>
            string.Equals(Kind, WorksetRuleKinds.ParameterValue, StringComparison.OrdinalIgnoreCase);

        /// <summary>Determines whether a run can use the rule at all.</summary>
        public bool IsUsable
        {
            get
            {
                if (!WorksetRuleKinds.IsKnown(Kind) || string.IsNullOrWhiteSpace(Pattern))
                {
                    return false;
                }

                return IsParameterKind || !string.IsNullOrWhiteSpace(Target);
            }
        }
    }
}
