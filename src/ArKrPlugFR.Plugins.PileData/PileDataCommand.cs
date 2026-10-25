using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// Numbers the picked piles and writes the elevations and cut heights of a pile
    /// schedule.
    /// <para>
    /// The window holds the settings and the three actions; every action is one undoable
    /// step of its own and writes parameter values only — the geometry is never touched.
    /// </para>
    /// </summary>
    public sealed class PileDataCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new PileText(context.Culture);
            var viewModel = new PileViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new PileWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
