using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// Pulls the picked columns exactly onto the picked grids.
    /// <para>
    /// The user picks the columns and the grids in the window, sets the capture
    /// distance, and the tool does the rest in one undoable step: the columns land on
    /// their grids, the grids are never touched, and nothing else changes.
    /// </para>
    /// </summary>
    public sealed class AlignToGridsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new AlignText(context.Culture);
            var viewModel = new AlignViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new AlignWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
