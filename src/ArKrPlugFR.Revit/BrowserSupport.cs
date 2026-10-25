using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the sheet organization scheme of the project and the folders a sheet sits in.
    /// <para>
    /// The scheme answers three questions the browser tree needs: which sheets the browser
    /// shows at all, which folders a sheet sits in, and which value the sheets of a folder
    /// are sorted by. Everything here only reads.
    /// </para>
    /// </summary>
    public static class BrowserSupport
    {
        /// <summary>Reads the active sheet organization scheme, or <c>null</c> when there is none.</summary>
        /// <param name="document">The project to read.</param>
        public static BrowserOrganization? CurrentForSheets(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return BrowserOrganization.GetCurrentBrowserOrganizationForSheets(document);
        }

        /// <summary>Determines whether the scheme shows a sheet at all.</summary>
        /// <param name="organization">The sheet organization scheme.</param>
        /// <param name="sheetId">Element id of the sheet.</param>
        public static bool IsShown(BrowserOrganization organization, ElementId sheetId)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            return organization.AreFiltersSatisfied(sheetId);
        }

        /// <summary>Reads the folders a sheet sits in, top level first.</summary>
        /// <param name="organization">The sheet organization scheme.</param>
        /// <param name="sheetId">Element id of the sheet.</param>
        /// <returns>The folder names, or an empty list for a sheet at the root.</returns>
        public static IReadOnlyList<string> ReadFolders(BrowserOrganization organization, ElementId sheetId)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            var folders = new List<string>();

            foreach (FolderItemInfo item in organization.GetFolderItems(sheetId))
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.Name))
                {
                    folders.Add(item.Name);
                }
            }

            return folders;
        }

        /// <summary>Reads the value a sheet is sorted by inside its folder.</summary>
        /// <param name="organization">The sheet organization scheme.</param>
        /// <param name="sheet">The sheet to read.</param>
        /// <returns>The sorting value, or the sheet number when the scheme has none.</returns>
        public static string ReadSortingValue(BrowserOrganization organization, ViewSheet sheet)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            if (sheet == null)
            {
                throw new ArgumentNullException(nameof(sheet));
            }

            ElementId sortingId = organization.SortingParameterId;

            if (sortingId != null && sortingId != ElementId.InvalidElementId)
            {
                string? value = ReadByParameterId(sheet, sortingId) ?? ReadByParameterId(ParameterSupport.TypeOf(sheet), sortingId);

                if (value != null)
                {
                    return value;
                }
            }

            return sheet.SheetNumber ?? string.Empty;
        }

        /// <summary>Determines whether the sheets of a folder are sorted descending.</summary>
        /// <param name="organization">The sheet organization scheme.</param>
        public static bool IsDescending(BrowserOrganization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            return organization.SortingOrder == SortingOrder.Descending;
        }

        private static string? ReadByParameterId(Element? element, ElementId parameterId)
        {
            if (element == null)
            {
                return null;
            }

            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Id != parameterId)
                {
                    continue;
                }

                string? text = parameter.AsString();

                if (text == null)
                {
                    text = parameter.AsValueString();
                }

                return text ?? string.Empty;
            }

            return null;
        }
    }
}
