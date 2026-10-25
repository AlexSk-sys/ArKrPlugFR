using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Types
{
    /// <summary>
    /// Builds the type tree: categories, the families inside them, and the types inside
    /// those — the shape the project browser uses, flattened into one list per level and
    /// sorted by name so two runs look the same.
    /// </summary>
    public static class TypeCatalog
    {
        /// <summary>Builds the tree of the given types.</summary>
        /// <param name="facts">Types to show.</param>
        /// <param name="query">Search query; empty keeps every type.</param>
        /// <param name="uncategorisedLabel">Name used for types that carry no category.</param>
        /// <returns>The categories of the tree, in name order.</returns>
        public static IReadOnlyList<CategoryGroup> Build(
            IReadOnlyList<TypeFact> facts,
            string? query,
            string uncategorisedLabel)
        {
            if (facts == null)
            {
                throw new ArgumentNullException(nameof(facts));
            }

            if (string.IsNullOrWhiteSpace(uncategorisedLabel))
            {
                throw new ArgumentException("The label for types without a category must be given.", nameof(uncategorisedLabel));
            }

            var typesByCategory = new SortedDictionary<string, SortedDictionary<string, List<TypeFact>>>(StringComparer.OrdinalIgnoreCase);

            foreach (TypeFact fact in facts)
            {
                if (fact == null || !TypeSearch.Matches(fact.Family, fact.Type, query))
                {
                    continue;
                }

                string category = string.IsNullOrWhiteSpace(fact.Category) ? uncategorisedLabel : fact.Category!.Trim();

                if (!typesByCategory.TryGetValue(category, out SortedDictionary<string, List<TypeFact>>? families))
                {
                    families = new SortedDictionary<string, List<TypeFact>>(StringComparer.OrdinalIgnoreCase);
                    typesByCategory[category] = families;
                }

                if (!families.TryGetValue(fact.Family, out List<TypeFact>? types))
                {
                    types = new List<TypeFact>();
                    families[fact.Family] = types;
                }

                types.Add(fact);
            }

            var categories = new List<CategoryGroup>(typesByCategory.Count);

            foreach (KeyValuePair<string, SortedDictionary<string, List<TypeFact>>> category in typesByCategory)
            {
                var familyGroups = new List<FamilyGroup>(category.Value.Count);

                foreach (KeyValuePair<string, List<TypeFact>> family in category.Value)
                {
                    family.Value.Sort((left, right) => string.Compare(left.Type, right.Type, StringComparison.OrdinalIgnoreCase));
                    familyGroups.Add(new FamilyGroup(family.Key, family.Value));
                }

                categories.Add(new CategoryGroup(category.Key, familyGroups));
            }

            return categories;
        }
    }
}
