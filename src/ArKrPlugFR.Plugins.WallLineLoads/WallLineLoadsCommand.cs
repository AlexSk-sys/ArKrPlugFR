using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// Creates the line loads of the chosen walls at the bottom of every wall, for the
    /// calculation model.
    /// <para>
    /// The whole run is one undoable step: it creates loads and, when the name is new, one
    /// load case. No wall is moved or edited.
    /// </para>
    /// </summary>
    public sealed class WallLineLoadsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new LoadText(context.Culture);
            var viewModel = new LoadViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new LoadWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
