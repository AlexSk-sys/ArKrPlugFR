using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>
    /// Exports the ticked sheets as one PDF file per sheet into a chosen folder.
    /// <para>
    /// Nothing in the document changes: the files are written outside the project, so the
    /// tool opens no transaction and the undo list stays untouched.
    /// </para>
    /// </summary>
    public sealed class SheetExportCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SheetExportText(context.Culture);
            var viewModel = new SheetExportViewModel(context.Document, text, context.Log);

            var window = new SheetExportWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
