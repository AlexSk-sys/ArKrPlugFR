using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>
    /// Hides the columns of a schedule whose every row is empty or zero, and shows the hidden
    /// columns that carry values again.
    /// <para>
    /// The run is one undoable step and touches the visibility of the columns only: no row,
    /// no field and no element of the model changes.
    /// </para>
    /// </summary>
    public sealed class ScheduleColumnsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new ColumnText(context.Culture);
            var viewModel = new ColumnViewModel(context.Document, text, context.Log);

            var window = new ColumnWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
