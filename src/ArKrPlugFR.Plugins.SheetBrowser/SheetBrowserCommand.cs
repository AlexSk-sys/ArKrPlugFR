using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// Shows the sheets of the project under the folders of the active browser scheme and
    /// copies the path of any row.
    /// <para>
    /// The window reads the scheme once when it opens; the tree shows the whole structure
    /// at a glance, groups and all, with the sorting of the scheme. The tool only reads the
    /// project.
    /// </para>
    /// </summary>
    public sealed class SheetBrowserCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SheetBrowserText(context.Culture);
            var viewModel = new SheetBrowserViewModel(context.Document, text, context.Log);

            var window = new SheetBrowserWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
