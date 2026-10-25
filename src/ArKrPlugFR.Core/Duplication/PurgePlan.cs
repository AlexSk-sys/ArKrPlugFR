using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// Splits the items the user ticked into the ones that may be deleted and the
    /// ones that are still in use.
    /// <para>
    /// This is the safety rule of the tool: an item with placed instances is never
    /// deleted, because deleting it would take the instances with it. An item whose
    /// usage cannot be determined counts as in use.
    /// </para>
    /// </summary>
    public sealed class PurgePlan
    {
        private PurgePlan(IReadOnlyList<long> doomed, IReadOnlyList<long> blocked)
        {
            Doomed = doomed;
            Blocked = blocked;
        }

        /// <summary>Gets the ids that may be deleted.</summary>
        public IReadOnlyList<long> Doomed { get; }

        /// <summary>Gets the ids that are in use, or whose usage is unknown, and must be kept.</summary>
        public IReadOnlyList<long> Blocked { get; }

        /// <summary>Builds a plan from the ticked ids and the current instance counts.</summary>
        /// <param name="selected">Ids the user ticked.</param>
        /// <param name="instanceCounts">Placed instances per type id, read immediately before the delete.</param>
        public static PurgePlan Build(IEnumerable<long> selected, IReadOnlyDictionary<long, int> instanceCounts)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }

            if (instanceCounts == null)
            {
                throw new ArgumentNullException(nameof(instanceCounts));
            }

            var doomed = new List<long>();
            var blocked = new List<long>();

            foreach (long id in selected.Distinct())
            {
                if (instanceCounts.TryGetValue(id, out int count) && count == 0)
                {
                    doomed.Add(id);
                }
                else
                {
                    blocked.Add(id);
                }
            }

            return new PurgePlan(doomed, blocked);
        }
    }
}
