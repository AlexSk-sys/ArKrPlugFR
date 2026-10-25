using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// Assigns the elements of a workshared project to worksets by the user's ordered
    /// rules.
    /// <para>
    /// The user edits the rules in the window, presses one button, and the tool does the
    /// rest in one undoable step: the worksets the rules ask for are created when they do
    /// not exist, and every element the rules match is moved into its workset.
    /// </para>
    /// </summary>
    public sealed class AssignWorksetsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new WorksetText(context.Culture);
            var viewModel = new WorksetViewModel(context.Document, text, context.Log);

            var window = new WorksetWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
