using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// Finds family types and whole families that repeat content already in the
    /// project.
    /// <para>
    /// Three questions are answered: which types inside one family carry the same
    /// values, which families offer the same set of types, and which family is a
    /// contained copy of a larger one. The scan is deterministic: equal candidates
    /// are resolved by name or by placed-instance count, never by enumeration
    /// order, so two runs on the same model report the same rows.
    /// </para>
    /// </summary>
    public static class DuplicateScanner
    {
        /// <summary>Scans the families of a project.</summary>
        /// <param name="families">Families loaded in the project.</param>
        /// <param name="options">Comparison settings; defaults are used when omitted.</param>
        public static DuplicateScanResult Scan(IReadOnlyList<FamilyRecord> families, DuplicateScanOptions? options = null)
        {
            if (families == null)
            {
                throw new ArgumentNullException(nameof(families));
            }

            DuplicateScanOptions settings = options ?? new DuplicateScanOptions();

            List<FamilyRecord> ordered = families
                .OrderBy(family => family.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(family => family.Name, StringComparer.Ordinal)
                .ToList();

            var signatures = new Dictionary<TypeRecord, TypeSignature>();

            TypeSignature Signature(TypeRecord type)
            {
                if (!signatures.TryGetValue(type, out TypeSignature? signature))
                {
                    signature = TypeSignature.Create(type, settings);
                    signatures[type] = signature;
                }

                return signature;
            }

            IReadOnlyList<DuplicateTypeRow> typeRows = CollectTypeDuplicates(ordered, Signature);

            var sets = new Dictionary<FamilyRecord, SignatureSet>();

            foreach (FamilyRecord family in ordered)
            {
                sets[family] = new SignatureSet(family.Types.Select(Signature));
            }

            IReadOnlyList<DuplicateFamilyRow> familyRows = CollectFamilyDuplicates(ordered, sets);

            IReadOnlyList<NearDuplicatePair> nearDuplicates =
                typeRows.Count == 0 && familyRows.Count == 0
                    ? CollectNearDuplicates(ordered, Signature, settings)
                    : Array.Empty<NearDuplicatePair>();

            return new DuplicateScanResult(ordered.Sum(family => family.Types.Count), typeRows, familyRows, nearDuplicates);
        }

        private static IReadOnlyList<DuplicateTypeRow> CollectTypeDuplicates(
            IReadOnlyList<FamilyRecord> families,
            Func<TypeRecord, TypeSignature> signature)
        {
            var rows = new List<DuplicateTypeRow>();

            foreach (FamilyRecord family in families)
            {
                var groups = new Dictionary<TypeSignature, List<TypeRecord>>();

                foreach (TypeRecord type in family.Types)
                {
                    TypeSignature key = signature(type);

                    if (!groups.TryGetValue(key, out List<TypeRecord>? group))
                    {
                        group = new List<TypeRecord>();
                        groups[key] = group;
                    }

                    group.Add(type);
                }

                foreach (List<TypeRecord> group in groups.Values)
                {
                    if (group.Count < 2)
                    {
                        continue;
                    }

                    List<TypeRecord> sorted = group
                        .OrderBy(type => type.TypeName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(type => type.TypeName, StringComparer.Ordinal)
                        .ThenBy(type => type.Id)
                        .ToList();

                    for (int index = 1; index < sorted.Count; index++)
                    {
                        rows.Add(new DuplicateTypeRow(family.Name, family.CategoryName, sorted[index], sorted[0]));
                    }
                }
            }

            return rows
                .OrderBy(row => row.FamilyName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Duplicate.TypeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Duplicate.Id)
                .ToList();
        }

        private static IReadOnlyList<DuplicateFamilyRow> CollectFamilyDuplicates(
            IReadOnlyList<FamilyRecord> families,
            IReadOnlyDictionary<FamilyRecord, SignatureSet> sets)
        {
            // A family without types takes part in nothing: an empty set would
            // otherwise look like a copy of every other family.
            List<FamilyRecord> withTypes = families.Where(family => family.Types.Count > 0).ToList();

            var used = new HashSet<FamilyRecord>();
            var rows = new List<DuplicateFamilyRow>();

            var bySet = new Dictionary<SignatureSet, List<FamilyRecord>>();

            foreach (FamilyRecord family in withTypes)
            {
                SignatureSet set = sets[family];

                if (!bySet.TryGetValue(set, out List<FamilyRecord>? group))
                {
                    group = new List<FamilyRecord>();
                    bySet[set] = group;
                }

                group.Add(family);
            }

            foreach (List<FamilyRecord> group in bySet.Values)
            {
                if (group.Count < 2)
                {
                    continue;
                }

                // Full copies: the family with the most placed instances is kept,
                // because it is the one the project already relies on.
                List<FamilyRecord> sorted = group
                    .OrderByDescending(family => family.InstanceCount)
                    .ThenBy(family => family.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(family => family.Name, StringComparer.Ordinal)
                    .ToList();

                for (int index = 1; index < sorted.Count; index++)
                {
                    rows.Add(new DuplicateFamilyRow(sorted[index], sorted[0], sets[sorted[index]].Count));
                }

                foreach (FamilyRecord family in group)
                {
                    used.Add(family);
                }
            }

            // Contained copies: a family whose every type also exists in a larger
            // family. Each family takes part in at most one row.
            List<FamilyRecord> candidates = withTypes
                .Where(family => !used.Contains(family))
                .OrderBy(family => sets[family].Count)
                .ThenBy(family => family.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(family => family.Name, StringComparer.Ordinal)
                .ToList();

            foreach (FamilyRecord candidate in candidates)
            {
                if (used.Contains(candidate))
                {
                    continue;
                }

                FamilyRecord? keeper = null;

                foreach (FamilyRecord other in candidates)
                {
                    if (ReferenceEquals(candidate, other) || used.Contains(other))
                    {
                        continue;
                    }

                    if (!sets[candidate].IsProperSubsetOf(sets[other]))
                    {
                        continue;
                    }

                    if (keeper == null
                        || sets[other].Count > sets[keeper].Count
                        || (sets[other].Count == sets[keeper].Count && IsNameBefore(other.Name, keeper.Name)))
                    {
                        keeper = other;
                    }
                }

                if (keeper == null)
                {
                    continue;
                }

                rows.Add(new DuplicateFamilyRow(candidate, keeper, sets[candidate].Count));
                used.Add(candidate);
                used.Add(keeper);
            }

            return rows
                .OrderBy(row => row.Duplicate.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Duplicate.Name, StringComparer.Ordinal)
                .ToList();
        }

        private static IReadOnlyList<NearDuplicatePair> CollectNearDuplicates(
            IReadOnlyList<FamilyRecord> families,
            Func<TypeRecord, TypeSignature> signature,
            DuplicateScanOptions options)
        {
            var pairs = new List<NearDuplicatePair>();

            foreach (FamilyRecord family in families)
            {
                if (pairs.Count >= options.NearDuplicateReportLimit)
                {
                    break;
                }

                IReadOnlyList<TypeRecord> types = family.Types;
                var differences = new Dictionary<(TypeRecord First, TypeRecord Second), int>();

                int examined = 0;

                for (int first = 0; first < types.Count; first++)
                {
                    for (int second = first + 1; second < types.Count; second++)
                    {
                        if (examined >= options.NearDuplicatePairLimitPerFamily
                            || pairs.Count >= options.NearDuplicateReportLimit)
                        {
                            break;
                        }

                        examined++;

                        int count = signature(types[first]).DifferenceCount(signature(types[second]));

                        if (count < options.NearDuplicateMinimumDifferences || count > options.NearDuplicateMaximumDifferences)
                        {
                            continue;
                        }

                        differences[(types[first], types[second])] = count;
                    }
                }

                foreach (KeyValuePair<(TypeRecord First, TypeRecord Second), int> entry in differences
                    .OrderBy(entry => entry.Value)
                    .ThenBy(entry => entry.Key.First.TypeName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(entry => entry.Key.Second.TypeName, StringComparer.OrdinalIgnoreCase))
                {
                    if (pairs.Count >= options.NearDuplicateReportLimit)
                    {
                        break;
                    }

                    pairs.Add(new NearDuplicatePair(
                        family.Name,
                        entry.Key.First.TypeName,
                        entry.Key.Second.TypeName,
                        entry.Value));
                }
            }

            return pairs;
        }

        private static bool IsNameBefore(string left, string right)
        {
            int comparison = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);

            return comparison < 0 || (comparison == 0 && string.CompareOrdinal(left, right) < 0);
        }
    }
}
