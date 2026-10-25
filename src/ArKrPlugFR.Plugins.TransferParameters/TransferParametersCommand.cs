using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Transfers parameter values: one value onto the ticked sheets, the layer structure
    /// of a wall or a floor into a text parameter, and the value of a room onto the
    /// structures around it.
    /// <para>
    /// The window holds one tab per transfer; each run is one undoable step, and a run
    /// that writes nothing leaves no entry in the undo list.
    /// </para>
    /// </summary>
    public sealed class TransferParametersCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new TransferText(context.Culture);
            var viewModel = new TransferViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new TransferWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
