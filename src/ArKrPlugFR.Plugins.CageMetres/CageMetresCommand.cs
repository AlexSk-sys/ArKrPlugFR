using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// Measures a rebar cage along its picked layout lines and writes the running metres
    /// into the cage.
    /// <para>
    /// The window holds the picks and the settings; the run is one undoable step and only
    /// touches the calculated-length parameter of the cage.
    /// </para>
    /// </summary>
    public sealed class CageMetresCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new CageMetresText(context.Culture);
            var viewModel = new CageMetresViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new CageMetresWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
