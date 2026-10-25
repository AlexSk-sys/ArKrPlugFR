using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// Writes the concrete structures of a project or of the current selection into a
    /// comma-separated file.
    /// <para>
    /// The window chooses the scope and the file; the run reads the elements, keeps the
    /// ones whose names say concrete, writes one row per element and reports what it did.
    /// The tool only reads the project and never opens a transaction.
    /// </para>
    /// </summary>
    public sealed class ConcreteExportCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new ConcreteExportText(context.Culture);
            var viewModel = new ConcreteExportViewModel(
                context.UiDocument,
                context.Document,
                text,
                context.Log);

            var window = new ConcreteExportWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
