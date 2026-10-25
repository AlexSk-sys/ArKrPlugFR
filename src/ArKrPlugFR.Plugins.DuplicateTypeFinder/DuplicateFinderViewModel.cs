using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Duplication;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// The state of the duplicate window: the findings, which of them are ticked,
    /// and what the delete did.
    /// <para>
    /// All of the behaviour is here rather than in the window, so the window stays
    /// a layout and the safety rule of the tool can be reasoned about on its own.
    /// </para>
    /// </summary>
    public sealed class DuplicateFinderViewModel : ObservableObject
    {
        /// <summary>How many failures are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly Log _log;
        private readonly List<CheckableRow> _rows = new List<CheckableRow>();
        private readonly int _checkedTypeCount;
        private string _report;

        /// <summary>Creates the view model from a finished scan.</summary>
        /// <param name="document">The project the findings come from.</param>
        /// <param name="uiDocument">The user interface document, used to show an element.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <param name="result">The findings to show.</param>
        /// <param name="readErrors">Items the inventory could not read.</param>
        public DuplicateFinderViewModel(
            Document document,
            UIDocument uiDocument,
            DuplicateFinderText text,
            Log log,
            DuplicateScanResult result,
            IReadOnlyList<string> readErrors)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log;

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            TypeRows = new ObservableCollection<TypeRowViewModel>(
                result.TypeDuplicates.Select(row => new TypeRowViewModel(row, Text)));

            FamilyRows = new ObservableCollection<FamilyRowViewModel>(
                result.FamilyDuplicates.Select(row => new FamilyRowViewModel(row, Text)));

            NearRows = new ObservableCollection<NearRowViewModel>(
                result.NearDuplicates.Select(row => new NearRowViewModel(row)));

            foreach (TypeRowViewModel row in TypeRows)
            {
                Track(row);
            }

            foreach (FamilyRowViewModel row in FamilyRows)
            {
                Track(row);
            }

            _checkedTypeCount = result.CheckedTypeCount;
            _report = BuildInitialReport(readErrors ?? Array.Empty<string>());
        }

        /// <summary>Raised after a row was shown in the model, so the window can close itself.</summary>
        public event Action? ZoomRequested;

        /// <summary>Gets the text resolved for the current language.</summary>
        public DuplicateFinderText Text { get; }

        /// <summary>Gets the rows of the types tab.</summary>
        public ObservableCollection<TypeRowViewModel> TypeRows { get; }

        /// <summary>Gets the rows of the families tab.</summary>
        public ObservableCollection<FamilyRowViewModel> FamilyRows { get; }

        /// <summary>Gets the rows of the close-matches tab.</summary>
        public ObservableCollection<NearRowViewModel> NearRows { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title: the document and the scan summary.</summary>
        public string Subtitle => _document.Title + "   " + SummaryText;

        /// <summary>Gets the summary of what the scan found, kept up to date as rows are removed.</summary>
        public string SummaryText => BuildSummary();

        /// <summary>Gets the caption of the types tab, with its row count.</summary>
        public string TypesTabHeader => Text.TypesTab + " (" + DuplicateFinderText.Number(TypeRows.Count) + ")";

        /// <summary>Gets the caption of the families tab, with its row count.</summary>
        public string FamiliesTabHeader => Text.FamiliesTab + " (" + DuplicateFinderText.Number(FamilyRows.Count) + ")";

        /// <summary>Gets the caption of the close-matches tab, with its row count.</summary>
        public string NearTabHeader => Text.NearTab + " (" + DuplicateFinderText.Number(NearRows.Count) + ")";

        /// <summary>Gets the heading shown above the close matches.</summary>
        public string NearIntro => Text.NearIntro;

        /// <summary>Gets a value indicating whether the close-matches tab has anything to show.</summary>
        public bool HasNearRows => NearRows.Count > 0;

        /// <summary>Gets the number of ticked rows across both tabs.</summary>
        public int SelectedCount => _rows.Count(row => row.IsSelected);

        /// <summary>Gets a value indicating whether anything is ticked.</summary>
        public bool HasSelection => SelectedCount > 0;

        /// <summary>Gets the caption of the delete button, with the number of ticked rows.</summary>
        public string DeleteButtonText =>
            Text.Delete + " (" + DuplicateFinderText.Number(SelectedCount) + ")";

        /// <summary>Gets the result of the last delete, or the read errors of the scan.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ResultCaption;

        /// <summary>Ticks every row that may be deleted.</summary>
        public void SelectAll()
        {
            foreach (CheckableRow row in _rows)
            {
                row.IsSelected = true;
            }
        }

        /// <summary>Unticks every row.</summary>
        public void ClearSelection()
        {
            foreach (CheckableRow row in _rows)
            {
                row.IsSelected = false;
            }
        }

        /// <summary>Builds the text that is copied to the clipboard.</summary>
        /// <remarks>
        /// The rows are separated with tabs so the list can be pasted straight into a
        /// spreadsheet, which is what people do with a list of things to delete.
        /// </remarks>
        public string BuildCopyText()
        {
            const char tab = '\t';
            var builder = new StringBuilder();

            builder.AppendLine(Text.CopyHeader + tab + _document.Title);
            builder.AppendLine();
            builder.AppendLine(Text.CopyTypesHeader);
            builder.AppendLine(string.Join(
                tab.ToString(),
                Text.ColumnFamily,
                Text.ColumnCategory,
                Text.ColumnType,
                Text.ColumnKeptType,
                Text.ColumnInstances,
                Text.ColumnStatus));

            foreach (TypeRowViewModel row in TypeRows)
            {
                builder.AppendLine(row.CopyLine);
            }

            builder.AppendLine();
            builder.AppendLine(Text.CopyFamiliesHeader);
            builder.AppendLine(string.Join(
                tab.ToString(),
                Text.ColumnFamily,
                Text.ColumnCategory,
                Text.ColumnKeptFamily,
                Text.ColumnMatchedTypes,
                Text.ColumnInstances,
                Text.ColumnStatus));

            foreach (FamilyRowViewModel row in FamilyRows)
            {
                builder.AppendLine(row.CopyLine);
            }

            return builder.ToString();
        }

        /// <summary>Deletes the ticked rows, after re-checking that they are still unused.</summary>
        public void DeleteSelected()
        {
            List<CheckableRow> selected = _rows.Where(row => row.IsSelected).ToList();

            if (selected.Count == 0)
            {
                return;
            }

            // The counts are read again, immediately before the delete: the model may
            // have changed since the window was opened, and the rule that an item in
            // use is never deleted has to hold at the moment of the delete.
            IReadOnlyDictionary<long, int> instanceCounts = FamilyInventory.CountInstances(_document);
            PurgePlan plan = PurgePlan.Build(selected.Select(row => row.ItemId), instanceCounts);

            DeletionOutcome outcome = ElementDeletion.Delete(_document, plan.Doomed, Text.TransactionName);

            var deleted = new HashSet<long>(outcome.Deleted);

            foreach (CheckableRow row in selected.Where(row => deleted.Contains(row.ItemId)).ToList())
            {
                Remove(row);
            }

            foreach (CheckableRow row in selected)
            {
                row.IsSelected = false;
            }

            _log?.Info(
                "Duplicate scan: deleted " + outcome.Deleted.Count +
                ", skipped " + plan.Blocked.Count +
                ", failed " + outcome.Failures.Count + ".",
                nameof(DuplicateFinderViewModel));

            SetReport(BuildDeleteReport(outcome, plan.Blocked.Count));
            RaisePropertyChanged(nameof(Subtitle));
        }

        /// <summary>Shows one instance of a row in the model.</summary>
        /// <param name="row">The row to show.</param>
        /// <returns><c>true</c> when the element was found and shown.</returns>
        public bool Zoom(CheckableRow row)
        {
            if (row == null || row.ZoomTypeId is not long typeId)
            {
                return false;
            }

            ElementId? instance = FamilyInventory.FindInstance(_document, typeId);

            if (instance == null)
            {
                return false;
            }

            _uiDocument.Selection.SetElementIds(new List<ElementId> { instance });
            _uiDocument.ShowElements(instance);
            ZoomRequested?.Invoke();

            return true;
        }

        private void Track(CheckableRow row)
        {
            _rows.Add(row);
            row.PropertyChanged += OnRowChanged;
        }

        private void Remove(CheckableRow row)
        {
            row.PropertyChanged -= OnRowChanged;
            _rows.Remove(row);

            if (row is TypeRowViewModel type)
            {
                TypeRows.Remove(type);
                RaisePropertyChanged(nameof(TypesTabHeader));
            }
            else if (row is FamilyRowViewModel family)
            {
                FamilyRows.Remove(family);
                RaisePropertyChanged(nameof(FamiliesTabHeader));
            }
        }

        private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckableRow.IsSelected))
            {
                RaisePropertyChanged(nameof(SelectedCount));
                RaisePropertyChanged(nameof(HasSelection));
                RaisePropertyChanged(nameof(DeleteButtonText));
            }
        }

        private void SetReport(string report)
        {
            _report = report;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private string BuildSummary()
        {
            int removableTypes = TypeRows.Count(row => row.IsRemovable);
            int removableFamilies = FamilyRows.Count(row => row.IsRemovable);

            return Text.SummaryChecked(_checkedTypeCount) + "   " +
                Text.SummaryTypes(TypeRows.Count, removableTypes) + "   " +
                Text.SummaryFamilies(FamilyRows.Count, removableFamilies);
        }

        private string BuildInitialReport(IReadOnlyList<string> readErrors)
        {
            if (readErrors.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            builder.AppendLine(Text.ReadErrorsLine(readErrors.Count));

            foreach (string error in readErrors)
            {
                builder.Append("  ");
                builder.AppendLine(error);
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildDeleteReport(DeletionOutcome outcome, int skippedCount)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Text.DeletedLine(outcome.Deleted.Count));

            if (skippedCount > 0)
            {
                builder.AppendLine(Text.SkippedLine(skippedCount));
            }

            if (outcome.Failures.Count > 0)
            {
                builder.AppendLine(Text.NotDeletedLine(outcome.Failures.Count));

                foreach (string failure in outcome.Failures.Take(ReportLimit))
                {
                    builder.Append("  ");
                    builder.AppendLine(failure);
                }

                if (outcome.Failures.Count > ReportLimit)
                {
                    builder.AppendLine(Text.MoreLine(outcome.Failures.Count - ReportLimit));
                }
            }

            return builder.ToString().TrimEnd();
        }
    }
}
