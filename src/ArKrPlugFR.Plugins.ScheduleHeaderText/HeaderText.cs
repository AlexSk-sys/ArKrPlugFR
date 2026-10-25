namespace ArKrPlugFR.Plugins.ScheduleHeaderText
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class HeaderText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public HeaderText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => HeaderStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => HeaderStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the schedule list.</summary>
        public string ScheduleLabel => HeaderStrings.ScheduleLabel.Resolve(_culture);

        /// <summary>Gets the caption of the parameter list.</summary>
        public string ParameterLabel => HeaderStrings.ParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the value field.</summary>
        public string ValueLabel => HeaderStrings.ValueLabel.Resolve(_culture);

        /// <summary>Gets the caption of the switch that takes the value from the elements.</summary>
        public string FromElements => HeaderStrings.FromElements.Resolve(_culture);

        /// <summary>Gets the hint inside the value field.</summary>
        public string ValuePlaceholder => HeaderStrings.ValuePlaceholder.Resolve(_culture);

        /// <summary>Gets the caption of the header row field.</summary>
        public string RowLabel => HeaderStrings.RowLabel.Resolve(_culture);

        /// <summary>Gets the caption of the header column field.</summary>
        public string ColumnLabel => HeaderStrings.ColumnLabel.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => HeaderStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => HeaderStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => HeaderStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => HeaderStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the project holds no schedules.</summary>
        public string NoSchedules => HeaderStrings.NoSchedules.Resolve(_culture);

        /// <summary>Gets the message shown when the schedule has no parameter fields.</summary>
        public string NoParameters => HeaderStrings.NoParameters.Resolve(_culture);

        /// <summary>Gets the message shown when no schedule was chosen.</summary>
        public string NoSchedule => HeaderStrings.NoSchedule.Resolve(_culture);

        /// <summary>Gets the message shown when no parameter was chosen.</summary>
        public string NoParameter => HeaderStrings.NoParameter.Resolve(_culture);

        /// <summary>Gets the message shown when the value is empty.</summary>
        public string NoValue => HeaderStrings.NoValue.Resolve(_culture);

        /// <summary>Gets the message shown for a schedule without a single category.</summary>
        public string NoCategory => HeaderStrings.NoCategory.Resolve(_culture);

        /// <summary>Gets the message shown when no element carries a value.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string NoElementValue(string parameter) => HeaderStrings.NoElementValue.Resolve(_culture, parameter);

        /// <summary>Gets the message shown for a schedule without a header area.</summary>
        public string NoHeader => HeaderStrings.NoHeader.Resolve(_culture);

        /// <summary>Gets the message shown for a row outside the header.</summary>
        /// <param name="rows">How many rows the header holds.</param>
        public string RowOutOfRange(int rows) => HeaderStrings.RowOutOfRange.Resolve(_culture, rows);

        /// <summary>Gets the message shown for a column outside the header.</summary>
        /// <param name="columns">How many columns the header holds.</param>
        public string ColumnOutOfRange(int columns) => HeaderStrings.ColumnOutOfRange.Resolve(_culture, columns);

        /// <summary>Gets the line that reports a successful write.</summary>
        /// <param name="value">Value that was written.</param>
        /// <param name="schedule">Name of the schedule.</param>
        /// <param name="row">Row of the cell.</param>
        /// <param name="column">Column of the cell.</param>
        public string Written(string value, string schedule, int row, int column) =>
            HeaderStrings.Written.Resolve(_culture, value, schedule, row, column);

        /// <summary>Gets the message shown when the platform refused the write.</summary>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string reason) => HeaderStrings.WriteFailed.Resolve(_culture, reason);

        /// <summary>Gets the note about where a tool may write in a schedule.</summary>
        public string BodyNote => HeaderStrings.BodyNote.Resolve(_culture);
    }
}
