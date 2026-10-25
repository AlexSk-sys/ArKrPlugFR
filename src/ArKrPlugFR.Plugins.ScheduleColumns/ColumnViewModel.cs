using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>
    /// The state of the tool window: the chosen schedule and the run.
    /// <para>
    /// The window only shows the state; the cells are read and the columns are hidden by the
    /// engine, and the run is one undoable step.
    /// </para>
    /// </summary>
    public sealed class ColumnViewModel : ObservableObject
    {
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<ViewSchedule> _schedules;
        private int _scheduleIndex = -1;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project the schedules live in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public ColumnViewModel(Document document, ColumnText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _schedules = ScheduleColumnsEngine.Schedules(_document).ToList();

            if (_schedules.Count > 0)
            {
                ScheduleIndex = 0;
            }
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public ColumnText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the schedule list.</summary>
        public string ScheduleLabel => Text.ScheduleLabel;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the names of the schedules of the project.</summary>
        public string[] ScheduleNames => _schedules.Select(schedule => schedule.Name).ToArray();

        /// <summary>Gets or sets the chosen schedule.</summary>
        public int ScheduleIndex
        {
            get => _scheduleIndex;
            set => SetProperty(ref _scheduleIndex, value);
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Hides and shows the columns of the chosen schedule.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_schedules.Count == 0)
            {
                SetReport(Text.NoSchedules);

                return Text.NoSchedules;
            }

            if (_scheduleIndex < 0 || _scheduleIndex >= _schedules.Count)
            {
                SetReport(Text.NoSchedule);

                return Text.NoSchedule;
            }

            ColumnsOutcome outcome = ScheduleColumnsEngine.Run(_document, _schedules[_scheduleIndex], Text, _log);

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

        private string BuildReport(ColumnsOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.ResultLine(outcome.Hidden, outcome.Shown, outcome.Left));

                if (outcome.Hidden == 0 && outcome.Shown == 0)
                {
                    builder.AppendLine(Text.NothingChanged);
                }
            }

            foreach (string note in outcome.Notes)
            {
                builder.Append(" - ");
                builder.AppendLine(note);
            }

            return builder.ToString().TrimEnd();
        }
    }
}
