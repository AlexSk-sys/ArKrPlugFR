using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.CouplerMarks
{
    /// <summary>
    /// Marks mechanical couplers with the identity of the bar they connect.
    /// <para>
    /// For every picked coupler the tool finds the bar whose centre is nearest to the
    /// central axis of the coupler, inside the tolerance, and writes the mark and the
    /// category of that bar's host into two named parameters of the coupler. The run is one
    /// undoable step and touches nothing else.
    /// </para>
    /// </summary>
    public sealed class CouplerMarksCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new CouplerText(context.Culture);
            var viewModel = new CouplerViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new CouplerWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
