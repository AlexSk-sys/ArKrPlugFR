using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>
    /// Extends the analytical model of the picked columns and walls up to the nearest floor
    /// or beam above them.
    /// <para>
    /// The run is one undoable step: it writes the analytical lines, or raises the physical
    /// columns where the release does not allow the line to be written, and reports every
    /// element it had to leave alone.
    /// </para>
    /// </summary>
    public sealed class AnalyticalExtensionCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new ExtensionText(context.Culture);
            var viewModel = new ExtensionViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new ExtensionWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
