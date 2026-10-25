using System.Collections.Generic;
using ArKrPlugFR.Core.SheetTransfer;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// Reads the plain sheet descriptions out of a Revit document.
    /// <para>
    /// This is the only place of the tool that turns elements into the values the
    /// transfer rules work on; the rules themselves never see the Revit API.
    /// </para>
    /// </summary>
    internal static class SheetInventory
    {
        /// <summary>
        /// Reads every real sheet of the document with the views placed on it.
        /// Placeholder sheets of a sheet list are left out, and so are their rows.
        /// </summary>
        /// <param name="document">Project to read.</param>
        /// <returns>The sheets, in the order Revit returns them.</returns>
        internal static IReadOnlyList<SheetCandidate> ReadSheets(Document document)
        {
            var sheets = new List<SheetCandidate>();

            var collector = new FilteredElementCollector(document).OfClass(typeof(ViewSheet));

            foreach (ElementId sheetId in collector.ToElementIds())
            {
                if (document.GetElement(sheetId) is not ViewSheet sheet || sheet.IsPlaceholder)
                {
                    continue;
                }

                var views = new List<ViewCandidate>();

                foreach (ElementId viewId in sheet.GetAllPlacedViews())
                {
                    if (document.GetElement(viewId) is View view)
                    {
                        views.Add(new ViewCandidate(ElementIdCompat.GetValue(viewId), view.Name, view.IsTemplate));
                    }
                }

                sheets.Add(new SheetCandidate(
                    ElementIdCompat.GetValue(sheetId),
                    sheet.SheetNumber,
                    sheet.Name,
                    false,
                    views));
            }

            return sheets;
        }

        /// <summary>Reads the sheet numbers a document already uses.</summary>
        /// <param name="document">Project to read.</param>
        /// <returns>Every sheet number of the document, placeholders included.</returns>
        internal static IReadOnlyList<string> ReadSheetNumbers(Document document)
        {
            var numbers = new List<string>();

            var collector = new FilteredElementCollector(document).OfClass(typeof(ViewSheet));

            foreach (ElementId sheetId in collector.ToElementIds())
            {
                if (document.GetElement(sheetId) is ViewSheet sheet)
                {
                    numbers.Add(sheet.SheetNumber);
                }
            }

            return numbers;
        }
    }
}
