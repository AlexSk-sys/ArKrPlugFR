using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Couplers;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.CouplerMarks
{
    /// <summary>
    /// The state of the tool window: the picked couplers, the tolerance, the two parameter
    /// names and the run they start.
    /// <para>
    /// The settings are read from the user's settings folder when the window opens and
    /// written back whenever they change, so the next run starts where this one left off.
    /// </para>
    /// </summary>
    public sealed class CouplerViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "coupler-marks";

        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Element> _couplers = new List<Element>();
        private readonly CouplerSettings _settings;
        private string _toleranceText;
        private string _markParameter;
        private string _categoryParameter;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the couplers belong to.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public CouplerViewModel(UIDocument uiDocument, Document document, CouplerText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new CouplerSettings());
            _toleranceText = _settings.ToleranceMillimeters.ToString(CultureInfo.InvariantCulture);
            _markParameter = _settings.MarkParameter;
            _categoryParameter = _settings.CategoryParameter;
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public CouplerText Text { get; }

        /// <summary>Gets the picked couplers, described for the list.</summary>
        public ObservableCollection<string> CouplerRows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the button that picks the couplers.</summary>
        public string PickCouplersText => Text.PickCouplers;

        /// <summary>Gets the caption of the picked coupler list.</summary>
        public string CouplersCaption => Text.CouplersCaption;

        /// <summary>Gets the caption of the tolerance field.</summary>
        public string ToleranceLabel => Text.ToleranceLabel;

        /// <summary>Gets the hint under the tolerance field.</summary>
        public string ToleranceHint => Text.ToleranceHint;

        /// <summary>Gets the caption of the field that names the mark parameter.</summary>
        public string MarkParameterLabel => Text.MarkParameterLabel;

        /// <summary>Gets the caption of the field that names the category parameter.</summary>
        public string CategoryParameterLabel => Text.CategoryParameterLabel;

        /// <summary>Gets the hint about how the two values are written.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the picked couplers.</summary>
        public string CouplersLine => Text.CouplersLine(_couplers.Count);

        /// <summary>Gets or sets the tolerance as typed in the window.</summary>
        public string ToleranceText
        {
            get => _toleranceText;

            set => SetProperty(ref _toleranceText, value ?? string.Empty);
        }

        /// <summary>Gets or sets the name of the parameter that receives the mark of the host.</summary>
        public string MarkParameter
        {
            get => _markParameter;

            set
            {
                if (SetProperty(ref _markParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the parameter that receives the category of the host.</summary>
        public string CategoryParameter
        {
            get => _categoryParameter;

            set
            {
                if (SetProperty(ref _categoryParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the couplers; the window has to be hidden while it runs.</summary>
        internal void PickCouplers()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new CouplerFilter(),
                    Text.PickPrompt);

                _couplers.Clear();
                CouplerRows.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is FamilyInstance instance)
                    {
                        _couplers.Add(instance);
                        CouplerRows.Add(ElementDescription.Describe(instance));
                    }
                }

                RaisePropertyChanged(nameof(CouplersLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("CouplerMarks: the pick of couplers was cancelled.", nameof(CouplerViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Marks the picked couplers and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_couplers.Count == 0)
            {
                SetReport(Text.NoCouplers);

                return Text.NoCouplers;
            }

            if (!TryReadTolerance(out string error))
            {
                return error;
            }

            CouplerMarkOutcome outcome = CouplerMarkEngine.Run(_document, _couplers, _settings, Text, _log);

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

        private bool TryReadTolerance(out string error)
        {
            error = string.Empty;

            if (double.TryParse(
                    _toleranceText.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out double value)
                && CouplerRules.IsUsableTolerance(value))
            {
                _settings.ToleranceMillimeters = value;
                SaveSettings();

                return true;
            }

            error = Text.ToleranceRequired;
            SetReport(error);

            return false;
        }

        private void SaveSettings()
        {
            if (double.TryParse(
                    _toleranceText.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out double tolerance)
                && CouplerRules.IsUsableTolerance(tolerance))
            {
                _settings.ToleranceMillimeters = tolerance;
            }

            _settings.MarkParameter = _markParameter;
            _settings.CategoryParameter = _categoryParameter;

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private string BuildReport(CouplerMarkOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.Result(
                    outcome.Considered,
                    outcome.Updated,
                    outcome.NoBarNearby,
                    outcome.NoHostData,
                    outcome.Failed));
            }

            int shown = 0;

            foreach (string note in outcome.Notes)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(note);
                shown++;
            }

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

            int total = outcome.Notes.Count + outcome.Failures.Count;

            if (total > shown)
            {
                builder.AppendLine(Text.MoreLine(total - shown));
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>Lets the user pick couplers — family instances — and nothing else.</summary>
        private sealed class CouplerFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is FamilyInstance;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
