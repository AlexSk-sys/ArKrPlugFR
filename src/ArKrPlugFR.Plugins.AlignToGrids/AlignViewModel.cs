using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// The state of the options window: the picked columns, the picked grids, the capture
    /// distance and the run they start.
    /// <para>
    /// The capture is read from the user's settings folder when the window opens and
    /// written back when a run starts. Picking hides the window: the user needs the model
    /// in front of them.
    /// </para>
    /// </summary>
    public sealed class AlignViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "align-to-grids";

        /// <summary>Highest capture the field accepts, in millimetres.</summary>
        internal const int MaximumCaptureMillimeters = 100;

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<FamilyInstance> _columns = new List<FamilyInstance>();
        private readonly List<Grid> _grids = new List<Grid>();
        private string _captureText;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in, for picking.</param>
        /// <param name="document">Project whose columns are moved.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public AlignViewModel(
            UIDocument uiDocument,
            Document document,
            AlignText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            AlignSettings settings = ToolSettings.Load(
                AppPaths.SettingsDirectory,
                SettingsName,
                new AlignSettings());

            _captureText = settings.CaptureMillimeters.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public AlignText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the apply button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the button that picks columns.</summary>
        public string PickColumnsText => Text.PickColumns;

        /// <summary>Gets the caption of the button that picks grids.</summary>
        public string PickGridsText => Text.PickGrids;

        /// <summary>Gets the caption of the capture field.</summary>
        public string CaptureLabel => Text.CaptureLabel;

        /// <summary>Gets the hint under the capture field.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the picked columns.</summary>
        public string ColumnsLine => Text.ColumnsLine(_columns.Count);

        /// <summary>Gets the line that counts the picked grids.</summary>
        public string GridsLine => Text.GridsLine(_grids.Count);

        /// <summary>Gets the capture of the next run, in millimetres.</summary>
        internal Length Capture { get; private set; } = Length.FromMillimeters(5d);

        /// <summary>Gets or sets the capture as typed in the window.</summary>
        public string CaptureText
        {
            get => _captureText;

            set => SetProperty(ref _captureText, value ?? string.Empty);
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>
        /// Reads the capture from the field and stores it for the next session.
        /// </summary>
        /// <param name="error">The message to show when the field cannot be read.</param>
        /// <returns><c>true</c> when the field holds a usable capture.</returns>
        internal bool TryReadCapture(out string error)
        {
            error = string.Empty;

            if (int.TryParse(
                    _captureText.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int value)
                && value >= 1
                && value <= MaximumCaptureMillimeters)
            {
                Capture = Length.FromMillimeters(value);

                ToolSettings.Save(
                    AppPaths.SettingsDirectory,
                    SettingsName,
                    new AlignSettings { CaptureMillimeters = value });

                return true;
            }

            error = Text.CaptureRequired;

            return false;
        }

        /// <summary>Asks the user for the columns; the window has to be hidden while it runs.</summary>
        internal void PickColumns()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new ColumnSelectionFilter(),
                    Text.PickColumnsPrompt);

                _columns.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is FamilyInstance column)
                    {
                        _columns.Add(column);
                    }
                }

                RaisePropertyChanged(nameof(ColumnsLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("AlignToGrids: the pick of columns was cancelled.", nameof(AlignViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the grids; the window has to be hidden while it runs.</summary>
        internal void PickGrids()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new GridSelectionFilter(),
                    Text.PickGridsPrompt);

                _grids.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Grid grid)
                    {
                        _grids.Add(grid);
                    }
                }

                RaisePropertyChanged(nameof(GridsLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("AlignToGrids: the pick of grids was cancelled.", nameof(AlignViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Aligns the picked columns and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            AlignOutcome outcome = AlignEngine.Align(_document, _columns, _grids, Capture, Text, _log);

            string message = BuildReport(outcome);
            SetReport(message);

            return message;
        }

        /// <summary>Shows the given text in the report area of the window.</summary>
        /// <param name="report">Text to show.</param>
        public void SetReport(string report)
        {
            _report = report ?? string.Empty;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private string BuildReport(AlignOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.Result(
                    outcome.Moved,
                    outcome.Columns,
                    outcome.AlreadyAligned,
                    outcome.NoGrid,
                    outcome.InGroup,
                    outcome.Failed));
            }

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
                builder.AppendLine(Text.MoreLine(outcome.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>Lets the user pick columns of the two column categories.</summary>
        private sealed class ColumnSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is FamilyInstance
                    && (ElementScope.HasCategory(elem, BuiltInCategory.OST_StructuralColumns)
                        || ElementScope.HasCategory(elem, BuiltInCategory.OST_Columns));
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        /// <summary>Lets the user pick grids.</summary>
        private sealed class GridSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Grid;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
