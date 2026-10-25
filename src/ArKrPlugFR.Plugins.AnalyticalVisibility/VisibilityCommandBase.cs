using System.Text;
using ArKrPlugFR.Core.Visibility;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.AnalyticalVisibility
{
    /// <summary>
    /// The shared body of the two commands: one hides the analytical model, the other shows
    /// it again.
    /// <para>
    /// Both do the same work with the opposite action, so the body lives here once; the
    /// derived commands only say which action they are. Nothing is deleted: the run changes
    /// the per-view hiding of the analytical elements and reports what it did.
    /// </para>
    /// </summary>
    public abstract class VisibilityCommandBase : PluginCommand
    {
        /// <summary>How many refusal lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        /// <summary>Gets the action this command performs.</summary>
        protected abstract VisibilityAction Action { get; }

        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new VisibilityText(context.Culture);

            VisibilityOutcome outcome = VisibilityEngine.Run(context.Document, Action, text, context.Log);

            ToolWindowPresenter.Show(
                text.Title(Action),
                text.Subtitle,
                text.ReportCaption,
                BuildReport(outcome, text));

            return Result.Succeeded;
        }

        private static string BuildReport(VisibilityOutcome outcome, VisibilityText text)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(text.Result(
                    outcome.Elements,
                    outcome.Views,
                    outcome.Changed,
                    outcome.InPlace,
                    outcome.Cannot,
                    outcome.Refused));

                if (outcome.Changed == 0 && outcome.Refused == 0)
                {
                    builder.AppendLine(text.NothingToDo);
                }

                builder.AppendLine(text.Safe);
            }

            int shown = 0;

            foreach (string failure in outcome.Failures)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append(" - ");
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

    /// <summary>Hides the analytical model in the views of the model.</summary>
    public sealed class HideAnalyticalModelCommand : VisibilityCommandBase
    {
        /// <inheritdoc />
        protected override VisibilityAction Action => VisibilityAction.Hide;
    }

    /// <summary>Shows the analytical model again in the views of the model.</summary>
    public sealed class ShowAnalyticalModelCommand : VisibilityCommandBase
    {
        /// <inheritdoc />
        protected override VisibilityAction Action => VisibilityAction.Show;
    }
}
