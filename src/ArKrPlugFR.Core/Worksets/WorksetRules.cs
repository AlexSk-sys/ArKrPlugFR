using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// The decision the tool makes for every element: which rule applies first and which
    /// workset that rule asks for.
    /// <para>
    /// Rules are tried in the order the user sees them and the first usable match wins,
    /// so a broad category rule can sit after a narrow prefix rule and never swallow it.
    /// A rule that names several categories or prefixes — separated by commas or
    /// semicolons — matches any one of them.
    /// </para>
    /// </summary>
    public static class WorksetRules
    {
        /// <summary>Reads the parameter names a run has to look at.</summary>
        /// <param name="rules">Rules of the run.</param>
        /// <returns>The distinct names of the usable parameter rules, in order.</returns>
        public static IReadOnlyList<string> ParameterNames(IReadOnlyList<WorksetRule> rules)
        {
            var names = new List<string>();

            if (rules == null)
            {
                return names;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (WorksetRule rule in rules)
            {
                if (rule == null || !rule.IsUsable || !rule.IsParameterKind)
                {
                    continue;
                }

                string name = rule.Pattern.Trim();

                if (seen.Add(name))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        /// <summary>Picks the workset for one element; the first usable matching rule wins.</summary>
        /// <param name="rules">Rules of the run, in the order shown to the user.</param>
        /// <param name="facts">What the rules may look at on the element.</param>
        /// <returns>The workset name, or <c>null</c> when no rule applies.</returns>
        public static string? SelectTarget(IReadOnlyList<WorksetRule> rules, WorksetElementFacts facts)
        {
            if (rules == null || facts == null)
            {
                return null;
            }

            foreach (WorksetRule rule in rules)
            {
                if (rule == null || !rule.IsUsable || !Matches(rule, facts))
                {
                    continue;
                }

                string? target = Normalize(rule.IsParameterKind ? facts.Parameter(rule.Pattern) : rule.Target);

                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        /// <summary>Determines whether a usable rule matches the facts of an element.</summary>
        /// <param name="rule">Rule to apply; it must be usable.</param>
        /// <param name="facts">What the rules may look at on the element.</param>
        public static bool Matches(WorksetRule rule, WorksetElementFacts facts)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (facts == null)
            {
                throw new ArgumentNullException(nameof(facts));
            }

            if (string.Equals(rule.Kind, WorksetRuleKinds.Category, StringComparison.OrdinalIgnoreCase))
            {
                foreach (string candidate in Split(rule.Pattern))
                {
                    if (EqualsWithoutCase(candidate, facts.Category) || EqualsWithoutCase(candidate, facts.CategoryIdentifier))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (string.Equals(rule.Kind, WorksetRuleKinds.FamilyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return StartsWithAny(facts.Family, rule.Pattern);
            }

            if (string.Equals(rule.Kind, WorksetRuleKinds.TypePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return StartsWithAny(facts.Type, rule.Pattern);
            }

            if (rule.IsParameterKind)
            {
                // The rule matches only when the parameter actually carries a value;
                // an empty value falls through to the rules below it instead of
                // swallowing the element.
                return facts.Parameter(rule.Pattern) != null;
            }

            return false;
        }

        /// <summary>Trims a value and treats an empty result as absent.</summary>
        /// <param name="value">Value to normalise.</param>
        /// <returns>The trimmed value, or <c>null</c> when nothing is left.</returns>
        public static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
        }

        private static bool EqualsWithoutCase(string left, string? right)
        {
            return right != null && string.Equals(left, right.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool StartsWithAny(string? value, string pattern)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (string prefix in Split(pattern))
            {
                if (value!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> Split(string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                yield break;
            }

            foreach (string part in pattern!.Split(new[] { ',', ';' }))
            {
                string trimmed = part.Trim();

                if (trimmed.Length > 0)
                {
                    yield return trimmed;
                }
            }
        }
    }
}
