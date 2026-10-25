using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Browser
{
    /// <summary>One folder of the browser tree with the groups and sheets under it.</summary>
    public sealed class SheetGroup
    {
        /// <summary>Creates a group.</summary>
        /// <param name="name">Name of the folder.</param>
        /// <param name="groups">Groups inside the folder, in name order.</param>
        /// <param name="sheets">Sheets of the folder itself, in the order of the scheme.</param>
        public SheetGroup(string name, IReadOnlyList<SheetGroup> groups, IReadOnlyList<SheetFact> sheets)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Groups = groups ?? throw new ArgumentNullException(nameof(groups));
            Sheets = sheets ?? throw new ArgumentNullException(nameof(sheets));
        }

        /// <summary>Gets the name of the folder.</summary>
        public string Name { get; }

        /// <summary>Gets the groups inside the folder.</summary>
        public IReadOnlyList<SheetGroup> Groups { get; }

        /// <summary>Gets the sheets of the folder itself.</summary>
        public IReadOnlyList<SheetFact> Sheets { get; }

        /// <summary>Gets how many sheets the folder holds, its groups included.</summary>
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
    }
}
