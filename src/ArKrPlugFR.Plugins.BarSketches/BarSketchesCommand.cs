using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>
    /// Makes a sheet per picked bar: a view cropped to the bar and a note with its shape,
    /// diameter, length and mark.
    /// <para>
    /// The run is one undoable step; nothing but new views, sheets and notes is added, and no
    /// bar or schedule is edited.
    /// </para>
    /// </summary>
    public sealed class BarSketchesCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SketchText(context.Culture);
            var viewModel = new SketchViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new SketchWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
