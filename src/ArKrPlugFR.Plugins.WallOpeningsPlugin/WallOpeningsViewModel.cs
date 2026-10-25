using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// The state of the options window: the two switches, the walls the command found in
    /// the selection, and the run they start.
    /// <para>
    /// The switches are read from the user's settings folder when the window opens and
    /// written back whenever one of them changes, so the next run starts where this one
    /// left off.
    /// </para>
    /// </summary>
    public sealed class WallOpeningsViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "wall-openings";

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly Document _document;
        private readonly IReadOnlyList<Wall> _walls;
        private readonly Log _log;
        private bool _doors;
        private bool _windows;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project the openings are cut in.</param>
        /// <param name="walls">Walls the user selected.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public WallOpeningsViewModel(
            Document document,
            IReadOnlyList<Wall> walls,
            WallOpeningsText text,
            Log log)
        {
            _document = document ?? throw new System.ArgumentNullException(nameof(document));
            _walls = walls ?? throw new System.ArgumentNullException(nameof(walls));
            Text = text ?? throw new System.ArgumentNullException(nameof(text));
            _log = log ?? throw new System.ArgumentNullException(nameof(log));

            WallOpeningsSettings settings = ToolSettings.Load(
                AppPaths.SettingsDirectory,
                SettingsName,
                new WallOpeningsSettings());

            _doors = settings.Doors;
            _windows = settings.Windows;
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public WallOpeningsText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the line that names the walls in the selection.</summary>
        public string SelectionLine => Text.SelectionLine(_walls.Count);

        /// <summary>Gets the caption of the apply button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets a value indicating whether doors are processed.</summary>
        public bool Doors
        {
            get => _doors;

            set
            {
                if (SetProperty(ref _doors, value))
                {
                    Remember();
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether windows are processed.</summary>
        public bool Windows
        {
            get => _windows;

            set
            {
                if (SetProperty(ref _windows, value))
                {
                    Remember();
                }
            }
        }

        /// <summary>Cuts the openings and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            var settings = new WallOpeningsSettings { Doors = _doors, Windows = _windows };

            WallOpeningOutcome outcome = WallOpeningCutter.Cut(_document, _walls, settings, Text, _log);

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

        private string BuildReport(WallOpeningOutcome outcome)
        {
            var builder = new StringBuilder();

            if (!outcome.AnyOpening && outcome.Failures.Count == 0)
            {
                builder.AppendLine(Text.NoOpenings);
            }
            else
            {
                builder.AppendLine(Text.Result(outcome.Cut, outcome.AlreadyThere, outcome.Skipped));
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

        private void Remember()
        {
            ToolSettings.Save(
                AppPaths.SettingsDirectory,
                SettingsName,
                new WallOpeningsSettings { Doors = _doors, Windows = _windows });
        }
    }
}
