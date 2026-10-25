using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.SheetTransfer;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// The state of the sheet-copy window: the sheets of this project, the open
    /// projects that can receive them, and the run that carries every ticked sheet
    /// into every ticked project.
    /// <para>
    /// Each target project gets its own transaction and, inside it, each sheet gets a
    /// sub-transaction, so a sheet that fails takes only itself down and the rest of
    /// the plan is still committed into that project.
    /// </para>
    /// </summary>
    public sealed class SheetCopyViewModel : ObservableObject
    {
        /// <summary>How often a refused sheet number is regenerated before the copy fails.</summary>
        private const int MaxNumberAttempts = 10;

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _source;
        private readonly Log _log;
        private readonly List<SheetRowViewModel> _rows = new List<SheetRowViewModel>();
        private readonly List<TargetDocumentViewModel> _targets = new List<TargetDocumentViewModel>();
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="source">The active project the sheets come from.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <param name="sheets">Sheets read from the active project.</param>
        /// <param name="openDocuments">Every project open in the running session.</param>
        public SheetCopyViewModel(
            Document source,
            SheetCopyText text,
            Log log,
            IReadOnlyList<SheetCandidate> sheets,
            IReadOnlyList<Document> openDocuments)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            if (sheets is null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            if (openDocuments is null)
            {
                throw new ArgumentNullException(nameof(openDocuments));
            }

            // The rules decide what the user may tick and in what order the sheets
            // are shown: transferable only, natural sheet-number order.
            foreach (SheetCandidate sheet in SheetTransferRules.SelectTransferable(sheets))
            {
                var row = new SheetRowViewModel(sheet);
                row.PropertyChanged += OnRowChanged;
                _rows.Add(row);
            }

            foreach (Document document in openDocuments)
            {
                if (ReferenceEquals(document, _source) || !document.IsValidObject)
                {
                    continue;
                }

                var target = new TargetDocumentViewModel(document, Describe(document), DescribeProblem(document));
                target.PropertyChanged += OnTargetChanged;
                _targets.Add(target);
            }
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SheetCopyText Text { get; }

        /// <summary>Gets the sheet rows shown in the list.</summary>
        public IReadOnlyList<SheetRowViewModel> Rows => _rows;

        /// <summary>Gets the open projects that were offered as targets.</summary>
        public IReadOnlyList<TargetDocumentViewModel> Targets => _targets;

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the line that names the source project.</summary>
        public string SourceLine => Text.SourceLine(_source.Title);

        /// <summary>Gets the caption of the sheets list, with the number of sheets.</summary>
        public string SheetsHeader => Text.SheetsHeader + " (" + SheetCopyText.Number(_rows.Count) + ")";

        /// <summary>Gets the caption of the target list, with the number of open projects.</summary>
        public string TargetsHeader => Text.TargetsHeader + " (" + SheetCopyText.Number(_targets.Count) + ")";

        /// <summary>Gets a value indicating whether this project had no sheets to show.</summary>
        public bool IsEmpty => _rows.Count == 0;

        /// <summary>Gets a value indicating whether any other project is open at all.</summary>
        public bool HasTargets => _targets.Count > 0;

        /// <summary>Gets a value indicating whether the target list has nothing to show.</summary>
        public bool HasNoTargets => _targets.Count == 0;

        /// <summary>Gets a value indicating whether any target can take sheets.</summary>
        public bool HasUsableTargets
        {
            get
            {
                foreach (TargetDocumentViewModel target in _targets)
                {
                    if (target.CanReceive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>Gets the number of ticked sheet rows.</summary>
        public int SelectedCount
        {
            get
            {
                int count = 0;

                foreach (SheetRowViewModel row in _rows)
                {
                    if (row.IsSelected)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>Gets the number of ticked target projects.</summary>
        public int SelectedTargetCount
        {
            get
            {
                int count = 0;

                foreach (TargetDocumentViewModel target in _targets)
                {
                    if (target.IsSelected && target.CanReceive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>Gets a value indicating whether anything is ticked.</summary>
        public bool HasSelection => SelectedCount > 0;

        /// <summary>Gets the caption of the copy button, with both counts.</summary>
        public string CopyButtonText =>
            Text.Copy + " (" + SheetCopyText.Number(SelectedCount) + " → " + SheetCopyText.Number(SelectedTargetCount) + ")";

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Ticks every sheet row.</summary>
        public void SelectAll()
        {
            foreach (SheetRowViewModel row in _rows)
            {
                row.IsSelected = true;
            }
        }

        /// <summary>Unticks every sheet row.</summary>
        public void ClearSelection()
        {
            foreach (SheetRowViewModel row in _rows)
            {
                row.IsSelected = false;
            }
        }

        /// <summary>Ticks every target project that can take sheets.</summary>
        public void SelectAllTargets()
        {
            foreach (TargetDocumentViewModel target in _targets)
            {
                if (target.CanReceive)
                {
                    target.IsSelected = true;
                }
            }
        }

        /// <summary>Unticks every target project.</summary>
        public void ClearTargets()
        {
            foreach (TargetDocumentViewModel target in _targets)
            {
                target.IsSelected = false;
            }
        }

        /// <summary>
        /// Carries the ticked sheets into every ticked project: one transaction per
        /// project, one sub-transaction per sheet inside it, and a sheet that fails is
        /// reported while the rest of that project's plan continues.
        /// </summary>
        /// <returns>What the run did to every project it entered.</returns>
        public SheetCopyRunResult CopySelected()
        {
            var selected = new List<SheetCandidate>();
            var counts = new Dictionary<long, int>();

            foreach (SheetRowViewModel row in _rows)
            {
                if (!row.IsSelected)
                {
                    continue;
                }

                selected.Add(row.Item);
                counts[row.Item.Id] = row.CopyCount;
            }

            var results = new List<TargetCopyResult>();

            foreach (TargetDocumentViewModel target in _targets)
            {
                if (target.IsSelected && target.CanReceive)
                {
                    results.Add(CopyInto(target, selected, counts));
                }
            }

            int created = 0;
            int planned = 0;

            foreach (TargetCopyResult result in results)
            {
                created += result.Created;
                planned += result.Planned;
            }

            _log.Info(
                "SheetCopy: created " + SheetCopyText.Number(created) + " of " +
                SheetCopyText.Number(planned) + " planned copies in " +
                SheetCopyText.Number(results.Count) + " project(s).",
                nameof(SheetCopyViewModel));

            return new SheetCopyRunResult(results);
        }

        /// <summary>
        /// Builds the text of the result: one summary line per target project and the
        /// sheets that could not be copied, capped so the box stays readable.
        /// </summary>
        /// <param name="result">The run to describe.</param>
        /// <returns>The report text.</returns>
        public string BuildReport(SheetCopyRunResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var builder = new StringBuilder();
            int shown = 0;
            int total = 0;

            foreach (TargetCopyResult target in result.Targets)
            {
                builder.AppendLine(Text.TargetSummary(target.Created, target.Planned, target.TargetName));

                foreach (string failure in target.Failures)
                {
                    total++;

                    if (shown >= ReportLimit)
                    {
                        continue;
                    }

                    builder.Append("  ");
                    builder.AppendLine(failure);
                    shown++;
                }
            }

            if (total > shown)
            {
                builder.AppendLine(Text.MoreLine(total - shown));
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

        /// <summary>
        /// Copies every planned sheet of one run into one project. A number the
        /// project refuses is added to the occupied set and the next candidate is
        /// generated, up to a small fixed number of tries.
        /// </summary>
        /// <param name="target">The project that receives the sheets.</param>
        /// <param name="selected">The ticked sheets.</param>
        /// <param name="counts">How many copies of each sheet the user asked for, by sheet id.</param>
        /// <returns>What the run did to that project.</returns>
        private TargetCopyResult CopyInto(
            TargetDocumentViewModel target,
            IReadOnlyList<SheetCandidate> selected,
            IReadOnlyDictionary<long, int> counts)
        {
            Document document = target.Document;

            // The project may have been closed while the window was open.
            if (!document.IsValidObject)
            {
                return new TargetCopyResult(
                    target.Description,
                    0,
                    0,
                    new[] { Text.TargetFailed(target.Description, Text.TargetLost) },
                    false);
            }

            var taken = new HashSet<string>(
                SheetInventory.ReadSheetNumbers(document),
                StringComparer.OrdinalIgnoreCase);

            TransferPlan plan = TransferPlan.Build(selected, taken, Text.CopyMarker, counts);

            var failures = new List<string>();
            int created = 0;
            bool committed = false;

            try
            {
                committed = DocumentEdit.Run(document, Text.TransactionName, () =>
                {
                    foreach (SheetTransferItem item in plan.Items)
                    {
                        var sheetEdit = new SubTransaction(document);
                        sheetEdit.Start();

                        try
                        {
                            string number = CopySheet(document, item, taken);
                            sheetEdit.Commit();
                            created++;

                            _log.Info(
                                "SheetCopy: sheet " + item.SourceNumber + " copied into '" +
                                target.Description + "' as " + number + " with " +
                                SheetCopyText.Number(item.ViewCount) + " views.",
                                nameof(SheetCopyViewModel));
                        }
                        catch (Exception exception)
                        {
                            try
                            {
                                sheetEdit.RollBack();
                            }
                            catch (Exception)
                            {
                                // The sub-transaction is already gone; the failure of
                                // the sheet is what matters, not the rollback.
                            }

                            failures.Add(Text.SheetFailed(item.SourceNumber, exception.Message));

                            _log.Warn(
                                "SheetCopy: sheet " + item.SourceNumber + " failed in '" +
                                target.Description + "': " + exception.Message,
                                nameof(SheetCopyViewModel),
                                exception);
                        }
                    }

                    return created > 0;
                });
            }
            catch (Exception exception)
            {
                // A project the run cannot even enter is reported, and the projects
                // behind it are still served.
                failures.Add(Text.TargetFailed(target.Description, exception.Message));

                _log.Error(
                    "SheetCopy: project '" + target.Description + "' failed: " + exception.Message,
                    nameof(SheetCopyViewModel),
                    exception);
            }

            return new TargetCopyResult(target.Description, created, plan.PlannedCount, failures, committed);
        }

        /// <summary>
        /// Copies one sheet into the target project and gives the copy its number and
        /// name.
        /// </summary>
        /// <param name="target">The project that receives the sheet.</param>
        /// <param name="item">The planned copy to produce.</param>
        /// <param name="taken">Numbers that are occupied; the assigned number is added to it.</param>
        /// <returns>The sheet number the copy received.</returns>
        private string CopySheet(Document target, SheetTransferItem item, HashSet<string> taken)
        {
            var sourceIds = new List<ElementId> { ElementIdCompat.Create(item.SourceSheetId) };
            var options = new CopyPasteOptions();

            ICollection<ElementId> copied = ElementTransformUtils.CopyElements(
                _source,
                sourceIds,
                target,
                Transform.Identity,
                options);

            ViewSheet? copy = null;

            if (copied != null)
            {
                foreach (ElementId copiedId in copied)
                {
                    if (target.GetElement(copiedId) is ViewSheet sheet)
                    {
                        copy = sheet;
                        break;
                    }
                }
            }

            if (copy == null)
            {
                throw new InvalidOperationException(Text.CopyReturnedNothing);
            }

            string? assigned = null;

            for (int attempt = 0; attempt < MaxNumberAttempts && assigned == null; attempt++)
            {
                // The first attempt uses the number the plan worked out against the
                // target's own numbers, so the copy receives what the run promised;
                // later attempts fall back to a fresh candidate for the case where the
                // project moved on after the plan was made.
                string candidate = attempt == 0
                    ? item.Number
                    : SheetTransferRules.MakeUniqueNumber(item.SourceNumber, taken);

                try
                {
                    copy.SheetNumber = candidate;
                    taken.Add(candidate);
                    assigned = candidate;
                }
                catch (Exception)
                {
                    // The number was taken or refused after the plan was made; treat
                    // it as occupied and generate the next candidate.
                    taken.Add(candidate);
                }
            }

            if (assigned == null)
            {
                throw new InvalidOperationException(Text.NumberRefused);
            }

            // Sheet names do not have to be unique, so the marked name is written as
            // it is; a name Revit refuses fails only this sheet.
            copy.Name = item.Name;

            return assigned;
        }

        private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetRowViewModel.IsSelected)
                || e.PropertyName == nameof(SheetRowViewModel.CopyCount))
            {
                RaisePropertyChanged(nameof(SelectedCount));
                RaisePropertyChanged(nameof(HasSelection));
                RaisePropertyChanged(nameof(CopyButtonText));
            }
        }

        private void OnTargetChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TargetDocumentViewModel.IsSelected))
            {
                RaisePropertyChanged(nameof(SelectedTargetCount));
                RaisePropertyChanged(nameof(CopyButtonText));
            }
        }

        /// <summary>Names an open project the way the target list shows it.</summary>
        /// <param name="document">The project to name.</param>
        /// <returns>Its title, its file, or a note that it has never been saved.</returns>
        private string Describe(Document document)
        {
            string title = document.Title;

            if (!string.IsNullOrEmpty(title))
            {
                return title;
            }

            string path = document.PathName;

            return string.IsNullOrEmpty(path) ? Text.TargetUnsaved : path;
        }

        /// <summary>Tells what stops a project from taking sheets, if anything does.</summary>
        /// <param name="document">The project to read.</param>
        /// <returns>The reason, or <c>null</c> when the project can take sheets.</returns>
        private string? DescribeProblem(Document document)
        {
            if (document.IsFamilyDocument)
            {
                return Text.TargetFamily;
            }

            if (document.IsLinked)
            {
                return Text.TargetLinked;
            }

            if (document.IsReadOnly)
            {
                return Text.TargetReadOnly;
            }

            return null;
        }
    }
}
