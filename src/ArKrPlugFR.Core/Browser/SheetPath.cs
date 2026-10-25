using System;
using System.Collections.Generic;
using System.Text;

namespace ArKrPlugFR.Core.Browser
{
    /// <summary>
    /// The path of a row of the browser tree: the folders of a sheet and the sheet itself,
    /// joined the way a user would write them down for a task or a letter.
    /// </summary>
    public static class SheetPath
    {
        /// <summary>What separates the steps of a path.</summary>
        public const string Separator = " \u2192 ";

        /// <summary>Joins the folders of a sheet with the label of the sheet.</summary>
        /// <param name="folders">Folder names, top level first.</param>
        /// <param name="label">Label of the row.</param>
        public static string Join(IReadOnlyList<string> folders, string label)
        {
            if (folders == null)
            {
                throw new ArgumentNullException(nameof(folders));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            var builder = new StringBuilder();

            foreach (string folder in folders)
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(Separator);
                }

                builder.Append(folder.Trim());
            }

            if (builder.Length > 0)
            {
                builder.Append(Separator);
            }

            builder.Append(label);

            return builder.ToString();
        }
    }
}
