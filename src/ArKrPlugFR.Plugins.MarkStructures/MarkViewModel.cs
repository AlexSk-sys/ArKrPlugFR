using System;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Marking;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// The state of the tool window: the codes, the suffixes, the switch that protects
    /// existing marks and the run they start.
    /// <para>
    /// The settings are read from the user's settings folder when the window opens and
    /// written back whenever they change, so the next run starts where this one left off.
    /// </para>
    /// </summary>
    public sealed class MarkViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "mark-structures";

        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _document;
        private readonly Log _log;
        private readonly MarkingSettings _settings;
        private bool _onlyEmpty;
        private string _columnMark;
        private string _beamMark;
        private string _floorMark;
        private string _wallMark;
        private string _foundationMark;
        private string _pileMark;
        private string _concreteSuffix;
        private string _steelSuffix;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project to mark.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public MarkViewModel(Document document, MarkText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new MarkingSettings());

            _onlyEmpty = _settings.OnlyEmpty;
            _columnMark = _settings.ColumnMark;
            _beamMark = _settings.BeamMark;
            _floorMark = _settings.FloorMark;
            _wallMark = _settings.WallMark;
            _foundationMark = _settings.FoundationMark;
            _pileMark = _settings.PileMark;
            _concreteSuffix = _settings.ConcreteSuffix;
            _steelSuffix = _settings.SteelSuffix;
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public MarkText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the switch that protects existing marks.</summary>
        public string OnlyEmptyText => Text.OnlyEmpty;

        /// <summary>Gets the caption of the mark code section.</summary>
        public string MarksCaption => Text.MarksCaption;

        /// <summary>Gets the caption of the material suffix section.</summary>
        public string SuffixesCaption => Text.SuffixesCaption;

        /// <summary>Gets the caption of a column.</summary>
        public string ColumnLabel => Text.LabelOf(StructureKind.Column);

        /// <summary>Gets the caption of a beam.</summary>
        public string BeamLabel => Text.LabelOf(StructureKind.Beam);

        /// <summary>Gets the caption of a floor.</summary>
        public string FloorLabel => Text.LabelOf(StructureKind.Floor);

        /// <summary>Gets the caption of a wall.</summary>
        public string WallLabel => Text.LabelOf(StructureKind.Wall);

        /// <summary>Gets the caption of a foundation.</summary>
        public string FoundationLabel => Text.LabelOf(StructureKind.Foundation);

        /// <summary>Gets the caption of a pile.</summary>
        public string PileLabel => Text.LabelOf(StructureKind.Pile);

        /// <summary>Gets the caption of the concrete suffix field.</summary>
        public string ConcreteSuffixLabel => Text.ConcreteSuffixLabel;

        /// <summary>Gets the caption of the steel suffix field.</summary>
        public string SteelSuffixLabel => Text.SteelSuffixLabel;

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets a value indicating whether an element with a mark is left alone.</summary>
        public bool OnlyEmpty
        {
            get => _onlyEmpty;

            set
            {
                if (SetProperty(ref _onlyEmpty, value))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a column.</summary>
        public string ColumnMark
        {
            get => _columnMark;

            set
            {
                if (SetProperty(ref _columnMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a beam.</summary>
        public string BeamMark
        {
            get => _beamMark;

            set
            {
                if (SetProperty(ref _beamMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a floor.</summary>
        public string FloorMark
        {
            get => _floorMark;

            set
            {
                if (SetProperty(ref _floorMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a wall.</summary>
        public string WallMark
        {
            get => _wallMark;

            set
            {
                if (SetProperty(ref _wallMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a foundation.</summary>
        public string FoundationMark
        {
            get => _foundationMark;

            set
            {
                if (SetProperty(ref _foundationMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the code of a pile.</summary>
        public string PileMark
        {
            get => _pileMark;

            set
            {
                if (SetProperty(ref _pileMark, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the suffix appended to the mark of a concrete element.</summary>
        public string ConcreteSuffix
        {
            get => _concreteSuffix;

            set
            {
                if (SetProperty(ref _concreteSuffix, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the suffix appended to the mark of a steel element.</summary>
        public string SteelSuffix
        {
            get => _steelSuffix;

            set
            {
                if (SetProperty(ref _steelSuffix, value ?? string.Empty))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Marks the structures and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (!HasAnyCode())
            {
                SetReport(Text.MarksRequired);

                return Text.MarksRequired;
            }

            StructureMarkOutcome outcome = StructureMarkEngine.Run(_document, _settings, Text, _log);

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

        private bool HasAnyCode()
        {
            return !string.IsNullOrWhiteSpace(_columnMark)
                || !string.IsNullOrWhiteSpace(_beamMark)
                || !string.IsNullOrWhiteSpace(_floorMark)
                || !string.IsNullOrWhiteSpace(_wallMark)
                || !string.IsNullOrWhiteSpace(_foundationMark)
                || !string.IsNullOrWhiteSpace(_pileMark);
        }

        private void SaveSettings()
        {
            _settings.OnlyEmpty = _onlyEmpty;
            _settings.ColumnMark = _columnMark;
            _settings.BeamMark = _beamMark;
            _settings.FloorMark = _floorMark;
            _settings.WallMark = _wallMark;
            _settings.FoundationMark = _foundationMark;
            _settings.PileMark = _pileMark;
            _settings.ConcreteSuffix = _concreteSuffix;
            _settings.SteelSuffix = _steelSuffix;

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private string BuildReport(StructureMarkOutcome outcome)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Text.Result(
                outcome.Considered,
                outcome.Marked,
                outcome.AlreadyMarked,
                outcome.NoKind,
                outcome.NoMarkCode,
                outcome.Failed));

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
    }
}
