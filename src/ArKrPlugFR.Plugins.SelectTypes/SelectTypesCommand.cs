using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// Finds the family types of the project and selects the instances of the ticked ones.
    /// <para>
    /// The window shows the types as a tree — category, family, type — with a search field
    /// and a scope: every family, the placed ones or the ones visible in the current view.
    /// The tool only reads the project: pressing the button selects elements and changes
    /// nothing else.
    /// </para>
    /// </summary>
    public sealed class SelectTypesCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SelectText(context.Culture);
            var viewModel = new SelectTypesViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new SelectTypesWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
