using System;
using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Joins;
using ArKrPlugFR.Core.Localization;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>
    /// Shared behaviour of the two buttons: unjoin columns and unjoin beams.
    /// <para>
    /// The buttons differ only in the categories they search and the words they
    /// show, so everything else lives here once.
    /// </para>
    /// </summary>
    public abstract class UnjoinCommandBase : PluginCommand
    {
        /// <summary>Gets the caption of the button, also used as the window title.</summary>
        protected abstract LocalizedText CommandTitle { get; }

        /// <summary>Gets the plural noun of the neighbours, used inside sentences.</summary>
        protected abstract LocalizedText NeighbourNoun { get; }

        /// <summary>Gets the categories whose cuts are removed.</summary>
        protected abstract IReadOnlyCollection<BuiltInCategory> NeighbourCategories { get; }

        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var engine = new UnjoinEngine(context, new UnjoinSearchOptions());

            UnjoinOutcome outcome = engine.Run(
                NeighbourCategories,
                CommandTitle.Resolve(context.Culture));

            Show(context, outcome);

            if (outcome.Status == UnjoinStatus.NoSelection || outcome.Status == UnjoinStatus.NoWalls)
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        private void Show(PluginContext context, UnjoinOutcome outcome)
        {
            string culture = context.Culture;
            string title = CommandTitle.Resolve(culture);

            switch (outcome.Status)
            {
                case UnjoinStatus.NoSelection:
                    ToolWindowPresenter.Show(
                        title,
                        string.Empty,
                        UnjoinStrings.CutsCaption.Resolve(culture),
                        UnjoinStrings.NoSelectionTitle.Resolve(culture) + Environment.NewLine + Environment.NewLine +
                        UnjoinStrings.NoSelectionHint.Resolve(culture));
                    return;

                case UnjoinStatus.NoWalls:
                    ToolWindowPresenter.Show(
                        title,
                        string.Empty,
                        UnjoinStrings.CutsCaption.Resolve(culture),
                        UnjoinStrings.NoWallsTitle.Resolve(culture) + Environment.NewLine + Environment.NewLine +
                        UnjoinStrings.NoWallsHint.Resolve(culture));
                    return;

                case UnjoinStatus.NothingToDo:
                    {
                        string message = outcome.Pairs.Count == 0
                            ? UnjoinStrings.NothingToDoMessage.Resolve(culture, NeighbourNoun.Resolve(culture))
                            : UnjoinStrings.NothingToDoTitle.Resolve(culture);

                        ToolWindowPresenter.Show(
                            title,
                            DocumentLine(context, outcome, UnjoinStrings.NothingToDoTitle),
                            UnjoinStrings.CutsCaption.Resolve(culture),
                            message + Environment.NewLine + Environment.NewLine +
                            BuildSummary(context, outcome));
                        return;
                    }

                default:
                    ToolWindowPresenter.Show(
                        title,
                        DocumentLine(context, outcome, UnjoinStrings.CompletedTitle),
                        UnjoinStrings.CutsCaption.Resolve(culture),
                        BuildSummary(context, outcome) + Environment.NewLine + Environment.NewLine +
                        BuildPairList(context, outcome));
                    return;
            }
        }

        private static string DocumentLine(PluginContext context, UnjoinOutcome outcome, LocalizedText statusTitle)
        {
            return context.Document.Title + "   " +
                statusTitle.Resolve(context.Culture) + "   " +
                UnjoinStrings.RemovedLine.Resolve(context.Culture, outcome.RemovedCount);
        }

        private static string BuildSummary(PluginContext context, UnjoinOutcome outcome)
        {
            string culture = context.Culture;
            var summary = new StringBuilder();

            summary.AppendLine(UnjoinStrings.WallsLine.Resolve(culture, outcome.WallCount));
            summary.AppendLine(UnjoinStrings.FoundLine.Resolve(culture, outcome.Pairs.Count));

            if (outcome.Status == UnjoinStatus.Completed)
            {
                summary.AppendLine(UnjoinStrings.RemovedLine.Resolve(culture, outcome.RemovedCount));
            }

            if (outcome.Failed.Count > 0)
            {
                summary.AppendLine(UnjoinStrings.FailedLine.Resolve(culture, outcome.Failed.Count));
            }

            return summary.ToString().TrimEnd();
        }

        private static string BuildPairList(PluginContext context, UnjoinOutcome outcome)
        {
            var report = new StringBuilder();
            Document document = context.Document;

            foreach (JoinPair pair in outcome.Pairs)
            {
                report.Append("  ");
                report.Append(ElementDescription.Describe(document, pair.First));
                report.Append("  <->  ");
                report.AppendLine(ElementDescription.Describe(document, pair.Second));
            }

            foreach (JoinPair pair in outcome.Failed)
            {
                report.Append("  ! ");
                report.Append(ElementDescription.Describe(document, pair.First));
                report.Append("  <->  ");
                report.AppendLine(ElementDescription.Describe(document, pair.Second));
            }

            return report.ToString().TrimEnd();
        }
    }
}
