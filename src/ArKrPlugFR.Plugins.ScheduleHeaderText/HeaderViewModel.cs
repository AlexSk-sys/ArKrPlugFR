using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ScheduleHeaderText
{
    /// <summary>
    /// The state of the tool window: the chosen schedule, its parameter, the value and the
    /// header cell.
    /// <para>
    /// The window only shows the state; the schedules and their fields are read by the
    /// engine, and the write happens in one transaction there.
    /// </para>
    /// </summary>
    public sealed class HeaderViewModel : ObservableObject
    {
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<ViewSchedule> _schedules;
        private readonly List<HeaderField> _fields = new List<HeaderField>();
        private int _scheduleIndex = -1;
        private int _parameterIndex = -1;
        private bool _fromElements = true;
        private string _value = string.Empty;
        private string _row = "4";
        private string _column = "1";
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project the schedules live in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public HeaderViewModel(Document document, HeaderText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _schedules = HeaderEngine.Schedules(_document).ToList();
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public HeaderText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the schedule list.</summary>
        public string ScheduleLabel => Text.ScheduleLabel;

        /// <summary>Gets the caption of the parameter list.</summary>
        public string ParameterLabel => Text.ParameterLabel;

        /// <summary>Gets the caption of the value field.</summary>
        public string ValueLabel => Text.ValueLabel;

        /// <summary>Gets the caption of the switch that takes the value from the elements.</summary>
        public string FromElementsText => Text.FromElements;

        /// <summary>Gets the caption of the header row field.</summary>
        public string RowLabel => Text.RowLabel;

        /// <summary>Gets the caption of the header column field.</summary>
        public string ColumnLabel => Text.ColumnLabel;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the note about where a tool may write in a schedule.</summary>
        public string BodyNote => Text.BodyNote;

        /// <summary>Gets the names of the schedules of the project.</summary>
        public string[] ScheduleNames => _schedules.Select(schedule => schedule.Name).ToArray();

        /// <summary>Gets the names of the parameter fields of the chosen schedule.</summary>
        public string[] ParameterNames => _fields.Select(field => field.Name).ToArray();

        /// <summary>Gets or sets the chosen schedule.</summary>
        public int ScheduleIndex
        {
            get => _scheduleIndex;

            set
            {
                if (SetProperty(ref _scheduleIndex, value))
                {
                    ReloadFields();
                }
            }
        }

        /// <summary>Gets or sets the chosen parameter field.</summary>
        public int ParameterIndex
        {
            get => _parameterIndex;
            set => SetProperty(ref _parameterIndex, value);
        }

        /// <summary>Gets or sets a value indicating whether the value comes from the elements.</summary>
        public bool FromElements
        {
            get => _fromElements;
            set => SetProperty(ref _fromElements, value);
        }

        /// <summary>Gets or sets the value typed in the window.</summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value ?? string.Empty);
        }

        /// <summary>Gets or sets the header row counted from one.</summary>
        public string Row
        {
            get => _row;
            set => SetProperty(ref _row, value ?? string.Empty);
        }

        /// <summary>Gets or sets the header column counted from one.</summary>
        public string Column
        {
            get => _column;
            set => SetProperty(ref _column, value ?? string.Empty);
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Writes the value into the header of the chosen schedule.</summary>
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

            if (_parameterIndex < 0 || _parameterIndex >= _fields.Count)
            {
                SetReport(Text.NoParameter);

                return Text.NoParameter;
            }

            if (!int.TryParse(_row, out int row) || !int.TryParse(_column, out int column))
            {
                SetReport(Text.NoValue);

                return Text.NoValue;
            }

            HeaderOutcome outcome = HeaderEngine.Run(
                _document,
                _schedules[_scheduleIndex],
                _fields[_parameterIndex],
                _fromElements,
                _value,
                row,
                column,
                Text,
                _log);

            string message = outcome.Written != null
                ? Text.Written(outcome.Written, _schedules[_scheduleIndex].Name, outcome.Row, outcome.Column)
                : outcome.Stopped ?? string.Empty;

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

        private void ReloadFields()
        {
            _fields.Clear();
            _parameterIndex = -1;

            if (_scheduleIndex >= 0 && _scheduleIndex < _schedules.Count)
            {
                _fields.AddRange(HeaderEngine.Fields(_schedules[_scheduleIndex]));
            }

            RaisePropertyChanged(nameof(ParameterNames));

            if (_fields.Count > 0)
            {
                ParameterIndex = 0;
            }
        }
    }
}
