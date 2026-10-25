using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Browser
{
    /// <summary>
    /// One sheet as the browser tree sees it: the folders it sits in, top level first, its
    /// number and name, and the value the scheme sorts it by.
    /// </summary>
    public sealed class SheetFact
    {
        /// <summary>Creates a fact.</summary>
        /// <param name="folders">Folder names of the sheet, top level first; may be empty.</param>
        /// <param name="number">Number of the sheet.</param>
        /// <param name="name">Name of the sheet.</param>
        /// <param name="sortKey">Value the sheet is sorted by inside its group; may be empty.</param>
        public SheetFact(IReadOnlyList<string> folders, string number, string name, string sortKey)
        {
            Folders = folders ?? throw new ArgumentNullException(nameof(folders));
            Number = number ?? throw new ArgumentNullException(nameof(number));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SortKey = sortKey ?? string.Empty;
        }

        /// <summary>Gets the folder names of the sheet, top level first.</summary>
        public IReadOnlyList<string> Folders { get; }

        /// <summary>Gets the number of the sheet.</summary>
        public string Number { get; }

        /// <summary>Gets the name of the sheet.</summary>
        public string Name { get; }

        /// <summary>Gets the value the sheet is sorted by inside its group.</summary>
        public string SortKey { get; }

        /// <summary>Gets the label of the sheet: its number and its name.</summary>
        public string Label => Number + " \u2014 " + Name;
    }
}
