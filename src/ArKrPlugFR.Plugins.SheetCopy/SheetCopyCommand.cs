using System.Collections.Generic;
using ArKrPlugFR.Core.SheetTransfer;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// Copies the sheets of the active project into the other projects that are open
    /// in the same session.
    /// <para>
    /// The active project is the source and is never written to. The window lists its
    /// sheets together with every open project that can receive them, and the run
    /// carries the ticked sheets into each ticked project under free sheet numbers.
    /// </para>
    /// </summary>
    public sealed class SheetCopyCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            Document source = context.Document;

            var text = new SheetCopyText(context.Culture);
            IReadOnlyList<SheetCandidate> sheets = SheetInventory.ReadSheets(source);

            // The tool works between projects that are open at the same time; the
            // command hands every open document over and the view model decides which
            // of them can actually take sheets and why the others cannot.
            var openDocuments = new List<Document>();

            foreach (Document document in context.UiApplication.Application.Documents)
            {
                openDocuments.Add(document);
            }

            var viewModel = new SheetCopyViewModel(source, text, context.Log, sheets, openDocuments);
            var window = new SheetCopyWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
