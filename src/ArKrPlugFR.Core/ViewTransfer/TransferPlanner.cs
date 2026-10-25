using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// Turns the views read from a source project into the plan the tool shows
    /// and transfers.
    /// <para>
    /// The planner applies the rules of the tool: it drops every view that is not
    /// transferable, gives every copy a name that is unique in the target, and
    /// groups the rest by view type. Group order here is a stable default; the
    /// plugin re-sorts by the localized group name before showing the tree.
    /// </para>
    /// </summary>
    public static class TransferPlanner
    {
        /// <summary>Prefix that marks a group named after a raw view type.</summary>
        private const string RawGroupPrefix = "raw:";

        /// <summary>Builds a plan from the views of a source project.</summary>
        /// <param name="candidates">Views read from the source project.</param>
        /// <param name="occupiedNames">
        /// Names already used by views of the target project; the planned names
        /// avoid them. May be null when the target is not known.
        /// </param>
        /// <returns>The plan; empty when nothing is transferable.</returns>
        public static TransferPlan Create(
            IReadOnlyList<ViewCandidate> candidates,
            IEnumerable<string>? occupiedNames = null)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var takenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (occupiedNames != null)
            {
                foreach (string occupied in occupiedNames)
                {
                    if (!string.IsNullOrWhiteSpace(occupied))
                    {
                        takenNames.Add(occupied.Trim());
                    }
                }
            }

            var seenIds = new HashSet<long>();
            var groups = new Dictionary<string, List<TransferItem>>(StringComparer.Ordinal);

            foreach (ViewCandidate candidate in candidates)
            {
                if (!ViewTransferRules.IsTransferable(candidate) || !seenIds.Add(candidate.Id))
                {
                    continue;
                }

                string sourceName = ViewNaming.Normalize(candidate.Name);
                string plannedName = ViewNaming.MakeUnique(sourceName, takenNames);
                takenNames.Add(plannedName);

                string groupKey = RawGroupPrefix + candidate.ViewTypeName;

                if (ViewGroupCatalog.TryGetGroupKey(candidate.ViewTypeName, out string catalogKey))
                {
                    groupKey = catalogKey;
                }

                if (!groups.TryGetValue(groupKey, out List<TransferItem>? items))
                {
                    items = new List<TransferItem>();
                    groups[groupKey] = items;
                }

                items.Add(new TransferItem(
                    candidate.Id,
                    sourceName,
                    plannedName,
                    candidate.ViewTypeName,
                    candidate.IsTemplate));
            }

            var orderedGroups = new List<TransferGroup>();

            foreach (string groupKey in groups.Keys.OrderBy(key => key, StringComparer.Ordinal))
            {
                List<TransferItem> items = groups[groupKey];
                items.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

                bool isRaw = groupKey.StartsWith(RawGroupPrefix, StringComparison.Ordinal);

                orderedGroups.Add(new TransferGroup(
                    isRaw ? null : groupKey,
                    isRaw ? groupKey.Substring(RawGroupPrefix.Length) : null,
                    items));
            }

            return new TransferPlan(orderedGroups);
        }
    }
}
