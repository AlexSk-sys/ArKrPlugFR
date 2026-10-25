using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// Compares the parameters of two picked elements and copies the values of the source
    /// into the target.
    /// <para>
    /// The window lists both sides together and marks what differs; the copy fills the
    /// target in one undoable step and never clears a value, because the source of an empty
    /// value is nothing to copy.
    /// </para>
    /// </summary>
    public sealed class CompareParametersCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new CompareText(context.Culture);
            var viewModel = new CompareViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new CompareWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
