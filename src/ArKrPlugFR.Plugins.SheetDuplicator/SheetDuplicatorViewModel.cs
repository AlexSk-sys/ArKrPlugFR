using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Sheets;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// The state of the sheet window: which sheets are listed, which are ticked,
    /// how many copies each asks for, what content travels, and the run itself.
    /// <para>
    /// The whole run is one transaction: a sheet that fails is reported and the
    /// run continues with the rest, and the copies of a copy that fails part-way
    /// are deleted again before the loop moves on, so a finished run never
    /// leaves half a sheet behind.
    /// </para>
    /// </summary>
    public sealed class SheetDuplicatorViewModel : ObservableObject
    {
        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        /// <summary>How often a taken sheet number is bumped before the copy is abandoned.</summary>
        private const int MaxNumberAttempts = 100;

        private readonly Document _document;
        private readonly SheetDuplicatorText _text;
        private readonly Log _log;
        private readonly List<SheetRowViewModel> _rows = new List<SheetRowViewModel>();

        private bool _copyViews = true;
        private bool _copyLegends = true;
        private bool _copyText = true;
        private bool _copyImages = true;
        private bool _copySchedules = true;
        private bool _copyAnnotations = true;
        private bool _copyParameters;
        private string _searchText = string.Empty;
        private string _report = string.Empty;

        /// <summary>Creates the view model from the sheets of the document.</summary>
        /// <param name="document">The project the sheets come from.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <param name="sheets">Every collected sheet, placeholders included; the rules pick the usable ones.</param>
        public SheetDuplicatorViewModel(
            Document document,
            SheetDuplicatorText text,
            Log log,
            IReadOnlyList<SheetFacts> sheets)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            if (sheets == null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            foreach (SheetFacts sheet in SheetRules.SelectDuplicable(sheets))
            {
                _rows.Add(new SheetRowViewModel(sheet));
            }

            _rows.Sort((left, right) => SheetRules.NaturalCompare(left.Number, right.Number));

            VisibleRows = new ObservableCollection<SheetRowViewModel>();
            ApplyFilter();

            foreach (SheetRowViewModel row in _rows)
            {
                row.PropertyChanged += OnRowChanged;
            }
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SheetDuplicatorText Text => _text;

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => _text.WindowTitle;

        /// <summary>Gets the English line under the caption.</summary>
        public string Subtitle => _text.Subtitle;

        /// <summary>Gets the rows the list currently shows, after the search filter.</summary>
        public ObservableCollection<SheetRowViewModel> VisibleRows { get; }

        /// <summary>Gets or sets the search text that filters the list.</summary>
        public string SearchText
        {
            get => _searchText;

            set
            {
                string normalized = value ?? string.Empty;

                if (SetProperty(ref _searchText, normalized))
                {
                    ApplyFilter();
                }
            }
        }

        /// <summary>Gets or sets the switch that duplicates placed views.</summary>
        public bool CopyViews
        {
            get => _copyViews;
            set => SetProperty(ref _copyViews, value);
        }

        /// <summary>Gets or sets the switch that places legends again.</summary>
        public bool CopyLegends
        {
            get => _copyLegends;
            set => SetProperty(ref _copyLegends, value);
        }

        /// <summary>Gets or sets the switch that copies text notes.</summary>
        public bool CopyText
        {
            get => _copyText;
            set => SetProperty(ref _copyText, value);
        }

        /// <summary>Gets or sets the switch that copies raster images.</summary>
        public bool CopyImages
        {
            get => _copyImages;
            set => SetProperty(ref _copyImages, value);
        }

        /// <summary>Gets or sets the switch that places schedule views on the copy again.</summary>
        public bool CopySchedules
        {
            get => _copySchedules;
            set => SetProperty(ref _copySchedules, value);
        }

        /// <summary>Gets or sets the switch that copies annotations, lines and detailing.</summary>
        public bool CopyAnnotations
        {
            get => _copyAnnotations;
            set => SetProperty(ref _copyAnnotations, value);
        }

        /// <summary>Gets or sets the switch that copies sheet and stamp parameters.</summary>
        public bool CopyParameters
        {
            get => _copyParameters;
            set => SetProperty(ref _copyParameters, value);
        }

        /// <summary>Gets the number of ticked sheets.</summary>
        public int SelectedCount => _rows.Count(row => row.IsSelected);

        /// <summary>Gets a value indicating whether anything is ticked.</summary>
        public bool HasSelection => SelectedCount > 0;

        /// <summary>Gets the caption of the run button, with the number of ticked sheets.</summary>
        public string DuplicateButtonText => _text.DuplicateButtonText(SelectedCount);

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => _text.ResultCaption;

        /// <summary>Ticks every listed sheet.</summary>
        public void SelectAll()
        {
            foreach (SheetRowViewModel row in _rows)
            {
                row.IsSelected = true;
            }
        }

        /// <summary>Unticks every listed sheet.</summary>
        public void ClearSelection()
        {
            foreach (SheetRowViewModel row in _rows)
            {
                row.IsSelected = false;
            }
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
        /// Duplicates every ticked sheet with its copies in one transaction and
        /// reports what happened.
        /// </summary>
        /// <returns>The outcome to show; never <c>null</c>.</returns>
        public SheetRunOutcome Duplicate()
        {
            List<SheetRowViewModel> selected = _rows.Where(row => row.IsSelected).ToList();

            if (selected.Count == 0)
            {
                return SheetRunOutcome.NoneSelected();
            }

            var failures = new List<string>();
            int createdSheetCount = 0;

            try
            {
                var state = new RunState(
                    new SheetCopyOptions(
                        _copyViews,
                        _copyLegends,
                        _copyText,
                        _copyImages,
                        _copySchedules,
                        _copyAnnotations,
                        _copyParameters),
                    CollectPlacedViews(),
                    CollectSheetNumbers(),
                    CollectViewNames(),
                    CollectOwnedBySheet(),
                    failures);

                foreach (SheetRowViewModel row in selected)
                {
                    var source = _document.GetElement(ElementIdCompat.Create(row.SheetId)) as ViewSheet;

                    if (source == null || !source.IsValidObject)
                    {
                        failures.Add(_text.FailedSheet(row.Number, _text.ReasonSheetMissing));
                        continue;
                    }

                    // Each sheet is its own undoable step, named with the sheet number
                    // and the number of copies it holds, so one sheet can be taken out
                    // of the model again without touching the others.
                    try
                    {
                        int produced = 0;

                        DocumentEdit.Run(
                            _document,
                            _text.TransactionNameFor(row.Number, row.CopyCount),
                            () =>
                            {
                                produced = CopySheet(source, row, state);
                                return produced > 0;
                            });

                        createdSheetCount += produced;
                    }
                    catch (Exception exception)
                    {
                        // An error escaping a sheet ends the run: the sheets behind it
                        // are left untouched.
                        _log.Error(
                            "Sheet " + row.Number + " failed: " + exception.Message,
                            nameof(SheetDuplicatorViewModel),
                            exception);

                        failures.Add(_text.FailedSheet(row.Number, exception.Message));
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Error(
                    "Sheet duplication failed: " + exception.Message,
                    nameof(SheetDuplicatorViewModel),
                    exception);

                failures.Add(_text.GeneralFailure(exception.Message));
            }

            return SheetRunOutcome.Completed(BuildMessage(failures), failures.Count > 0);
        }

        private void ApplyFilter()
        {
            VisibleRows.Clear();

            foreach (SheetRowViewModel row in _rows)
            {
                if (_searchText.Length == 0
                    || row.Number.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || row.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    VisibleRows.Add(row);
                }
            }
        }

        private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetRowViewModel.IsSelected)
                || e.PropertyName == nameof(SheetRowViewModel.CopyCount))
            {
                RaisePropertyChanged(nameof(SelectedCount));
                RaisePropertyChanged(nameof(HasSelection));
                RaisePropertyChanged(nameof(DuplicateButtonText));
            }
        }

        private string BuildMessage(IReadOnlyList<string> failures)
        {
            if (failures.Count == 0)
            {
                return _text.Success;
            }

            var builder = new StringBuilder();
            int shown = Math.Min(failures.Count, ReportLimit);

            for (int index = 0; index < shown; index++)
            {
                builder.AppendLine(failures[index]);
            }

            if (failures.Count > ReportLimit)
            {
                builder.Append(_text.MoreLine(failures.Count - ReportLimit));
            }

            return builder.ToString().TrimEnd();
        }

        private int CopySheet(ViewSheet source, SheetRowViewModel row, RunState state)
        {
            string sourceNumber = source.SheetNumber;
            string sourceName = source.Name;

            List<PlacedOnSheet> placedViews = FindPlaced(state, row.SheetId);

            var facts = new List<SourceViewFact>(placedViews.Count);

            foreach (PlacedOnSheet placed in placedViews)
            {
                facts.Add(new SourceViewFact(placed.View.Name, ClassifyView(placed.View)));
            }

            SheetCopyPlan plan = SheetViewPlanner.Plan(facts, state.Options);

            int baseSuffix = SheetNumbering.FindHighestSuffix(sourceNumber, state.SheetNumbers);
            IReadOnlyList<string> plannedNumbers = SheetNumbering.GenerateNumbers(
                sourceNumber,
                state.SheetNumbers,
                row.CopyCount);

            FamilyInstance? sourceStamp = FindTitleBlock(state, row.SheetId);

            int created = 0;

            for (int index = 0; index < row.CopyCount; index++)
            {
                created += CreateOneCopy(
                    source,
                    row,
                    sourceName,
                    index + 1,
                    plannedNumbers[index],
                    baseSuffix + index + 1,
                    plan,
                    placedViews,
                    sourceStamp,
                    state);
            }

            return created;
        }

        /// <summary>
        /// Puts the stamp of the source sheet on its copy, at the same point.
        /// </summary>
        /// <param name="copy">The sheet that receives the stamp.</param>
        /// <param name="sourceStamp">The stamp of the source sheet, when it has one.</param>
        private void PlaceStamp(ViewSheet copy, FamilyInstance? sourceStamp)
        {
            if (sourceStamp == null || !sourceStamp.IsValidObject)
            {
                return;
            }

            try
            {
                XYZ location = (sourceStamp.Location as LocationPoint)?.Point ?? XYZ.Zero;

                _document.Create.NewFamilyInstance(location, sourceStamp.Symbol, copy);
            }
            catch (Exception exception)
            {
                // A copy without its stamp is still a usable sheet, so only the log
                // records the loss.
                _log.Warn(
                    "Stamp of a sheet copy was not placed: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));
            }
        }

        private int CreateOneCopy(
            ViewSheet source,
            SheetRowViewModel row,
            string sourceName,
            int copyNumber,
            string plannedNumber,
            int plannedSuffix,
            SheetCopyPlan plan,
            List<PlacedOnSheet> placedViews,
            FamilyInstance? sourceStamp,
            RunState state)
        {
            var createdIds = new List<long>();

            try
            {
                // The copy starts as an empty sheet: the stamp is placed by hand right
                // after, so it sits where the source stamp sits rather than at the
                // platform's default spot for the title-block type.
                ViewSheet copy = ViewSheet.Create(_document, ElementId.InvalidElementId);

                createdIds.Add(ElementIdCompat.GetValue(copy.Id));

                PlaceStamp(copy, sourceStamp);

                if (!TryAssignNumber(copy, plannedNumber, source.SheetNumber, plannedSuffix, state))
                {
                    throw new InvalidOperationException(_text.ReasonNoFreeNumber);
                }

                copy.Name = SheetCopyNames.MarkedSheetName(sourceName, copyNumber);

                CopyViewsOnto(copy, plan, placedViews, createdIds, state);
                CopySheetContent(source, copy, row, state);

                if (state.Options.CopyParameters)
                {
                    CopySheetParameters(source, copy, sourceStamp);
                }

                return 1;
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "Copy " + copyNumber + " of sheet " + row.Number + " abandoned: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));

                state.Failures.Add(_text.SheetIssue(row.Number, exception.Message));
                Abandon(createdIds);

                return 0;
            }
        }

        private static bool TryAssignNumber(
            ViewSheet copy,
            string plannedNumber,
            string sourceNumber,
            int plannedSuffix,
            RunState state)
        {
            if (TrySetNumber(copy, plannedNumber, state))
            {
                return true;
            }

            for (int offset = 1; offset <= MaxNumberAttempts; offset++)
            {
                string candidate = SheetNumbering.WithSuffix(sourceNumber, plannedSuffix + offset);

                if (TrySetNumber(copy, candidate, state))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TrySetNumber(ViewSheet copy, string number, RunState state)
        {
            try
            {
                copy.SheetNumber = number;
                state.SheetNumbers.Add(number);
                return true;
            }
            catch (Exception)
            {
                // The number was taken meanwhile; the run bumps the suffix instead
                // of giving up.
                return false;
            }
        }

        private void Abandon(IReadOnlyList<long> createdIds)
        {
            foreach (long id in createdIds)
            {
                try
                {
                    ElementId elementId = ElementIdCompat.Create(id);

                    if (_document.GetElement(elementId) != null)
                    {
                        _document.Delete(elementId);
                    }
                }
                catch (Exception)
                {
                    // Best effort: an element that refuses the delete is left to
                    // the user's undo rather than stopping the run.
                }
            }
        }

        private void CopyViewsOnto(
            ViewSheet copy,
            SheetCopyPlan plan,
            List<PlacedOnSheet> placedViews,
            List<long> createdIds,
            RunState state)
        {
            for (int index = 0; index < plan.Views.Count && index < placedViews.Count; index++)
            {
                PlannedView planned = plan.Views[index];
                PlacedOnSheet placed = placedViews[index];

                if (planned.Action == SheetViewAction.Duplicate)
                {
                    DuplicateOneView(placed.View, copy, placed.Viewport.GetBoxCenter(), createdIds, state);
                }
                else if (planned.Action == SheetViewAction.Reuse)
                {
                    try
                    {
                        Viewport.Create(_document, copy.Id, placed.View.Id, placed.Viewport.GetBoxCenter());
                    }
                    catch (Exception exception)
                    {
                        _log.Warn(
                            "View " + placed.View.Name + " was not placed: " + exception.Message,
                            nameof(SheetDuplicatorViewModel));

                        state.Failures.Add(BuildUnplaceableMessage(placed.View, planned.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Copies the detailing of the source view into its duplicate.
        /// <para>
        /// A plain duplicate is empty of detailing: the lines, dimensions, detail
        /// components, tags and revision clouds the view was drawn with stay in the
        /// source, so every element the view owns is pasted over into the copy.
        /// </para>
        /// </summary>
        /// <param name="sourceView">The view being duplicated.</param>
        /// <param name="duplicate">The copy that needs the same detailing.</param>
        private void PasteViewDetails(View sourceView, View duplicate)
        {
            var ids = new List<ElementId>();

            foreach (ElementId id in new FilteredElementCollector(sourceView.Document, sourceView.Id)
                .WhereElementIsNotElementType()
                .ToElementIds())
            {
                if (sourceView.Document.GetElement(id) is Element element
                    && element.ViewSpecific
                    && element.Category != null)
                {
                    ids.Add(id);
                }
            }

            if (ids.Count == 0)
            {
                return;
            }

            try
            {
                ElementTransformUtils.CopyElements(
                    sourceView,
                    ids,
                    duplicate,
                    Transform.Identity,
                    new CopyPasteOptions());

                return;
            }
            catch (Exception)
            {
                // The platform refuses a whole batch when one of its elements cannot
                // travel, so the copy is filled one element at a time instead and only
                // the elements that refuse are left behind.
            }

            int refused = 0;

            foreach (ElementId id in ids)
            {
                try
                {
                    ElementTransformUtils.CopyElements(
                        sourceView,
                        new List<ElementId> { id },
                        duplicate,
                        Transform.Identity,
                        new CopyPasteOptions());
                }
                catch (Exception)
                {
                    refused++;
                }
            }

            if (refused > 0)
            {
                _log.Warn(
                    "Detailing of view " + sourceView.Name + ": " + refused + " of " + ids.Count
                    + " elements could not be copied to its duplicate.",
                    nameof(SheetDuplicatorViewModel));
            }
        }

        private void DuplicateOneView(
            View sourceView,
            ViewSheet copy,
            XYZ position,
            List<long> createdIds,
            RunState state)
        {
            if (!sourceView.CanViewBeDuplicated(ViewDuplicateOption.Duplicate))
            {
                state.Failures.Add(_text.CannotDuplicateView(sourceView.Name));
                return;
            }

            ElementId duplicateId = sourceView.Duplicate(ViewDuplicateOption.Duplicate);
            createdIds.Add(ElementIdCompat.GetValue(duplicateId));

            var duplicate = _document.GetElement(duplicateId) as View;

            if (duplicate == null || !duplicate.IsValidObject)
            {
                return;
            }

            try
            {
                if (duplicate.ViewTemplateId != ElementId.InvalidElementId)
                {
                    duplicate.ViewTemplateId = ElementId.InvalidElementId;
                }

                string uniqueName = SheetCopyNames.NextFreeName(
                    sourceView.Name,
                    name => state.TakenViewNames.Contains(name));

                if (uniqueName.Length > 0)
                {
                    duplicate.Name = uniqueName;
                    state.TakenViewNames.Add(uniqueName);
                }
            }
            catch (Exception exception)
            {
                // A name the document refuses is not worth abandoning the copy for.
                _log.Warn(
                    "Name of a view duplicate was not set: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));
            }

            PasteViewDetails(sourceView, duplicate);

            try
            {
                Viewport.Create(_document, copy.Id, duplicateId, position);
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "View " + sourceView.Name + " was not placed: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));

                state.Failures.Add(BuildUnplaceableMessage(sourceView, sourceView.Name));
            }
        }

        private string BuildUnplaceableMessage(View view, string viewName)
        {
            return view.ViewType == ViewType.DraftingView
                ? _text.DraftingUnplaceableView(viewName)
                : _text.UnplaceableView(viewName);
        }

        private void CopySheetContent(ViewSheet source, ViewSheet copy, SheetRowViewModel row, RunState state)
        {
            if (state.Options.CopyAnnotations)
            {
                CarryGuideGridReference(source, copy);
            }

            long sourceSheetId = ElementIdCompat.GetValue(source.Id);

            if (!state.OwnedBySheet.TryGetValue(sourceSheetId, out List<Element>? owned) || owned == null)
            {
                return;
            }

            var textIds = new List<ElementId>();
            var imageIds = new List<ElementId>();
            var annotationIds = new List<ElementId>();

            foreach (Element element in owned)
            {
                if (!element.IsValidObject || IsTitleBlock(element) || IsStampPart(element))
                {
                    continue;
                }

                if (element is ScheduleSheetInstance schedule)
                {
                    // A schedule sits on a sheet as an element of its own rather than as
                    // a viewport, so it never travels with the pasted content and has to
                    // be placed on the copy by hand. The revision schedules belong to the
                    // stamp and come back with it.
                    if (state.Options.CopySchedules && !schedule.IsTitleblockRevisionSchedule)
                    {
                        PlaceSchedule(copy, row, schedule, state);
                    }

                    continue;
                }

                if (IsPlatformOwned(element))
                {
                    continue;
                }

                if (IsImageInstance(element))
                {
                    if (state.Options.CopyImages)
                    {
                        imageIds.Add(element.Id);
                    }
                }
                else if (element is TextNote)
                {
                    if (state.Options.CopyText)
                    {
                        textIds.Add(element.Id);
                    }
                }
                else if (state.Options.CopyAnnotations)
                {
                    annotationIds.Add(element.Id);
                }
            }

            PasteGroup(source, copy, textIds, row, state);
            PasteGroup(source, copy, imageIds, row, state);
            PasteGroup(source, copy, annotationIds, row, state);
        }

        /// <summary>
        /// Places the schedule of a source sheet on its copy at the same point.
        /// </summary>
        /// <param name="copy">The sheet the schedule is placed on.</param>
        /// <param name="row">Row of the run, used for the failure message.</param>
        /// <param name="schedule">The schedule instance of the source sheet.</param>
        /// <param name="state">State of the run.</param>
        private void PlaceSchedule(
            ViewSheet copy,
            SheetRowViewModel row,
            ScheduleSheetInstance schedule,
            RunState state)
        {
            try
            {
                XYZ location = (schedule.Location as LocationPoint)?.Point ?? XYZ.Zero;

                ScheduleSheetInstance.Create(_document, copy.Id, schedule.ScheduleId, location);
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "Schedule of sheet " + row.Number + " was not placed: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));

                state.Failures.Add(_text.SheetIssue(row.Number, exception.Message));
            }
        }

        private void PasteGroup(
            ViewSheet source,
            ViewSheet copy,
            List<ElementId> ids,
            SheetRowViewModel row,
            RunState state)
        {
            if (ids.Count == 0)
            {
                return;
            }

            try
            {
                ElementTransformUtils.CopyElements(
                    source,
                    ids,
                    copy,
                    Transform.Identity,
                    new CopyPasteOptions());
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "Content of sheet " + row.Number + " was not pasted: " + exception.Message,
                    nameof(SheetDuplicatorViewModel));

                state.Failures.Add(_text.SheetIssue(row.Number, exception.Message));
            }
        }

        private void CopySheetParameters(ViewSheet source, ViewSheet copy, FamilyInstance? sourceStamp)
        {
            CopyParametersBetween(source, copy);

            if (sourceStamp == null)
            {
                return;
            }

            foreach (Element element in new FilteredElementCollector(_document, copy.Id).WhereElementIsNotElementType())
            {
                if (element is FamilyInstance stamp && IsTitleBlock(stamp))
                {
                    CopyParametersBetween(sourceStamp, stamp);
                    break;
                }
            }
        }

        private void CopyParametersBetween(Element source, Element target)
        {
            foreach (Parameter sourceParameter in source.Parameters)
            {
                if (sourceParameter == null || !sourceParameter.HasValue)
                {
                    continue;
                }

                Definition? definition = sourceParameter.Definition;

                if (definition == null || ParameterCopyRules.ShouldSkip(definition.Name))
                {
                    continue;
                }

                Parameter? targetParameter = target.get_Parameter(definition);

                if (targetParameter == null)
                {
                    continue;
                }

                try
                {
                    WriteParameter(targetParameter, sourceParameter);
                }
                catch (Exception exception)
                {
                    // Read-only, absent or unsupported parameters are skipped
                    // silently; the copy itself is still sound.
                    _log.Warn(
                        "Parameter " + definition.Name + " was not copied: " + exception.Message,
                        nameof(SheetDuplicatorViewModel));
                }
            }
        }

        private static void WriteParameter(Parameter target, Parameter source)
        {
            switch (source.StorageType)
            {
                case StorageType.String:
                    target.Set(source.AsString());
                    break;

                case StorageType.Double:
                    target.Set(source.AsDouble());
                    break;

                case StorageType.Integer:
                    target.Set(source.AsInteger());
                    break;

                case StorageType.ElementId:
                    target.Set(source.AsElementId());
                    break;
            }
        }

        private void CarryGuideGridReference(ViewSheet source, ViewSheet copy)
        {
            foreach (Parameter sourceParameter in source.Parameters)
            {
                if (sourceParameter == null || !sourceParameter.HasValue)
                {
                    continue;
                }

                if (!ParameterCopyRules.IsGuideGridReference(sourceParameter.Definition?.Name))
                {
                    continue;
                }

                Parameter? target = copy.get_Parameter(sourceParameter.Definition);

                if (target == null || sourceParameter.StorageType != StorageType.ElementId)
                {
                    continue;
                }

                try
                {
                    target.Set(sourceParameter.AsElementId());
                }
                catch (Exception exception)
                {
                    _log.Warn(
                        "Guide grid of sheet " + source.SheetNumber + " was not carried over: " + exception.Message,
                        nameof(SheetDuplicatorViewModel));
                }
            }
        }

        private Dictionary<long, List<PlacedOnSheet>> CollectPlacedViews()
        {
            var map = new Dictionary<long, List<PlacedOnSheet>>();

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(Viewport)))
            {
                if (element is not Viewport viewport || !viewport.IsValidObject)
                {
                    continue;
                }

                var view = _document.GetElement(viewport.ViewId) as View;

                if (view == null || !view.IsValidObject)
                {
                    continue;
                }

                long sheetId = ElementIdCompat.GetValue(viewport.SheetId);

                if (!map.TryGetValue(sheetId, out List<PlacedOnSheet>? list))
                {
                    list = new List<PlacedOnSheet>();
                    map[sheetId] = list;
                }

                list.Add(new PlacedOnSheet(view, viewport));
            }

            return map;
        }

        private List<string> CollectSheetNumbers()
        {
            var numbers = new List<string>();

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(ViewSheet)))
            {
                if (element is ViewSheet sheet)
                {
                    numbers.Add(sheet.SheetNumber);
                }
            }

            return numbers;
        }

        private HashSet<string> CollectViewNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(View)))
            {
                if (element is View view && !string.IsNullOrEmpty(view.Name))
                {
                    names.Add(view.Name);
                }
            }

            return names;
        }

        private Dictionary<long, List<Element>> CollectOwnedBySheet()
        {
            var map = new Dictionary<long, List<Element>>();

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(ViewSheet)))
            {
                if (element is not ViewSheet sheet || !sheet.IsValidObject)
                {
                    continue;
                }

                long sheetId = ElementIdCompat.GetValue(sheet.Id);
                var owned = new List<Element>();

                foreach (Element candidate in new FilteredElementCollector(_document, sheet.Id).WhereElementIsNotElementType())
                {
                    if (candidate == null || !candidate.IsValidObject)
                    {
                        continue;
                    }

                    if (ElementIdCompat.GetValue(candidate.OwnerViewId) == sheetId)
                    {
                        owned.Add(candidate);
                    }
                }

                map[sheetId] = owned;
            }

            return map;
        }

        private static List<PlacedOnSheet> FindPlaced(RunState state, long sheetId)
        {
            return state.PlacedViews.TryGetValue(sheetId, out List<PlacedOnSheet>? list) && list != null
                ? list
                : new List<PlacedOnSheet>();
        }

        private static FamilyInstance? FindTitleBlock(RunState state, long sheetId)
        {
            if (!state.OwnedBySheet.TryGetValue(sheetId, out List<Element>? owned) || owned == null)
            {
                return null;
            }

            FamilyInstance? standIn = null;

            foreach (Element element in owned)
            {
                if (element is not FamilyInstance instance)
                {
                    continue;
                }

                // A real title block always wins; an annotation that merely reads like
                // a stamp is only kept as the fallback.
                if (IsTitleBlock(instance))
                {
                    return instance;
                }

                if (standIn == null && IsStampStandIn(instance))
                {
                    standIn = instance;
                }
            }

            return standIn;
        }

        /// <summary>
        /// Tells whether an annotation stands in for the stamp: a generic annotation
        /// whose family, type or element name carries a stamp word.
        /// </summary>
        /// <param name="instance">The family instance to read.</param>
        /// <returns><c>true</c> when the instance is a stamp stand-in.</returns>
        private static bool IsStampStandIn(FamilyInstance instance)
        {
            Category? category = instance.Category;

            if (category == null
                || ElementIdCompat.GetValue(category.Id) != (long)(int)BuiltInCategory.OST_GenericAnnotation)
            {
                return false;
            }

            return StampKeywords.LooksLikeStamp(new[]
            {
                instance.Name,
                instance.Symbol?.Name,
                instance.Symbol?.Family?.Name
            });
        }

        /// <summary>
        /// Tells whether a sheet-owned element belongs to the stamp, which the stamp
        /// placement carries over on its own.
        /// </summary>
        /// <param name="element">The element to read.</param>
        /// <returns><c>true</c> when the element is part of the stamp.</returns>
        private static bool IsStampPart(Element element)
        {
            return element is FamilyInstance instance && IsStampStandIn(instance);
        }

        private static SheetViewKind ClassifyView(View view)
        {
            if (view is ViewSchedule)
            {
                return SheetViewKind.Schedule;
            }

            return view.ViewType == ViewType.Legend ? SheetViewKind.Legend : SheetViewKind.View;
        }

        private static bool IsPlatformOwned(Element element)
        {
            return element is Viewport || element is ScheduleSheetInstance;
        }

        private static bool IsTitleBlock(Element element)
        {
            Category? category = element.Category;

            return category != null
                && ElementIdCompat.GetValue(category.Id) == (long)(int)BuiltInCategory.OST_TitleBlocks;
        }

        private static bool IsImageInstance(Element element)
        {
            return element is ImageInstance;
        }

        /// <summary>One view placed on a sheet, with the viewport that carries its position.</summary>
        private sealed class PlacedOnSheet
        {
            /// <summary>Creates one placed view.</summary>
            /// <param name="view">The view itself.</param>
            /// <param name="viewport">The viewport that places it on the sheet.</param>
            public PlacedOnSheet(View view, Viewport viewport)
            {
                View = view;
                Viewport = viewport;
            }

            /// <summary>Gets the view.</summary>
            public View View { get; }

            /// <summary>Gets the viewport that carries the position.</summary>
            public Viewport Viewport { get; }
        }

        /// <summary>Everything one run reads before it starts, and what it collects while it goes.</summary>
        private sealed class RunState
        {
            /// <summary>Creates the state of one run.</summary>
            /// <param name="options">The switches of the run.</param>
            /// <param name="placedViews">Views placed per sheet, keyed by sheet id.</param>
            /// <param name="sheetNumbers">Every sheet number of the document; the run adds the numbers it assigns.</param>
            /// <param name="takenViewNames">View names of the document; the run adds the names it gives out.</param>
            /// <param name="ownedBySheet">Elements owned per sheet, keyed by sheet id.</param>
            /// <param name="failures">The failure lines of the run.</param>
            public RunState(
                SheetCopyOptions options,
                Dictionary<long, List<PlacedOnSheet>> placedViews,
                List<string> sheetNumbers,
                HashSet<string> takenViewNames,
                Dictionary<long, List<Element>> ownedBySheet,
                List<string> failures)
            {
                Options = options;
                PlacedViews = placedViews;
                SheetNumbers = sheetNumbers;
                TakenViewNames = takenViewNames;
                OwnedBySheet = ownedBySheet;
                Failures = failures;
            }

            /// <summary>Gets the switches of the run.</summary>
            public SheetCopyOptions Options { get; }

            /// <summary>Gets the views placed per sheet.</summary>
            public Dictionary<long, List<PlacedOnSheet>> PlacedViews { get; }

            /// <summary>Gets the sheet numbers the run knows about, and grows as it assigns more.</summary>
            public List<string> SheetNumbers { get; }

            /// <summary>Gets the view names the run knows about, and grows as it gives more out.</summary>
            public HashSet<string> TakenViewNames { get; }

            /// <summary>Gets the elements owned per sheet.</summary>
            public Dictionary<long, List<Element>> OwnedBySheet { get; }

            /// <summary>Gets the failure lines of the run.</summary>
            public List<string> Failures { get; }
        }
    }
}
