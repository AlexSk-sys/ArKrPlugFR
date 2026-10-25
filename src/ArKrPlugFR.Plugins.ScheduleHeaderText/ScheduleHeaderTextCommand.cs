using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.ScheduleHeaderText
{
    /// <summary>
    /// Writes a value into the header of a schedule, taken from the elements the schedule
    /// lists or typed in the window.
    /// <para>
    /// The write is one undoable step and touches one header cell; the rows of the schedule
    /// are left to the model, because the platform does not let a tool write them.
    /// </para>
    /// </summary>
    public sealed class ScheduleHeaderTextCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new HeaderText(context.Culture);
            var viewModel = new HeaderViewModel(context.Document, text, context.Log);

            var window = new HeaderWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
