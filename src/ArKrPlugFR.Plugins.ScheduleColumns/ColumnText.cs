namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class ColumnText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public ColumnText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => ColumnStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => ColumnStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the schedule list.</summary>
        public string ScheduleLabel => ColumnStrings.ScheduleLabel.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => ColumnStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => ColumnStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => ColumnStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => ColumnStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the project holds no schedules.</summary>
        public string NoSchedules => ColumnStrings.NoSchedules.Resolve(_culture);

        /// <summary>Gets the message shown when no schedule was chosen.</summary>
        public string NoSchedule => ColumnStrings.NoSchedule.Resolve(_culture);

        /// <summary>Gets the message shown for a schedule without a body.</summary>
        public string NoBody => ColumnStrings.NoBody.Resolve(_culture);

        /// <summary>Gets the message shown when nothing needed changing.</summary>
        public string NothingChanged => ColumnStrings.NothingChanged.Resolve(_culture);

        /// <summary>Gets the line that counts the run.</summary>
        /// <param name="hidden">Columns that were hidden.</param>
        /// <param name="shown">Columns that were shown.</param>
        /// <param name="left">Columns that were left as they were.</param>
        public string ResultLine(int hidden, int shown, int left) =>
            ColumnStrings.Result.Resolve(_culture, hidden, shown, left);

        /// <summary>Gets the line that reports a column without a parameter.</summary>
        /// <param name="column">Number of the column.</param>
        public string NoParameterColumn(int column) => ColumnStrings.NoParameterColumn.Resolve(_culture, column);

        /// <summary>Gets the line that reports a refused column.</summary>
        /// <param name="column">Number of the column.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(int column, string reason) => ColumnStrings.Refused.Resolve(_culture, column, reason);
    }
}
