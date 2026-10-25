using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// Creates many copies of views, schedules and view templates of the active project
    /// in one action.
    /// <para>
    /// The active project is both source and destination: the tool only reads it while
    /// the window is open, and the whole run is one transaction, so a failure at that
    /// level leaves the project as it was.
    /// </para>
    /// </summary>
    public sealed class DuplicateViewsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new DuplicateViewsText(context.Culture);
            var viewModel = new DuplicateViewsViewModel(context.Document, text, context.Log);
            var window = new DuplicateViewsWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
