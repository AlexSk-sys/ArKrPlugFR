using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.ViewTransfer;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// The state of the transfer window: the tree of views this project offers, the
    /// open project chosen to receive them, and what the transfer did.
    /// <para>
    /// This project is the source and is never written to. The target is picked from
    /// the projects that are open beside it, so the tool opens no file of its own and
    /// has nothing to close afterwards; the copy runs as one transaction inside the
    /// target.
    /// </para>
    /// </summary>
    public sealed class TransferViewModel : ObservableObject
    {
        /// <summary>How many failures are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _source;
        private readonly Log? _log;
        private readonly string _revitVersion;
        private readonly List<TemplateNodeViewModel> _leaves = new List<TemplateNodeViewModel>();
        private readonly List<Document> _targets = new List<Document>();
        private Document? _selectedTarget;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="source">The active project the views come from.</param>
        /// <param name="openDocuments">Every project open in the running session.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public TransferViewModel(
            Document source,
            IReadOnlyList<Document> openDocuments,
            ViewTemplateTransferText text,
            Log log)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log;
            _revitVersion = source.Application.VersionNumber ?? string.Empty;

            if (openDocuments == null)
            {
                throw new ArgumentNullException(nameof(openDocuments));
            }

            foreach (Document document in openDocuments)
            {
                // A project the tool could never write into stays out of the list
                // altogether: the source itself, linked models and family documents.
                if (ReferenceEquals(document, _source)
                    || !document.IsValidObject
                    || document.IsLinked
                    || document.IsFamilyDocument)
                {
                    continue;
                }

                _targets.Add(document);
            }

            BuildTree();

            _log?.Info(
                "View transfer started (Revit " + _revitVersion + ") from '" + _source.Title +
                "', possible targets: " + ViewTemplateTransferText.Number(_targets.Count) + ".",
                nameof(TransferViewModel));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public ViewTemplateTransferText Text { get; }

        /// <summary>Gets the groups of the tree, sorted by the localized group name.</summary>
        public ObservableCollection<GroupNodeViewModel> Groups { get; } = new ObservableCollection<GroupNodeViewModel>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the projects that can receive the views.</summary>
        public IReadOnlyList<Document> Targets => _targets;

        /// <summary>Gets the caption of the target list.</summary>
        public string TargetsHeader => Text.TargetsHeader;

        /// <summary>Gets a value indicating whether another project is open at all.</summary>
        public bool HasNoTargets => _targets.Count == 0;

        /// <summary>Gets or sets the project the views are transferred into.</summary>
        public Document? SelectedTarget
        {
            get => _selectedTarget;

            set
            {
                if (SetProperty(ref _selectedTarget, value))
                {
                    RaisePropertyChanged(nameof(TargetLine));
                }
            }
        }

        /// <summary>Gets the line that names the chosen target project.</summary>
        public string TargetLine => _selectedTarget == null
            ? Text.NoTargetChosen
            : Text.TargetLine(_selectedTarget.Title) + " — " + Text.NeverOverwritten;

        /// <summary>Gets the line that names the source project.</summary>
        public string SourceLine => Text.SourceLine(_source.Title);

        /// <summary>Gets a value indicating whether the tree is empty.</summary>
        public bool HasNoGroups => Groups.Count == 0;

        /// <summary>Gets the number of ticked views.</summary>
        public int SelectedCount => _leaves.Count(leaf => leaf.IsChecked);

        /// <summary>Gets a value indicating whether anything is ticked.</summary>
        public bool HasSelection => SelectedCount > 0;

        /// <summary>Gets the caption of the transfer button, with the number of ticked views.</summary>
        public string TransferButtonText => SelectedCount > 0
            ? Text.TransferWithCount(SelectedCount)
            : Text.Transfer;

        /// <summary>Gets the result of the last transfer.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ResultCaption;

        /// <summary>Gets the hint about ticking rows and Shift multi-selection.</summary>
        public string TreeHint => Text.TreeHint;

        /// <summary>Gets the note shown when this project has nothing to transfer.</summary>
        public string EmptyTreeNote => Text.EmptyTree;

        /// <summary>
        /// Copies the ticked views into the chosen project, as one undoable step.
        /// </summary>
        /// <param name="validationError">The message that blocked the transfer, when nothing was copied.</param>
        /// <returns>The outcome, or <c>null</c> when validation blocked the transfer.</returns>
        public TransferOutcome? TryTransfer(out string? validationError)
        {
            validationError = null;

            Document? target = _selectedTarget;

            if (target == null)
            {
                validationError = Text.NoTargetChosen;
                return null;
            }

            if (!target.IsValidObject)
            {
                validationError = Text.TargetLost;
                return null;
            }

            if (target.IsReadOnly)
            {
                validationError = Text.TargetReadOnly;
                return null;
            }

            List<TemplateNodeViewModel> ticked = _leaves.Where(leaf => leaf.IsChecked).ToList();

            if (ticked.Count == 0)
            {
                validationError = Text.NoSelection;
                return null;
            }

            List<long> elementIds = ticked.Select(leaf => leaf.Id).ToList();

            _log?.Info(
                "View transfer: copying " + ViewTemplateTransferText.Number(elementIds.Count) +
                " view(s) from '" + _source.Title + "' into '" + target.Title +
                "' (Revit " + _revitVersion + ").",
                nameof(TransferViewModel));

            TransferOutcome outcome = ViewInventory.Transfer(
                _source,
                target,
                elementIds,
                Text.TransactionName);

            SetReport(BuildResultReport(outcome));

            _log?.Info(
                "View transfer finished: copied " + ViewTemplateTransferText.Number(outcome.CopiedCount) +
                ", failed " + ViewTemplateTransferText.Number(outcome.Failures.Count) + ".",
                nameof(TransferViewModel));

            return outcome;
        }

        /// <summary>Reads the views of this project and fills the tree.</summary>
        private void BuildTree()
        {
            ClearTree();

            try
            {
                IReadOnlyList<ViewCandidate> candidates = ViewInventory.Read(_source);

                // No target is chosen yet, so the tree shows the names exactly as this
                // project has them; the platform names the copies inside the target.
                TransferPlan plan = TransferPlanner.Create(candidates, Array.Empty<string>());

                foreach (GroupNodeViewModel group in SortedGroups(plan))
                {
                    Track(group);
                    Groups.Add(group);
                }

                _log?.Info(
                    "View transfer: transferable views in '" + _source.Title + "': " +
                    ViewTemplateTransferText.Number(plan.TotalCount) + ".",
                    nameof(TransferViewModel));
            }
            catch (Exception exception)
            {
                _log?.Warn(
                    "View transfer: the views of '" + _source.Title + "' could not be read: " +
                    exception.Message,
                    nameof(TransferViewModel),
                    exception);
            }

            RaisePropertyChanged(nameof(HasNoGroups));
        }

        private IEnumerable<GroupNodeViewModel> SortedGroups(TransferPlan plan)
        {
            return plan.Groups
                .Select(group => new GroupNodeViewModel(Text.GroupDisplayName(group), group.Items))
                .OrderBy(group => group.DisplayName, StringComparer.OrdinalIgnoreCase);
        }

        private void Track(GroupNodeViewModel group)
        {
            foreach (TemplateNodeViewModel leaf in group.Children)
            {
                _leaves.Add(leaf);
                leaf.PropertyChanged += OnLeafChanged;
            }
        }

        private void ClearTree()
        {
            foreach (TemplateNodeViewModel leaf in _leaves)
            {
                leaf.PropertyChanged -= OnLeafChanged;
            }

            _leaves.Clear();
            Groups.Clear();
            RaisePropertyChanged(nameof(HasNoGroups));
        }

        private void OnLeafChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateNodeViewModel.IsChecked))
            {
                RaisePropertyChanged(nameof(SelectedCount));
                RaisePropertyChanged(nameof(HasSelection));
                RaisePropertyChanged(nameof(TransferButtonText));
            }
        }

        private void SetReport(string report)
        {
            _report = report;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private string BuildResultReport(TransferOutcome outcome)
        {
            var builder = new StringBuilder();

            // A transfer is undone as a whole when any view fails, so the report
            // must not announce a count that was rolled back.
            if (outcome.IsCompletelySuccessful && outcome.CopiedCount > 0)
            {
                builder.AppendLine(Text.SuccessLine(outcome.CopiedCount));
            }
            else
            {
                builder.AppendLine(Text.RolledBackLine);
            }

            foreach (CopyFailure failure in outcome.Failures.Take(ReportLimit))
            {
                builder.AppendLine(Text.FailureLine(NameOf(failure.Id), failure.Message));
            }

            if (outcome.Failures.Count > ReportLimit)
            {
                builder.AppendLine(Text.MoreLine(outcome.Failures.Count - ReportLimit));
            }

            return builder.ToString().TrimEnd();
        }

        private string NameOf(long elementId)
        {
            foreach (TemplateNodeViewModel leaf in _leaves)
            {
                if (leaf.Id == elementId)
                {
                    return leaf.Name;
                }
            }

            return ViewNaming.FallbackName;
        }
    }
}
