using System.Collections.Generic;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.HoleCuttingPlugin
{
    /// <summary>
    /// Cuts a rectangular opening in a wall where a service crosses it.
    /// <para>
    /// The tool works on the current selection: one wall and one service element — a
    /// duct, a pipe, a cable tray or a conduit. It shows no window of its own and reports
    /// through the shared result window; the wall itself is the only thing changed.
    /// </para>
    /// </summary>
    public sealed class HoleCuttingCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new HoleCuttingText(context.Culture);
            UIDocument? uiDocument = context.UiApplication.ActiveUIDocument;

            if (uiDocument == null)
            {
                ToolWindowPresenter.Show(
                    text.WindowTitle,
                    text.Subtitle,
                    text.ReportCaption,
                    text.NothingSelected);

                return Result.Cancelled;
            }

            Wall? wall = null;
            Element? service = null;
            int wallCount = 0;
            int serviceCount = 0;
            int otherCount = 0;

            ICollection<ElementId> selected = uiDocument.Selection.GetElementIds();

            foreach (ElementId id in selected)
            {
                if (context.Document.GetElement(id) is not Element element)
                {
                    continue;
                }

                if (element is Wall candidate)
                {
                    wallCount++;
                    wall ??= candidate;
                    continue;
                }

                if (CrossingCutter.IsSupported(element))
                {
                    serviceCount++;
                    service ??= element;
                    continue;
                }

                otherCount++;
            }

            if (wallCount != 1 || serviceCount != 1 || otherCount != 0)
            {
                string message = wallCount == 0 && serviceCount == 0 && otherCount == 0
                    ? text.NothingSelected
                    : text.WrongSelection(wallCount, serviceCount, otherCount);

                Log.Info(
                    "HoleCutting: selection refused (walls " + wallCount + ", services " +
                    serviceCount + ", others " + otherCount + ").",
                    nameof(HoleCuttingCommand));

                ToolWindowPresenter.Show(text.WindowTitle, text.Subtitle, text.ReportCaption, message);

                return Result.Cancelled;
            }

            CrossingOutcome outcome = CrossingCutter.Cut(
                context.Document,
                wall!,
                service!,
                text,
                context.Log);

            ToolWindowPresenter.Show(
                text.WindowTitle,
                text.SelectionLine,
                text.ReportCaption,
                outcome.Message);

            return Result.Succeeded;
        }
    }
}
