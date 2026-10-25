using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Core.Worksets;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// The state of the tool window: the ordered rules, the switch that allows creating
    /// worksets, and the run they start.
    /// <para>
    /// The rules are read from the user's settings folder when the window opens and
    /// written back whenever they change, so the next run starts where this one left
    /// off. The decision itself is made by <see cref="WorksetRules"/> and carried out by
    /// <see cref="WorksetEngine"/>.
    /// </para>
    /// </summary>
    public sealed class WorksetViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "assign-worksets";

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _document;
        private readonly Log _log;
        private readonly WorksetSettings _settings;
        private bool _createMissingWorksets;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project to distribute.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public WorksetViewModel(Document document, WorksetText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new WorksetSettings());
            _createMissingWorksets = _settings.CreateMissingWorksets;

            Rules = new ObservableCollection<WorksetRow>();

            foreach (WorksetRule rule in _settings.Rules ?? new List<WorksetRule>())
            {
                AddRow(rule);
            }

            if (Rules.Count == 0)
            {
                AddRule();
            }
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public WorksetText Text { get; }

        /// <summary>Gets the rules, in the order they are tried.</summary>
        public ObservableCollection<WorksetRow> Rules { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the rule list.</summary>
        public string RulesCaption => Text.RulesCaption;

        /// <summary>Gets the caption of the kind column.</summary>
        public string KindCaption => Text.KindCaption;

        /// <summary>Gets the caption of the match column.</summary>
        public string PatternCaption => Text.PatternCaption;

        /// <summary>Gets the caption of the workset column.</summary>
        public string TargetCaption => Text.TargetCaption;

        /// <summary>Gets the hint that explains what each kind matches.</summary>
        public string KindHint => Text.KindHint;

        /// <summary>Gets the caption of the button that adds a rule.</summary>
        public string AddRuleText => Text.AddRule;

        /// <summary>Gets the caption of the switch that allows creating missing worksets.</summary>
        public string CreateMissingText => Text.CreateMissing;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets a value indicating whether a run may create worksets that do not exist yet.</summary>
        public bool CreateMissingWorksets
        {
            get => _createMissingWorksets;

            set
            {
                if (SetProperty(ref _createMissingWorksets, value))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Adds an empty rule at the end of the list.</summary>
        public void AddRule()
        {
            AddRow(null);
            SaveSettings();
        }

        /// <summary>Removes a rule from the list.</summary>
        /// <param name="row">Row to remove.</param>
        public void RemoveRule(WorksetRow? row)
        {
            if (row == null || !Rules.Remove(row))
            {
                return;
            }

            row.Changed -= OnRowChanged;

            SaveSettings();
        }

        /// <summary>Runs the assignment and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (!_document.IsWorkshared)
            {
                SetReport(Text.NotWorkshared);

                return Text.NotWorkshared;
            }

            var rules = new List<WorksetRule>();

            foreach (WorksetRow row in Rules)
            {
                WorksetRule rule = row.ToRule();

                if (rule.IsUsable)
                {
                    rules.Add(rule);
                }
            }

            if (rules.Count == 0)
            {
                SetReport(Text.NoRules);

                return Text.NoRules;
            }

            SaveSettings();

            WorksetOutcome outcome = WorksetEngine.Run(_document, rules, _createMissingWorksets, Text, _log);

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

        internal void SaveSettings()
        {
            _settings.CreateMissingWorksets = _createMissingWorksets;
            _settings.Rules = new List<WorksetRule>();

            foreach (WorksetRow row in Rules)
            {
                _settings.Rules.Add(row.ToRule());
            }

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private void AddRow(WorksetRule? rule)
        {
            var row = new WorksetRow(Text, rule);

            row.Changed += OnRowChanged;
            Rules.Add(row);
        }

        private void OnRowChanged()
        {
            SaveSettings();
        }

        private string BuildReport(WorksetOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }

            if (outcome.Considered > 0)
            {
                builder.AppendLine(Text.Result(
                    outcome.Considered,
                    outcome.Assigned,
                    outcome.AlreadyInTarget,
                    outcome.NoMatch,
                    outcome.WorksetMissing,
                    outcome.Failed));
            }

            if (outcome.CreatedWorksets > 0)
            {
                builder.AppendLine(Text.CreatedLine(outcome.CreatedWorksets));
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

            if (outcome.Failed > shown)
            {
                builder.AppendLine(Text.MoreLine(outcome.Failed - shown));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
