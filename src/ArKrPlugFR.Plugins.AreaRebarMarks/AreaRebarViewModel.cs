using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.RebarMarks;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// The state of the tool window: the scope, the parameter, the switch and the run.
    /// <para>
    /// The settings are read from the user's settings folder when the window opens and
    /// written back whenever they change. The parameter list offers the names that carry a
    /// value on at least one source of the scope, so the run is never pointed at an empty
    /// parameter.
    /// </para>
    /// </summary>
    public sealed class AreaRebarViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "area-rebar-marks";

        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Element> _sources = new List<Element>();
        private readonly AreaRebarSettings _settings;
        private bool _wholeProject;
        private string? _parameterName;
        private bool _onlyEmpty;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the bars belong to.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public AreaRebarViewModel(UIDocument uiDocument, Document document, AreaRebarText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new AreaRebarSettings());
            _wholeProject = _settings.WholeProject;
            _onlyEmpty = _settings.OnlyEmpty;

            RefreshParameterNames();

            if (_parameterName == null && ParameterNames.Count > 0)
            {
                _parameterName = ParameterNames[0];
            }
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public AreaRebarText Text { get; }

        /// <summary>Gets the names of the parameters that carry a value on a source.</summary>
        public ObservableCollection<string> ParameterNames { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the switch that chooses the whole project.</summary>
        public string WholeProjectText => Text.WholeProject;

        /// <summary>Gets the caption of the button that picks the sources.</summary>
        public string PickSourcesText => Text.PickSources;

        /// <summary>Gets the caption of the parameter list.</summary>
        public string ParameterLabel => Text.ParameterLabel;

        /// <summary>Gets the caption of the switch that protects existing values.</summary>
        public string OnlyEmptyText => Text.OnlyEmpty;

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the picked sources.</summary>
        public string SourcesLine => Text.SourcesLine(_sources.Count);

        /// <summary>Gets or sets a value indicating whether the whole project is the scope.</summary>
        public bool WholeProject
        {
            get => _wholeProject;

            set
            {
                if (SetProperty(ref _wholeProject, value))
                {
                    SaveSettings();
                    RefreshParameterNames();
                }
            }
        }

        /// <summary>Gets or sets the chosen parameter.</summary>
        public string? ParameterName
        {
            get => _parameterName;

            set
            {
                if (SetProperty(ref _parameterName, value))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether a bar with a value is kept.</summary>
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

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the area reinforcement elements; the window has to be hidden while it runs.</summary>
        internal void PickSources()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new SourceFilter(),
                    Text.PickPrompt);

                _sources.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is AreaReinforcement source)
                    {
                        _sources.Add(source);
                    }
                }

                RaisePropertyChanged(nameof(SourcesLine));
                RefreshParameterNames();
            }
            catch (OperationCanceledException)
            {
                _log.Info("AreaRebarMarks: the pick of sources was cancelled.", nameof(AreaRebarViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Fills the bars and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            List<Element> sources = CurrentSources();

            if (sources.Count == 0)
            {
                SetReport(Text.NoSources);

                return Text.NoSources;
            }

            if (string.IsNullOrWhiteSpace(_parameterName))
            {
                SetReport(Text.ParameterRequired);

                return Text.ParameterRequired;
            }

            _settings.ParameterName = _parameterName!;
            SaveSettings();

            AreaRebarOutcome outcome = AreaRebarEngine.Run(_document, sources, _settings, Text, _log);

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

        private List<Element> CurrentSources()
        {
            if (!_wholeProject)
            {
                return new List<Element>(_sources);
            }

            var sources = new List<Element>();

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(AreaReinforcement)))
            {
                if (element is AreaReinforcement source)
                {
                    sources.Add(source);
                }
            }

            return sources;
        }

        private void RefreshParameterNames()
        {
            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Element source in CurrentSources())
            {
                foreach (KeyValuePair<string, string?> pair in ParameterSupport.ReadValues(source))
                {
                    if (!string.IsNullOrWhiteSpace(pair.Value) && seen.Add(pair.Key))
                    {
                        names.Add(pair.Key);
                    }
                }
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);

            ParameterNames.Clear();

            foreach (string name in names)
            {
                ParameterNames.Add(name);
            }

            if (_parameterName != null && !seen.Contains(_parameterName))
            {
                ParameterName = ParameterNames.Count > 0 ? ParameterNames[0] : null;
            }
        }

        private void SaveSettings()
        {
            _settings.WholeProject = _wholeProject;
            _settings.OnlyEmpty = _onlyEmpty;

            if (_parameterName != null)
            {
                _settings.ParameterName = _parameterName;
            }

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private string BuildReport(AreaRebarOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.Result(
                    outcome.Sources,
                    outcome.NoValue,
                    outcome.NoBars,
                    outcome.Filled,
                    outcome.Kept,
                    outcome.Refused));
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

        /// <summary>Lets the user pick area reinforcement and nothing else.</summary>
        private sealed class SourceFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is AreaReinforcement;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
