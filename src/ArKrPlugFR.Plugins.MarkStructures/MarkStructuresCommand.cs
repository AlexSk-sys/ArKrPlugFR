using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// Decides the kind of every structure of the project and writes the mark it asks for.
    /// <para>
    /// The window holds the codes and the suffixes; the run walks the supported categories
    /// once, decides the kind of every element from its category and shape, the material
    /// family from its materials, and writes the mark in one undoable step.
    /// </para>
    /// </summary>
    public sealed class MarkStructuresCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new MarkText(context.Culture);
            var viewModel = new MarkViewModel(context.Document, text, context.Log);

            var window = new MarkWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
