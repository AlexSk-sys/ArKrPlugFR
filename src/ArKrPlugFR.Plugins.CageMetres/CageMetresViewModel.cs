using System;
using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// The state of the tool window: the picked lines and cage, the parameter names and the
    /// run that measures the cage.
    /// <para>
    /// The parameter names and the write switch are read from the user's settings folder when
    /// the window opens and written back whenever they change, so the next run starts where
    /// this one left off. The window only shows the state; the model is read and written by
    /// the engine.
    /// </para>
    /// </summary>
    public sealed class CageMetresViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "cage-metres";

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly Log _log;
        private readonly CageMetresSettings _settings;
        private readonly List<Curve> _lines = new List<Curve>();
        private FamilyInstance? _cage;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">Document the picks happen in.</param>
        /// <param name="document">Project the cage lives in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public CageMetresViewModel(
            UIDocument uiDocument,
            Document document,
            CageMetresText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new CageMetresSettings());
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public CageMetresText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the layout lines.</summary>
        public string LinesLabel => Text.LinesLabel;

        /// <summary>Gets the caption of the cage.</summary>
        public string CageLabel => Text.CageLabel;

        /// <summary>Gets the caption of the button that picks the lines.</summary>
        public string PickLinesText => Text.PickLines;

        /// <summary>Gets the caption of the button that picks the cage.</summary>
        public string PickCageText => Text.PickCage;

        /// <summary>Gets the hint about the parameter names.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the calculated-length parameter field.</summary>
        public string ResultParameterLabel => Text.ResultParameterLabel;

        /// <summary>Gets the caption of the write switch.</summary>
        public string WriteSwitchText => Text.WriteSwitch;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the state of the picked lines.</summary>
        public string LinesState
        {
            get
            {
                if (_lines.Count == 0)
                {
                    return Text.LinesNone;
                }

                double feet = 0;

                foreach (Curve line in _lines)
                {
                    feet += line.Length;
                }

                return Text.LinesLine(_lines.Count, ArKrPlugFR.Core.Units.Length.FromFeet(feet).Meters);
            }
        }

        /// <summary>Gets the state of the picked cage.</summary>
        public string CageState =>
            _cage == null ? Text.CageNone : Text.CageLine(_cage.Symbol?.Family?.Name ?? _cage.Name ?? string.Empty);

        /// <summary>Gets or sets the parameter the calculated length is written into.</summary>
        public string ResultParameter
        {
            get => _settings.ResultParameter;

            set
            {
                string wanted = value ?? string.Empty;

                if (_settings.ResultParameter != wanted)
                {
                    _settings.ResultParameter = wanted;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether the calculated length is written into the cage.</summary>
        public bool WriteResult
        {
            get => _settings.WriteResult;

            set
            {
                if (_settings.WriteResult != value)
                {
                    _settings.WriteResult = value;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the layout lines; the window has to be hidden while it runs.</summary>
        internal void PickLines()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new LineFilter(),
                    Text.PickLinesPrompt);

                _lines.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is CurveElement curve && curve.GeometryCurve != null)
                    {
                        _lines.Add(curve.GeometryCurve);
                    }
                }

                RaisePropertyChanged(nameof(LinesState));
            }
            catch (OperationCanceledException)
            {
                _log.Info("CageMetres: the pick of lines was cancelled.", nameof(CageMetresViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the cage; the window has to be hidden while it runs.</summary>
        internal void PickCage()
        {
            try
            {
                Reference reference = _uiDocument.Selection.PickObject(
                    ObjectType.Element,
                    new CageFilter(),
                    Text.PickCagePrompt);

                _cage = _document.GetElement(reference) as FamilyInstance;

                RaisePropertyChanged(nameof(CageState));
            }
            catch (OperationCanceledException)
            {
                _log.Info("CageMetres: the pick of the cage was cancelled.", nameof(CageMetresViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Measures the cage and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_lines.Count == 0)
            {
                SetReport(Text.NoLines);

                return Text.NoLines;
            }

            if (_cage == null)
            {
                SetReport(Text.NoCage);

                return Text.NoCage;
            }

            CageOutcome outcome = CageMetresEngine.Run(_document, _lines, _cage, _settings, Text, _log);

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

        private string BuildReport(CageOutcome outcome)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Text.CageLine(outcome.Family));
            builder.AppendLine(Text.LinesLine(outcome.LineCount, outcome.LineMetres));
            builder.AppendLine(Text.LongitudinalLine(
                outcome.LongitudinalBars, outcome.LineMetres, outcome.LongitudinalMetres));
            builder.AppendLine(Text.InclinedLine(
                outcome.InclinedPositions, outcome.InclinedBarMetres, outcome.InclinedMetres));
            builder.AppendLine(Text.TransverseLine(
                outcome.TransversePositions, outcome.TransverseBarMetres, outcome.TransverseMetres));
            builder.AppendLine(Text.TotalLine(outcome.TotalMetres));

            if (outcome.WriteNote != null)
            {
                builder.AppendLine(outcome.WriteNote);
            }

            if (outcome.Warnings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(Text.WarningsCaption);

                foreach (string warning in outcome.Warnings)
                {
                    builder.Append(" - ");
                    builder.AppendLine(warning);
                }
            }

            return builder.ToString().TrimEnd();
        }

        private void SaveSettings()
        {
            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private sealed class LineFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => element is CurveElement;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private sealed class CageFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => element is FamilyInstance;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
