using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Piles;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// The state of the tool window: the picked piles and slabs, the settings and the three
    /// actions.
    /// <para>
    /// The settings are read from the user's settings folder when the window opens and
    /// written back whenever they change, so the next session starts where this one left
    /// off.
    /// </para>
    /// </summary>
    public sealed class PileViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "pile-data";

        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Element> _piles = new List<Element>();
        private readonly List<Element> _slabs = new List<Element>();
        private readonly PileSettings _settings;
        private string _startText;
        private bool _topDown;
        private string _numberParameter;
        private string _cutParameter;
        private string _slabParameter;
        private string _pileParameter;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the piles belong to.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public PileViewModel(UIDocument uiDocument, Document document, PileText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new PileSettings());

            _startText = _settings.StartNumber.ToString(CultureInfo.InvariantCulture);
            _topDown = _settings.TopDown;
            _numberParameter = _settings.NumberParameter;
            _cutParameter = _settings.CutHeightParameter;
            _slabParameter = _settings.SlabBottomParameter;
            _pileParameter = _settings.PileBottomParameter;
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public PileText Text { get; }

        /// <summary>Gets the picked piles, described for the list.</summary>
        public ObservableCollection<string> PileRows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the picked slabs, described for the list.</summary>
        public ObservableCollection<string> SlabRows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the button that picks the piles.</summary>
        public string PickPilesText => Text.PickPiles;

        /// <summary>Gets the caption of the button that picks the slabs.</summary>
        public string PickSlabsText => Text.PickSlabs;

        /// <summary>Gets the caption of the pile list.</summary>
        public string PilesCaption => Text.PilesCaption;

        /// <summary>Gets the caption of the slab list.</summary>
        public string SlabsCaption => Text.SlabsCaption;

        /// <summary>Gets the caption of the start number field.</summary>
        public string StartLabel => Text.StartLabel;

        /// <summary>Gets the caption of the direction switch.</summary>
        public string TopDownText => Text.TopDown;

        /// <summary>Gets the caption of the number parameter field.</summary>
        public string NumberParameterLabel => Text.NumberParameterLabel;

        /// <summary>Gets the caption of the cut height parameter field.</summary>
        public string CutParameterLabel => Text.CutParameterLabel;

        /// <summary>Gets the caption of the raft bottom parameter field.</summary>
        public string SlabParameterLabel => Text.SlabParameterLabel;

        /// <summary>Gets the caption of the pile bottom parameter field.</summary>
        public string PileParameterLabel => Text.PileParameterLabel;

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the numbering button.</summary>
        public string NumberButtonText => Text.NumberButton;

        /// <summary>Gets the caption of the elevation button.</summary>
        public string ElevationsButtonText => Text.ElevationsButton;

        /// <summary>Gets the caption of the cut height button.</summary>
        public string CutButtonText => Text.CutButton;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the picked piles.</summary>
        public string PilesLine => Text.PilesLine(_piles.Count);

        /// <summary>Gets the line that counts the picked slabs.</summary>
        public string SlabsLine => Text.SlabsLine(_slabs.Count);

        /// <summary>Gets or sets the start number as typed in the window.</summary>
        public string StartText
        {
            get => _startText;

            set => SetProperty(ref _startText, value ?? string.Empty);
        }

        /// <summary>Gets or sets a value indicating whether the numbering runs top-down.</summary>
        public bool TopDown
        {
            get => _topDown;

            set
            {
                if (SetProperty(ref _topDown, value))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the parameter that receives the number.</summary>
        public string NumberParameter
        {
            get => _numberParameter;

            set
            {
                if (SetProperty(ref _numberParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the parameter that receives the cut height.</summary>
        public string CutParameter
        {
            get => _cutParameter;

            set
            {
                if (SetProperty(ref _cutParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the parameter that receives the raft bottom.</summary>
        public string SlabParameter
        {
            get => _slabParameter;

            set
            {
                if (SetProperty(ref _slabParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the parameter that receives the pile bottom.</summary>
        public string PileParameter
        {
            get => _pileParameter;

            set
            {
                if (SetProperty(ref _pileParameter, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last action, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the piles; the window has to be hidden while it runs.</summary>
        internal void PickPiles()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new PileFilter(),
                    Text.PickPilesPrompt);

                _piles.Clear();
                PileRows.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is FamilyInstance pile)
                    {
                        _piles.Add(pile);
                        PileRows.Add(ElementDescription.Describe(pile));
                    }
                }

                RaisePropertyChanged(nameof(PilesLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("PileData: the pick of piles was cancelled.", nameof(PileViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the slabs; the window has to be hidden while it runs.</summary>
        internal void PickSlabs()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new SlabFilter(),
                    Text.PickSlabsPrompt);

                _slabs.Clear();
                SlabRows.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Element slab)
                    {
                        _slabs.Add(slab);
                        SlabRows.Add(ElementDescription.Describe(slab));
                    }
                }

                RaisePropertyChanged(nameof(SlabsLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("PileData: the pick of slabs was cancelled.", nameof(PileViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Numbers the picked piles and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyNumber()
        {
            if (_piles.Count == 0)
            {
                SetReport(Text.NoPiles);

                return Text.NoPiles;
            }

            if (!TryReadStart(out string error))
            {
                return error;
            }

            PileDataOutcome outcome = PileDataEngine.Number(_document, _piles, _settings, Text, _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.NumberResult(outcome.Considered, outcome.Written, outcome.Refused));

            SetReport(message);

            return message;
        }

        /// <summary>Writes the elevations and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyElevations()
        {
            PileDataOutcome outcome = PileDataEngine.Elevations(_document, _piles, _slabs, _settings, Text, _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.ElevationResult(outcome.Considered, outcome.Written, outcome.Skipped, outcome.Refused));

            SetReport(message);

            return message;
        }

        /// <summary>Writes the cut heights and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyCut()
        {
            PileDataOutcome outcome = PileDataEngine.CutHeights(_document, _piles, _slabs, _settings, Text, _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.CutResult(outcome.Considered, outcome.Written, outcome.Skipped, outcome.Refused));

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

        private bool TryReadStart(out string error)
        {
            error = string.Empty;

            if (int.TryParse(_startText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= 1)
            {
                _settings.StartNumber = value;
                SaveSettings();

                return true;
            }

            error = Text.StartRequired;
            SetReport(error);

            return false;
        }

        private void SaveSettings()
        {
            if (int.TryParse(_startText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= 1)
            {
                _settings.StartNumber = value;
            }

            _settings.TopDown = _topDown;
            _settings.NumberParameter = _numberParameter;
            _settings.CutHeightParameter = _cutParameter;
            _settings.SlabBottomParameter = _slabParameter;
            _settings.PileBottomParameter = _pileParameter;

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private string BuildReport(PileDataOutcome outcome, string? resultLine)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else if (resultLine != null)
            {
                builder.AppendLine(resultLine);
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

        /// <summary>Lets the user pick pile families and nothing else.</summary>
        private sealed class PileFilter : ISelectionFilter
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

        /// <summary>Lets the user pick floors and families as rafts.</summary>
        private sealed class SlabFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Floor || elem is FamilyInstance;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
