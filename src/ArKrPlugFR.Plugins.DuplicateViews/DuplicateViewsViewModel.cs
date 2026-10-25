using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Views;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// The state of the window: the trees of views, schedules and view templates of the
    /// project, the template chosen for the copies, and the run that creates them.
    /// <para>
    /// The whole run is one transaction, so a failure at that level leaves the project
    /// untouched; a single copy that fails is skipped and the run carries on.
    /// </para>
    /// </summary>
    public sealed class DuplicateViewsViewModel : ObservableObject
    {
        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _document;
        private readonly Log _log;
        private readonly List<CopyGroupViewModel> _viewGroups = new List<CopyGroupViewModel>();
        private readonly List<CopyGroupViewModel> _scheduleGroups = new List<CopyGroupViewModel>();
        private readonly List<CopyGroupViewModel> _templateGroups = new List<CopyGroupViewModel>();
        private readonly List<CopyLeafViewModel> _leaves = new List<CopyLeafViewModel>();
        private readonly List<TemplateChoice> _allTemplates = new List<TemplateChoice>();
        private string _searchText = string.Empty;
        private string _templateSearchText = string.Empty;
        private TemplateChoice? _selectedTemplate;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">The project whose elements are copied.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public DuplicateViewsViewModel(Document document, DuplicateViewsText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            Build();
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public DuplicateViewsText Text { get; }

        /// <summary>Gets the tree of ordinary and drafting views.</summary>
        public ObservableCollection<CopyGroupViewModel> Views { get; } = new ObservableCollection<CopyGroupViewModel>();

        /// <summary>Gets the tree of schedules.</summary>
        public ObservableCollection<CopyGroupViewModel> Schedules { get; } = new ObservableCollection<CopyGroupViewModel>();

        /// <summary>Gets the tree of view templates.</summary>
        public ObservableCollection<CopyGroupViewModel> TemplateGroups { get; } = new ObservableCollection<CopyGroupViewModel>();

        /// <summary>Gets the entries the template picker offers for the current tick and search.</summary>
        public ObservableCollection<TemplateChoice> TemplateChoices { get; } = new ObservableCollection<TemplateChoice>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the views tree, with the number of views.</summary>
        public string ViewsHeader => Text.ViewsHeader + " (" + Number(CountOf(CopyTree.Views)) + ")";

        /// <summary>Gets the caption of the schedules tree, with the number of schedules.</summary>
        public string SchedulesHeader => Text.SchedulesHeader + " (" + Number(CountOf(CopyTree.Schedules)) + ")";

        /// <summary>Gets the caption of the templates tree, with the number of templates.</summary>
        public string TemplatesHeader => Text.TemplatesHeader + " (" + Number(CountOf(CopyTree.Templates)) + ")";

        /// <summary>Gets the hint of the tree search field.</summary>
        public string SearchHint => Text.SearchHint;

        /// <summary>Gets the hint about ticking rows and Shift multi-selection.</summary>
        public string TreeHint => Text.TreeHint;

        /// <summary>Gets the caption of the template section.</summary>
        public string TemplateSection => Text.TemplateSection;

        /// <summary>Gets the hint of the template search field.</summary>
        public string TemplateSearchHint => Text.TemplateSearchHint;

        /// <summary>Gets the note about what the chosen template is applied to.</summary>
        public string TemplateHint => Text.TemplateHint;

        /// <summary>Gets a value indicating whether the views tree has nothing to show.</summary>
        public bool HasNoVisibleViews => !_viewGroups.Any(group => group.IsVisible);

        /// <summary>Gets a value indicating whether the schedules tree has nothing to show.</summary>
        public bool HasNoVisibleSchedules => !_scheduleGroups.Any(group => group.IsVisible);

        /// <summary>Gets a value indicating whether the templates tree has nothing to show.</summary>
        public bool HasNoVisibleTemplates => !_templateGroups.Any(group => group.IsVisible);

        /// <summary>Gets or sets the text the trees are filtered by.</summary>
        public string SearchText
        {
            get => _searchText;

            set
            {
                if (SetProperty(ref _searchText, value ?? string.Empty))
                {
                    ApplyFilter();
                }
            }
        }

        /// <summary>Gets or sets the text the template picker is filtered by.</summary>
        public string TemplateSearchText
        {
            get => _templateSearchText;

            set
            {
                if (SetProperty(ref _templateSearchText, value ?? string.Empty))
                {
                    RefreshTemplates();
                }
            }
        }

        /// <summary>Gets or sets the template applied to every created view.</summary>
        public TemplateChoice? SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }

        /// <summary>Gets the number of ticked rows.</summary>
        public int SelectedCount => _leaves.Count(leaf => leaf.IsChecked);

        /// <summary>Gets a value indicating whether anything is ticked.</summary>
        public bool HasSelection => SelectedCount > 0;

        /// <summary>Gets how many copies the ticked rows ask for together.</summary>
        public int PlannedCopies => _leaves.Where(leaf => leaf.IsChecked).Sum(leaf => leaf.Count);

        /// <summary>Gets the caption of the duplicate button, with the number of copies.</summary>
        public string DuplicateButtonText => PlannedCopies > 0
            ? Text.DuplicateWithCount(PlannedCopies)
            : Text.Duplicate;

        /// <summary>Gets the result of the last run.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Unticks every row, which is what Escape does in the window.</summary>
        public void ClearAll()
        {
            foreach (CopyGroupViewModel group in _viewGroups)
            {
                group.SetChecked(false);
            }

            foreach (CopyGroupViewModel group in _scheduleGroups)
            {
                group.SetChecked(false);
            }

            foreach (CopyGroupViewModel group in _templateGroups)
            {
                group.SetChecked(false);
            }
        }

        /// <summary>
        /// Tells whether the chosen template fits every ticked element that takes one.
        /// Schedules and the copies of templates themselves never take a template.
        /// </summary>
        /// <returns><c>true</c> when the template may be applied, or none was chosen.</returns>
        public bool ChosenTemplateFits()
        {
            if (_selectedTemplate == null || _selectedTemplate.IsNone)
            {
                return true;
            }

            foreach (CopyLeafViewModel leaf in _leaves)
            {
                if (!leaf.IsChecked || leaf.Tree == CopyTree.Templates)
                {
                    continue;
                }

                if (!TemplateRules.Fits(_selectedTemplate.Kind, leaf.Kind))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the requested copies of every ticked element in one transaction.
        /// </summary>
        /// <param name="applyTemplate">Whether the chosen template is applied to the copies.</param>
        /// <returns>What the run did.</returns>
        public DuplicateRunResult Run(bool applyTemplate)
        {
            List<CopyLeafViewModel> ticked = _leaves.Where(leaf => leaf.IsChecked).ToList();

            var failures = new List<string>();
            int created = 0;

            _log.Info(
                "DuplicateViews: copying " + Number(PlannedCopies) + " copy(ies) for " +
                Number(ticked.Count) + " row(s); template: " +
                (_selectedTemplate == null ? "none" : _selectedTemplate.Name) + ".",
                nameof(DuplicateViewsViewModel));

            try
            {
                DocumentEdit.Run(_document, Text.TransactionName, () =>
                {
                    var taken = new HashSet<string>(
                        ViewInventory.ReadViewNames(_document),
                        StringComparer.OrdinalIgnoreCase);

                    foreach (CopyLeafViewModel leaf in ticked)
                    {
                        for (int copyNumber = 1; copyNumber <= leaf.Count; copyNumber++)
                        {
                            try
                            {
                                View? copy = CreateCopy(leaf, copyNumber, taken);

                                if (copy == null)
                                {
                                    failures.Add(Text.FailedLine(leaf.Name, Text.CopyRefused));
                                    continue;
                                }

                                ApplyTemplate(copy, leaf, applyTemplate);
                                CarryPhaseOver(leaf.Id, copy);

                                created++;
                            }
                            catch (Exception exception)
                            {
                                failures.Add(Text.FailedLine(leaf.Name, exception.Message));

                                _log.Warn(
                                    "DuplicateViews: copy " + Number(copyNumber) + " of '" +
                                    leaf.Name + "' failed: " + exception.Message,
                                    nameof(DuplicateViewsViewModel),
                                    exception);
                            }
                        }
                    }

                    return created > 0;
                });
            }
            catch (Exception exception)
            {
                _log.Error(
                    "DuplicateViews: the run failed: " + exception.Message,
                    nameof(DuplicateViewsViewModel),
                    exception);

                failures.Add(Text.FailedLine(Text.TransactionName, exception.Message));
            }

            _log.Info(
                "DuplicateViews: created " + Number(created) + " copy(ies), failed " +
                Number(failures.Count) + ".",
                nameof(DuplicateViewsViewModel));

            return new DuplicateRunResult(created, failures);
        }

        /// <summary>Builds the text of the result: the summary line and the failures.</summary>
        /// <param name="result">The run to describe.</param>
        /// <returns>The report text.</returns>
        public string BuildReport(DuplicateRunResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var builder = new StringBuilder();

            builder.AppendLine(Text.Success(result.Created));

            int shown = Math.Min(result.Failures.Count, ReportLimit);

            for (int index = 0; index < shown; index++)
            {
                builder.Append("  ");
                builder.AppendLine(result.Failures[index]);
            }

            if (result.Failures.Count > shown)
            {
                builder.AppendLine(Text.MoreLine(result.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>Shows the given text in the report area of the window.</summary>
        /// <param name="report">Text to show.</param>
        public void SetReport(string report)
        {
            _report = report ?? string.Empty;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        /// <summary>Reads the project and fills the three trees and the template picker.</summary>
        private void Build()
        {
            IReadOnlyList<CopyCandidate> candidates = ViewInventory.Read(_document, Text);

            BuildTree(candidates.Where(candidate => candidate.Tree == CopyTree.Views), Views, _viewGroups);
            BuildTree(candidates.Where(candidate => candidate.Tree == CopyTree.Schedules), Schedules, _scheduleGroups);
            BuildTree(candidates.Where(candidate => candidate.Tree == CopyTree.Templates), TemplateGroups, _templateGroups);

            _allTemplates.Add(TemplateChoice.None(Text.NoTemplateOption));

            foreach (CopyCandidate candidate in candidates.Where(candidate => candidate.Tree == CopyTree.Templates))
            {
                _allTemplates.Add(TemplateChoice.Of(candidate.Id, candidate.Name, candidate.Kind, candidate.Group));
            }

            _selectedTemplate = _allTemplates[0];

            ApplyFilter();
            RefreshTemplates();

            _log.Info(
                "DuplicateViews: listed " + Number(_leaves.Count) + " element(s) in '" +
                _document.Title + "'.",
                nameof(DuplicateViewsViewModel));
        }

        private void BuildTree(
            IEnumerable<CopyCandidate> candidates,
            ObservableCollection<CopyGroupViewModel> target,
            List<CopyGroupViewModel> registry)
        {
            foreach (IGrouping<string, CopyCandidate> group in candidates
                .GroupBy(candidate => candidate.Group)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                var node = new CopyGroupViewModel(
                    group.Key,
                    group.OrderBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase).ToList());

                foreach (CopyLeafViewModel leaf in node.Children)
                {
                    _leaves.Add(leaf);
                    leaf.PropertyChanged += OnLeafChanged;
                }

                registry.Add(node);
                target.Add(node);
            }
        }

        private void ApplyFilter()
        {
            ApplyFilterTo(_viewGroups);
            ApplyFilterTo(_scheduleGroups);
            ApplyFilterTo(_templateGroups);
        }

        private void ApplyFilterTo(List<CopyGroupViewModel> groups)
        {
            foreach (CopyGroupViewModel group in groups)
            {
                group.ApplyFilter(
                    Matches(_searchText, group.DisplayName),
                    name => Matches(_searchText, name));
            }

            RaisePropertyChanged(nameof(HasNoVisibleViews));
            RaisePropertyChanged(nameof(HasNoVisibleSchedules));
            RaisePropertyChanged(nameof(HasNoVisibleTemplates));
        }

        private void RefreshTemplates()
        {
            // The picker offers what could actually be applied: schedule templates only
            // while schedules are ticked, view templates only while views are.
            bool anyViews = _leaves.Any(leaf => leaf.IsChecked && leaf.Tree == CopyTree.Views);
            bool anySchedules = _leaves.Any(leaf => leaf.IsChecked && leaf.Tree == CopyTree.Schedules);

            var wanted = new List<TemplateChoice>();

            foreach (TemplateChoice choice in _allTemplates)
            {
                if (choice.IsNone)
                {
                    wanted.Add(choice);
                    continue;
                }

                bool fitsTickedKind = (anyViews && choice.Kind != TemplateKind.Schedule)
                    || (anySchedules && choice.Kind == TemplateKind.Schedule);

                if (!fitsTickedKind)
                {
                    continue;
                }

                if (TemplateRules.Matches(_templateSearchText, new[] { choice.Name, choice.Description }))
                {
                    wanted.Add(choice);
                }
            }

            TemplateChoices.Clear();

            foreach (TemplateChoice choice in wanted
                .OrderBy(choice => choice.Description, StringComparer.OrdinalIgnoreCase)
                .ThenBy(choice => choice.Name, StringComparer.OrdinalIgnoreCase))
            {
                TemplateChoices.Add(choice);
            }

            if (_selectedTemplate != null && !TemplateChoices.Contains(_selectedTemplate))
            {
                SelectedTemplate = TemplateChoices.Count > 0 ? TemplateChoices[0] : null;
            }
            else if (_selectedTemplate == null && TemplateChoices.Count > 0)
            {
                SelectedTemplate = TemplateChoices[0];
            }
        }

        private View? CreateCopy(CopyLeafViewModel leaf, int copyNumber, HashSet<string> taken)
        {
            ElementId sourceId = ElementIdCompat.Create(leaf.Id);

            if (_document.GetElement(sourceId) is not View source || !source.IsValidObject)
            {
                throw new InvalidOperationException(Text.ElementMissing);
            }

            string name = ViewCopyNames.NextCopyName(source.Name, copyNumber, candidate => taken.Contains(candidate));

            ViewDuplicateOption option = NeedsDetailing(source)
                ? ViewDuplicateOption.WithDetailing
                : ViewDuplicateOption.Duplicate;

            if (!source.CanViewBeDuplicated(option))
            {
                return null;
            }

            var copy = _document.GetElement(source.Duplicate(option)) as View;

            if (copy == null || !copy.IsValidObject)
            {
                return null;
            }

            if (name.Length > 0)
            {
                copy.Name = name;
                taken.Add(name);
            }

            return copy;
        }

        /// <summary>
        /// Tells whether a copy keeps the detailing of its source: ordinary and drafting
        /// views do, while legends, schedules and templates are copied whole.
        /// </summary>
        private static bool NeedsDetailing(View source)
        {
            if (source.IsTemplate || source is ViewSchedule)
            {
                return false;
            }

            return source.ViewType != ViewType.Legend;
        }

        private void ApplyTemplate(View copy, CopyLeafViewModel leaf, bool applyTemplate)
        {
            if (!applyTemplate
                || _selectedTemplate == null
                || _selectedTemplate.IsNone
                || leaf.Tree == CopyTree.Templates
                || copy is ViewSchedule
                || !TemplateRules.Fits(_selectedTemplate.Kind, leaf.Kind))
            {
                return;
            }

            copy.ViewTemplateId = ElementIdCompat.Create(_selectedTemplate.Id);
        }

        private void CarryPhaseOver(long sourceId, View copy)
        {
            try
            {
                if (_document.GetElement(ElementIdCompat.Create(sourceId)) is not View source)
                {
                    return;
                }

                Parameter? sourcePhase = source.get_Parameter(BuiltInParameter.VIEW_PHASE);
                Parameter? copyPhase = copy.get_Parameter(BuiltInParameter.VIEW_PHASE);

                if (sourcePhase == null || copyPhase == null || copyPhase.IsReadOnly)
                {
                    return;
                }

                ElementId phaseId = sourcePhase.AsElementId();

                if (phaseId != null && phaseId != ElementId.InvalidElementId)
                {
                    copyPhase.Set(phaseId);
                }
            }
            catch (Exception exception)
            {
                // A copy without the source's stage is still a usable copy.
                _log.Warn(
                    "DuplicateViews: the stage of '" + copy.Name + "' was not carried over: " + exception.Message,
                    nameof(DuplicateViewsViewModel));
            }
        }

        /// <summary>
        /// Gets the rows of one tree in display order, as far as the search shows them.
        /// </summary>
        /// <param name="tree">Which tree to walk.</param>
        /// <returns>The visible rows, group by group.</returns>
        internal IReadOnlyList<CopyLeafViewModel> VisibleLeaves(CopyTree tree)
        {
            var ordered = new List<CopyLeafViewModel>();

            IEnumerable<CopyGroupViewModel> groups;

            switch (tree)
            {
                case CopyTree.Views:
                    groups = _viewGroups;
                    break;

                case CopyTree.Schedules:
                    groups = _scheduleGroups;
                    break;

                default:
                    groups = _templateGroups;
                    break;
            }

            foreach (CopyGroupViewModel group in groups)
            {
                foreach (CopyLeafViewModel leaf in group.Children)
                {
                    if (leaf.IsVisible)
                    {
                        ordered.Add(leaf);
                    }
                }
            }

            return ordered;
        }

        private void OnLeafChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CopyLeafViewModel.IsChecked))
            {
                RaisePropertyChanged(nameof(SelectedCount));
                RaisePropertyChanged(nameof(HasSelection));
                RaisePropertyChanged(nameof(PlannedCopies));
                RaisePropertyChanged(nameof(DuplicateButtonText));

                RefreshTemplates();
            }
            else if (e.PropertyName == nameof(CopyLeafViewModel.Count))
            {
                RaisePropertyChanged(nameof(PlannedCopies));
                RaisePropertyChanged(nameof(DuplicateButtonText));
            }
        }

        private bool Matches(string? search, string? text)
        {
            if (search == null || search.Length == 0)
            {
                return true;
            }

            return text != null && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Number(int value)
        {
            return DuplicateViewsText.Number(value);
        }

        private int CountOf(CopyTree tree)
        {
            int count = 0;

            foreach (CopyLeafViewModel leaf in _leaves)
            {
                if (leaf.Tree == tree)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
