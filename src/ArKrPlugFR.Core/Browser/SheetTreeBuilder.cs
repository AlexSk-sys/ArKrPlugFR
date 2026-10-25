using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Browser
{
    /// <summary>
    /// Builds the browser tree of the sheets: the folders of the scheme as the levels of
    /// the tree, the groups ordered by name, and the sheets of a group ordered by the value
    /// the scheme sorts them by — their number where the scheme has none.
    /// </summary>
    public static class SheetTreeBuilder
    {
        /// <summary>Builds the tree of the given sheets.</summary>
        /// <param name="sheets">Sheets to place, in any order.</param>
        /// <param name="descending">Whether the sheets of a group are ordered descending.</param>
        /// <returns>The tree: the root sheets and the groups of the scheme.</returns>
        public static SheetTree Build(IReadOnlyList<SheetFact> sheets, bool descending)
        {
            if (sheets == null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            var root = new Level();

            foreach (SheetFact sheet in sheets)
            {
                if (sheet == null)
                {
                    continue;
                }

                Level level = root;

                foreach (string folder in sheet.Folders)
                {
                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        continue;
                    }

                    string name = folder.Trim();

                    if (!level.Children.TryGetValue(name, out Level? child))
                    {
                        child = new Level();
                        level.Children[name] = child;
                    }

                    level = child;
                }

                level.Sheets.Add(sheet);
            }

            return new SheetTree(Order(root.Sheets, descending), BuildLevel(root, descending));
        }

        private static IReadOnlyList<SheetGroup> BuildLevel(Level level, bool descending)
        {
            var groups = new List<SheetGroup>(level.Children.Count);

            foreach (KeyValuePair<string, Level> child in level.Children)
            {
                groups.Add(new SheetGroup(
                    child.Key,
                    BuildLevel(child.Value, descending),
                    Order(child.Value.Sheets, descending)));
            }

            groups.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

            return groups;
        }

        private static IReadOnlyList<SheetFact> Order(List<SheetFact> sheets, bool descending)
        {
            var ordered = new List<SheetFact>(sheets);

            ordered.Sort((left, right) =>
            {
                int byKey = string.Compare(left.SortKey, right.SortKey, StringComparison.OrdinalIgnoreCase);

                if (byKey != 0)
                {
                    return descending ? -byKey : byKey;
                }

                int byNumber = string.Compare(left.Number, right.Number, StringComparison.OrdinalIgnoreCase);

                return descending ? -byNumber : byNumber;
            });

            return ordered;
        }

        private sealed class Level
        {
            internal SortedDictionary<string, Level> Children { get; } =
                new SortedDictionary<string, Level>(StringComparer.OrdinalIgnoreCase);

            internal List<SheetFact> Sheets { get; } = new List<SheetFact>();
        }
    }
}
