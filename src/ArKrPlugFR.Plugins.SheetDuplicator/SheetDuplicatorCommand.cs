using System.Collections.Generic;
using ArKrPlugFR.Core.Sheets;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Revit.Compatibility;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// Duplicates the sheets of the current project together with their views.
    /// <para>
    /// The command reads the sheets once, opens one window owned by Revit, and
    /// leaves the whole duplication to the view model, so the tool works on the
    /// project the user already has open and never touches another document.
    /// </para>
    /// </summary>
    public sealed class SheetDuplicatorCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SheetDuplicatorText(context.Culture);

            IReadOnlyList<SheetFacts> sheets = ReadSheets(context.Document);

            var viewModel = new SheetDuplicatorViewModel(context.Document, text, context.Log, sheets);
            var window = new SheetDuplicatorWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }

        /// <summary>
        /// Reads the plain sheet facts out of the document.
        /// <para>
        /// This is the only place of the tool that turns elements into the values
        /// the rules work on; the rules themselves never see the Revit API.
        /// </para>
        /// </summary>
        /// <param name="document">Project to read.</param>
        /// <returns>Every sheet, placeholders included; the rules pick the usable ones.</returns>
        private static IReadOnlyList<SheetFacts> ReadSheets(Document document)
        {
            var sheets = new List<SheetFacts>();

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(ViewSheet)))
            {
                if (element is not ViewSheet sheet)
                {
                    continue;
                }

                sheets.Add(new SheetFacts(
                    ElementIdCompat.GetValue(sheet.Id),
                    sheet.SheetNumber,
                    sheet.Name,
                    sheet.IsPlaceholder,
                    sheet.IsValidObject));
            }

            return sheets;
        }
    }
}
