using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Joins
{
    /// <summary>
    /// Turns the per-element join information reported by the modelling software
    /// into a de-duplicated list of pairs.
    /// <para>
    /// Only joins where both elements are in scope are returned. This is the rule
    /// that keeps a tool from unjoining an element that sits outside the current
    /// selection.
    /// </para>
    /// </summary>
    public static class JoinPairs
    {
        /// <summary>
        /// Collects the joins that lie entirely inside the supplied scope.
        /// </summary>
        /// <param name="joinMap">
        /// Joined element ids per element id. Elements that are not in the scope
        /// must not be keys of this map.
        /// </param>
        /// <returns>Distinct pairs, ordered by the first and then the second id.</returns>
        public static IReadOnlyList<JoinPair> Collect(IReadOnlyDictionary<long, IReadOnlyCollection<long>> joinMap)
        {
            if (joinMap == null)
            {
                throw new ArgumentNullException(nameof(joinMap));
            }

            var pairs = new HashSet<JoinPair>();

            foreach (KeyValuePair<long, IReadOnlyCollection<long>> entry in joinMap)
            {
                if (entry.Value == null)
                {
                    continue;
                }

                foreach (long other in entry.Value)
                {
                    if (other == entry.Key || !joinMap.ContainsKey(other))
                    {
                        continue;
                    }

                    pairs.Add(new JoinPair(entry.Key, other));
                }
            }

            return pairs
                .OrderBy(pair => pair)
                .ToList();
        }

        /// <summary>
        /// Counts how many of the collected joins touch each element, which is what
        /// a user wants to see before deciding to break them.
        /// </summary>
        /// <param name="pairs">Pairs collected by <see cref="Collect"/>.</param>
        public static IReadOnlyDictionary<long, int> CountByElement(IReadOnlyCollection<JoinPair> pairs)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            var counts = new Dictionary<long, int>();

            foreach (JoinPair pair in pairs)
            {
                counts[pair.First] = counts.TryGetValue(pair.First, out int first) ? first + 1 : 1;
                counts[pair.Second] = counts.TryGetValue(pair.Second, out int second) ? second + 1 : 1;
            }

            return counts;
        }
    }
}
