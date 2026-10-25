using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Sections
{
    /// <summary>One collected structure.</summary>
    public sealed class SectionRow
    {
        /// <summary>Creates a row.</summary>
        /// <param name="level">Name of the element's level, or a dash.</param>
        /// <param name="category">Name of the element's category.</param>
        /// <param name="typeName">Name of the element's type.</param>
        /// <param name="section">Section read from the type name, or <c>null</c>.</param>
        public SectionRow(string level, string category, string typeName, string? section)
        {
            Level = level ?? string.Empty;
            Category = category ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            Section = section;
        }

        /// <summary>Gets the name of the element's level.</summary>
        public string Level { get; }

        /// <summary>Gets the name of the element's category.</summary>
        public string Category { get; }

        /// <summary>Gets the name of the element's type.</summary>
        public string TypeName { get; }

        /// <summary>Gets the section read from the type name, or <c>null</c>.</summary>
        public string? Section { get; }
    }

    /// <summary>One type with its section and the number of elements.</summary>
    public sealed class SectionItem
    {
        /// <summary>Creates an item.</summary>
        public SectionItem(string typeName, string? section, int count)
        {
            TypeName = typeName;
            Section = section;
            Count = count;
        }

        /// <summary>Gets the name of the type.</summary>
        public string TypeName { get; }

        /// <summary>Gets the section, or <c>null</c>.</summary>
        public string? Section { get; }

        /// <summary>Gets the number of elements.</summary>
        public int Count { get; }
    }

    /// <summary>One category of one level.</summary>
    public sealed class SectionCategoryGroup
    {
        /// <summary>Creates a category group.</summary>
        public SectionCategoryGroup(string category, IReadOnlyList<SectionItem> items)
        {
            Category = category;
            Items = items ?? Array.Empty<SectionItem>();
        }

        /// <summary>Gets the name of the category.</summary>
        public string Category { get; }

        /// <summary>Gets the types of the category.</summary>
        public IReadOnlyList<SectionItem> Items { get; }
    }

    /// <summary>One level of the report.</summary>
    public sealed class SectionLevelGroup
    {
        /// <summary>Creates a level group.</summary>
        public SectionLevelGroup(string level, IReadOnlyList<SectionCategoryGroup> categories)
        {
            Level = level;
            Categories = categories ?? Array.Empty<SectionCategoryGroup>();
        }

        /// <summary>Gets the name of the level.</summary>
        public string Level { get; }

        /// <summary>Gets the categories of the level.</summary>
        public IReadOnlyList<SectionCategoryGroup> Categories { get; }
    }

    /// <summary>
    /// Groups the collected structures by level and category and counts the types.
    /// <para>
    /// The order of the report is fixed — levels, then categories, then type names, each
    /// sorted without case — so the same model always produces the same text.
    /// </para>
    /// </summary>
    public static class SectionCollector
    {
        /// <summary>Groups the rows of a collection into the report shape.</summary>
        /// <param name="rows">The collected structures.</param>
        /// <returns>The report groups, ordered and counted.</returns>
        public static IReadOnlyList<SectionLevelGroup> Group(IEnumerable<SectionRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var levels = new List<SectionLevelGroup>();

            foreach (IGrouping<string, SectionRow> level in rows
                .GroupBy(row => row.Level)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                var categories = new List<SectionCategoryGroup>();

                foreach (IGrouping<string, SectionRow> category in level
                    .GroupBy(row => row.Category)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var items = category
                        .GroupBy(row => row.TypeName)
                        .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(group => new SectionItem(
                            group.Key,
                            group.Select(row => row.Section).FirstOrDefault(section => section != null),
                            group.Count()))
                        .ToList();

                    categories.Add(new SectionCategoryGroup(category.Key, items));
                }

                levels.Add(new SectionLevelGroup(level.Key, categories));
            }

            return levels;
        }
    }
}
