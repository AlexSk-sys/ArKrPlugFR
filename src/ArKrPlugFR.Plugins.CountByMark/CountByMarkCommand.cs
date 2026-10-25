using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>
    /// Writes into every rebar bar how many structures carry its structure mark.
    /// <para>
    /// The user chooses the kind of structures in the window, and the tool does the rest
    /// in one undoable step. Only the number parameter of bars changes; the structures
    /// themselves are never touched.
    /// </para>
    /// </summary>
    public sealed class CountByMarkCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new CountText(context.Culture);
            var viewModel = new CountViewModel(context.Document, text, context.Log);
            var window = new CountWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
