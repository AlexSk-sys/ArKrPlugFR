using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// Fills the bars of every area reinforcement from the field they belong to.
    /// <para>
    /// The window chooses the scope and the parameter; the run reads the bars of the project
    /// once, groups them by their host, and writes the value of every field into its bars in
    /// one undoable step, keeping the values the bars already carry unless asked otherwise.
    /// </para>
    /// </summary>
    public sealed class AreaRebarMarksCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new AreaRebarText(context.Culture);
            var viewModel = new AreaRebarViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new AreaRebarWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
