using System.Collections.Generic;
using ArKrPlugFR.Core.Duplication;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// Finds and removes family types and families that repeat what the project
    /// already has.
    /// <para>
    /// The scan, the decision of what may be deleted and the delete itself are
    /// separate: the window only ever ticks rows that the scan already marked as
    /// unused, and the count is read once more before anything is removed.
    /// </para>
    /// </summary>
    public sealed class DuplicateTypeFinderCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            Document document = context.Document;

            IReadOnlyList<FamilyRecord> families = FamilyInventory.Read(document, out IReadOnlyList<string> readErrors);
            DuplicateScanResult result = DuplicateScanner.Scan(families);

            var text = new DuplicateFinderText(context.Culture);
            var viewModel = new DuplicateFinderViewModel(
                document,
                context.UiDocument,
                text,
                context.Log,
                result,
                readErrors);

            var window = new DuplicateTypeWindow(viewModel);
            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
