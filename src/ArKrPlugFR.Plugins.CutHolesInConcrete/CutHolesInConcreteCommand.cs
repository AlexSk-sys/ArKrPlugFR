using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// Cuts openings in the concrete walls, floors and columns of the current project
    /// where the equipment of a linked model passes through them.
    /// <para>
    /// The user picks the linked model in the window, sets the clearance, and the tool
    /// does the rest in one undoable step: nothing is loaded into the project, no
    /// temporary element is created and the link is never modified.
    /// </para>
    /// </summary>
    public sealed class CutHolesInConcreteCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new CutHolesText(context.Culture);
            var viewModel = new CutHolesViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new CutHolesWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
