using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Export;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Exports sheets as PDF files, on every release that offers it.
    /// <para>
    /// The platform gained its PDF export in Revit 2022: the call takes the folder, the
    /// sheets and an options object that says whether the set is combined into one file and
    /// how the files are named. Revit 2020 has no such call at all, and the tool that uses
    /// this helper is not offered there; the branch keeps the release difference in one
    /// place instead of spreading it over the tool.
    /// </para>
    /// </summary>
    public static class SheetExportCompat
    {
        /// <summary>
        /// Gets a value indicating whether the release can export a sheet as a PDF file.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
#if REVIT_2022 || REVIT_2024
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>Exports sheets as one PDF file per sheet.</summary>
        /// <param name="document">The document to export from.</param>
        /// <param name="folder">Folder the files are written into.</param>
        /// <param name="sheetIds">The sheets to export.</param>
        public static void ExportPdf(Document document, string folder, IList<ElementId> sheetIds)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("The folder must be given.", nameof(folder));
            }

            if (sheetIds == null)
            {
                throw new ArgumentNullException(nameof(sheetIds));
            }

#if REVIT_2022 || REVIT_2024
            var options = new PDFExportOptions
            {
                // One file per sheet, named after the sheet; a failing sheet does not stop
                // the rest of the set.
                Combine = false,
                StopOnError = false,
            };

            options.SetNamingRule(NamingRule());

            document.Export(folder, sheetIds, options);
#else
            throw new NotSupportedException(
                "This release of the platform has no API to export a sheet as a PDF file.");
#endif
        }

#if REVIT_2022 || REVIT_2024
        private static IList<TableCellCombinedParameterData> NamingRule()
        {
            TableCellCombinedParameterData number = TableCellCombinedParameterData.Create();
            number.ParamId = new ElementId(BuiltInParameter.SHEET_NUMBER);
            number.CategoryId = new ElementId(BuiltInCategory.OST_Sheets);

            TableCellCombinedParameterData name = TableCellCombinedParameterData.Create();
            name.ParamId = new ElementId(BuiltInParameter.SHEET_NAME);
            name.CategoryId = new ElementId(BuiltInCategory.OST_Sheets);
            name.Prefix = SheetFileNames.Separator;

            return new List<TableCellCombinedParameterData> { number, name };
        }
#endif
    }
}
