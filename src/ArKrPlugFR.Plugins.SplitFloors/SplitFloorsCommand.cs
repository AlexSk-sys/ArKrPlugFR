using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SplitFloors
{
    /// <summary>
    /// Splits the picked floors: every outer contour of a flat slab becomes its own floor
    /// with the openings it holds.
    /// <para>
    /// The user picks the floors in the window, checks the list and presses one button;
    /// the split of each floor is one undoable step of its own, and a floor that cannot be
    /// split leaves the model unchanged.
    /// </para>
    /// </summary>
    public sealed class SplitFloorsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SplitText(context.Culture);
            var viewModel = new SplitViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new SplitWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
