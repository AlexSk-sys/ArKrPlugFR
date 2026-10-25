using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>
    /// Brings analytical members to the height of a target member and analytical nodes onto
    /// its line.
    /// <para>
    /// Each of the two alignments is one undoable step of its own; the physical model, the
    /// loads and the boundary conditions are never touched.
    /// </para>
    /// </summary>
    public sealed class AnalyticalAlignmentCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new AlignmentText(context.Culture);
            var viewModel = new AlignmentViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new AlignmentWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
