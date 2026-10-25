using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SectionCollector
{
    /// <summary>
    /// Collects the concrete structures of the active 3D view into a report grouped by
    /// level, category and section, and lets the user select the elements in the model
    /// or copy the report to the clipboard.
    /// <para>
    /// The tool only reads: nothing in the model changes.
    /// </para>
    /// </summary>
    public sealed class SectionCollectorCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new SectionText(context.Culture);

            SectionOutcome outcome = SectionEngine.Collect(context.Document, text, context.Log);
            var window = new SectionWindow(context.UiDocument, outcome, text, context.Log);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
