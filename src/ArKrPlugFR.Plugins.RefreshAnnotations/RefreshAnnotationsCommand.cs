using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.RefreshAnnotations
{
    /// <summary>
    /// Makes the tags of the active view recompute their text.
    /// <para>
    /// With nothing selected the whole view is refreshed; with a selection only the tags
    /// of the selected elements are. Nothing about the project changes: every tag ends
    /// where it started, with its text computed against the current model.
    /// </para>
    /// </summary>
    public sealed class RefreshAnnotationsCommand : PluginCommand
    {
        /// <summary>How many refusal lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new RefreshText(context.Culture);

            View? view = context.Document.ActiveView;

            RefreshOutcome outcome = AnnotationRefresher.Refresh(
                context.UiDocument,
                view,
                context.UiDocument.Selection.GetElementIds(),
                text,
                context.Log);

            ToolWindowPresenter.Show(
                text.WindowTitle,
                text.Subtitle,
                text.ReportCaption,
                BuildReport(outcome, text));

            return Result.Succeeded;
        }

        private static string BuildReport(RefreshOutcome outcome, RefreshText text)
        {
            if (outcome.Stopped != null)
            {
                return outcome.Stopped;
            }

            var builder = new StringBuilder();

            builder.AppendLine(text.Result(
                outcome.Refreshed,
                outcome.Tags,
                outcome.View,
                outcome.Orphaned,
                outcome.Refused));

            int shown = 0;

            foreach (string failure in outcome.Failures)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(failure);
                shown++;
            }

            if (outcome.Failures.Count > shown)
            {
                builder.AppendLine(text.MoreLine(outcome.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
