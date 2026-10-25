using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Compare
{
    /// <summary>
    /// Builds the comparison of two elements: the names of both sides together, in
    /// alphabetical order, with the two values of every name.
    /// </summary>
    public static class ParameterComparison
    {
        /// <summary>Builds the comparison.</summary>
        /// <param name="source">Values of the source element, keyed by parameter name.</param>
        /// <param name="target">Values of the target element, keyed by parameter name.</param>
        /// <returns>The rows, in name order.</returns>
        public static IReadOnlyList<ParameterRow> Build(
            IReadOnlyDictionary<string, string?> source,
            IReadOnlyDictionary<string, string?> target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in source.Keys)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name.Trim());
                }
            }

            foreach (string name in target.Keys)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name.Trim());
                }
            }

            var rows = new List<ParameterRow>(names.Count);

            foreach (string name in names)
            {
                source.TryGetValue(name, out string? sourceValue);
                target.TryGetValue(name, out string? targetValue);

                rows.Add(new ParameterRow(name, sourceValue, targetValue));
            }

            return rows;
        }
    }
}
