using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// The state of the tool window: the scope of the walls, the load case and the load
    /// values.
    /// <para>
    /// The settings are read from the user's settings folder when the window opens and
    /// written back whenever they change, so the next run starts where this one left off.
    /// The walls are resolved at the moment of the run: a selection can change while the
    /// window is open, and the view can be switched.
    /// </para>
    /// </summary>
    public sealed class LoadViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "wall-line-loads";

        /// <summary>Settings names of the three scopes, in the order of the list.</summary>
        private static readonly string[] ScopeKeys = { "selected", "view", "workset" };

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly LoadSettings _settings;
        private readonly List<string> _worksets;
        private int _scopeIndex;
        private int _worksetIndex = -1;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">Document the selection is read from.</param>
        /// <param name="document">Project to edit.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public LoadViewModel(
            UIDocument uiDocument,
            Document document,
            LoadText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new LoadSettings());
            _scopeIndex = IndexOfScope(_settings.Scope);

            _worksets = WorksetSupport.IsWorkshared(_document)
                ? WorksetSupport.ReadUserWorksetMap(_document).Keys.OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase).ToList()
                : new List<string>();

            _worksetIndex = _worksets.IndexOf(_settings.WorksetName);
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public LoadText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the scope.</summary>
        public string ScopeLabel => Text.ScopeLabel;

        /// <summary>Gets the three scopes, in the order of the list.</summary>
        public string[] Scopes => new[] { Text.ScopeSelected, Text.ScopeView, Text.ScopeWorkset };

        /// <summary>Gets the caption of the workset list.</summary>
        public string WorksetLabel => Text.WorksetLabel;

        /// <summary>Gets the names of the worksets of the project.</summary>
        public string[] WorksetNames => _worksets.ToArray();

        /// <summary>Gets a value indicating whether the project uses worksets.</summary>
        public bool WorksetsEnabled => _worksets.Count > 0;

        /// <summary>Gets the caption of the load case field.</summary>
        public string CaseLabel => Text.CaseLabel;

        /// <summary>Gets the hint about a load case that does not exist yet.</summary>
        public string CaseHint => Text.CaseHint;

        /// <summary>Gets the caption of the force field.</summary>
        public string ForceLabel => Text.ForceLabel;

        /// <summary>Gets the caption of the moment field.</summary>
        public string MomentLabel => Text.MomentLabel;

        /// <summary>Gets the caption of the uniformity switch.</summary>
        public string UniformLabel => Text.UniformLabel;

        /// <summary>Gets the caption of the comment field.</summary>
        public string CommentLabel => Text.CommentLabel;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets the chosen scope.</summary>
        public int ScopeIndex
        {
            get => _scopeIndex;

            set
            {
                if (SetProperty(ref _scopeIndex, value) && value >= 0 && value < ScopeKeys.Length)
                {
                    _settings.Scope = ScopeKeys[value];
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the chosen workset.</summary>
        public int WorksetIndex
        {
            get => _worksetIndex;

            set
            {
                if (SetProperty(ref _worksetIndex, value) && value >= 0 && value < _worksets.Count)
                {
                    _settings.WorksetName = _worksets[value];
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the name of the load case.</summary>
        public string CaseName
        {
            get => _settings.CaseName;

            set
            {
                string wanted = value ?? string.Empty;

                if (_settings.CaseName != wanted)
                {
                    _settings.CaseName = wanted;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the force per metre as typed.</summary>
        public string ForceText
        {
            get => LoadText.Number(_settings.Force);

            set
            {
                if (TryParse(value, out double force) && _settings.Force != force)
                {
                    _settings.Force = force;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the moment per metre as typed.</summary>
        public string MomentText
        {
            get => LoadText.Number(_settings.Moment);

            set
            {
                if (TryParse(value, out double moment) && _settings.Moment != moment)
                {
                    _settings.Moment = moment;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether the load is uniform along the wall.</summary>
        public bool Uniform
        {
            get => _settings.Uniform;

            set
            {
                if (_settings.Uniform != value)
                {
                    _settings.Uniform = value;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the comment written into every created load.</summary>
        public string Comment
        {
            get => _settings.Comment;

            set
            {
                string wanted = value ?? string.Empty;

                if (_settings.Comment != wanted)
                {
                    _settings.Comment = wanted;
                    RaisePropertyChanged();
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Creates the loads and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (!TryResolveWalls(out IReadOnlyList<Wall> walls, out string? problem))
            {
                SetReport(problem ?? Text.NoWalls);

                return _report;
            }

            LoadOutcome outcome = WallLineLoadsEngine.Run(_document, walls, _settings, Text, _log);

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

        private bool TryResolveWalls(out IReadOnlyList<Wall> walls, out string? problem)
        {
            walls = Array.Empty<Wall>();
            problem = null;

            switch (ScopeKeys[_scopeIndex])
            {
                case "selected":
                    walls = ElementScope.Resolve(_document, _uiDocument.Selection.GetElementIds())
                        .OfType<Wall>()
                        .ToList();

                    return true;

                case "workset":
                    if (_worksetIndex < 0 || _worksetIndex >= _worksets.Count)
                    {
                        problem = Text.NoWorkset(_settings.WorksetName);

                        return false;
                    }

                    if (!WorksetSupport.IsWorkshared(_document))
                    {
                        problem = Text.NotAWorksharedModel;

                        return false;
                    }

                    Dictionary<string, WorksetId> map = WorksetSupport.ReadUserWorksetMap(_document);

                    if (!map.TryGetValue(_worksets[_worksetIndex], out WorksetId worksetId))
                    {
                        problem = Text.NoWorkset(_worksets[_worksetIndex]);

                        return false;
                    }

                    walls = new FilteredElementCollector(_document)
                        .OfClass(typeof(Wall))
                        .Cast<Wall>()
                        .Where(wall => wall.WorksetId == worksetId)
                        .OrderBy(wall => ArKrPlugFR.Revit.Compatibility.ElementIdCompat.GetValue(wall.Id))
                        .ToList();

                    return true;

                default:
                    walls = new FilteredElementCollector(_document, _document.ActiveView.Id)
                        .OfClass(typeof(Wall))
                        .Cast<Wall>()
                        .OrderBy(wall => ArKrPlugFR.Revit.Compatibility.ElementIdCompat.GetValue(wall.Id))
                        .ToList();

                    return true;
            }
        }

        private string BuildReport(LoadOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.Result(
                    outcome.Created,
                    outcome.Walls,
                    outcome.Skipped,
                    outcome.Refused,
                    _settings.CaseName,
                    _settings.Force,
                    _settings.Moment));

                if (outcome.CaseCreated)
                {
                    builder.AppendLine(Text.CaseCreated(_settings.CaseName));
                }

                if (outcome.HostedStayed > 0)
                {
                    builder.AppendLine(Text.HostedNote);
                }
            }

            foreach (string reason in outcome.Reasons)
            {
                builder.Append(" - ");
                builder.AppendLine(reason);
            }

            return builder.ToString().TrimEnd();
        }

        private static int IndexOfScope(string? scope)
        {
            for (int index = 0; index < ScopeKeys.Length; index++)
            {
                if (string.Equals(ScopeKeys[index], scope, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 1;
        }

        private static bool TryParse(string? value, out double number)
        {
            number = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out number)
                || double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }

        private void SaveSettings()
        {
            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }
    }
}
