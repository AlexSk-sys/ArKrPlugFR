using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Browser
{
    /// <summary>
    /// The whole tree of the sheets: the groups of the scheme and the sheets that sit at
    /// its root without a folder.
    /// </summary>
    public sealed class SheetTree
    {
        /// <summary>Creates a tree.</summary>
        /// <param name="sheets">Sheets at the root of the tree, without a folder.</param>
        /// <param name="groups">Groups of the root, in name order.</param>
        public SheetTree(IReadOnlyList<SheetFact> sheets, IReadOnlyList<SheetGroup> groups)
        {
            Sheets = sheets ?? throw new ArgumentNullException(nameof(sheets));
            Groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }

        /// <summary>Gets the sheets at the root of the tree.</summary>
        public IReadOnlyList<SheetFact> Sheets { get; }

        /// <summary>Gets the groups at the root of the tree.</summary>
        public IReadOnlyList<SheetGroup> Groups { get; }

        /// <summary>Gets how many sheets the tree holds.</summary>
        public int SheetCount
        {
            get
            {
                int total = Sheets.Count;

                foreach (SheetGroup group in Groups)
                {
                    total += group.SheetCount;
                }

                return total;
            }
        }

        /// <summary>Gets how many groups the tree holds, nested ones included.</summary>
        public int GroupCount
        {
            get
            {
                int total = 0;

                foreach (SheetGroup group in Groups)
                {
                    total += CountGroups(group);
                }

                return total;
            }
        }

        private static int CountGroups(SheetGroup group)
        {
            int total = 1;

            foreach (SheetGroup child in group.Groups)
            {
                total += CountGroups(child);
            }

            return total;
        }
    }
}
