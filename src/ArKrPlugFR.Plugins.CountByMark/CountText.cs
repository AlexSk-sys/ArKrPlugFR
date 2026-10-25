namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class CountText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public CountText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => CountStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => CountStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the kind list.</summary>
        public string KindLabel => CountStrings.KindLabel.Resolve(_culture);

        /// <summary>Gets the hint about the parameter names.</summary>
        public string OptionsHint => CountStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => CountStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => CountStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => CountStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => CountStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when no structure carries a mark.</summary>
        public string NoStructures => CountStrings.NoStructures.Resolve(_culture);

        /// <summary>Gets the message shown when the project holds no bars.</summary>
        public string NoBars => CountStrings.NoBars.Resolve(_culture);

        /// <summary>Gets the message shown when every bar was skipped.</summary>
        public string NothingWritten => CountStrings.NothingWritten.Resolve(_culture);

        /// <summary>Gets the five choices of the kind list, in the order of the list.</summary>
        public string[] Kinds => new[]
        {
            CountStrings.KindColumns.Resolve(_culture),
            CountStrings.KindWalls.Resolve(_culture),
            CountStrings.KindFloors.Resolve(_culture),
            CountStrings.KindBeams.Resolve(_culture),
            CountStrings.KindAll.Resolve(_culture)
        };

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="structures">Structures that were counted.</param>
        /// <param name="marks">Distinct marks they carry.</param>
        /// <param name="bars">Bars that were read.</param>
        /// <param name="updated">Bars that were written.</param>
        /// <param name="noHostMark">Bars without a structure mark.</param>
        /// <param name="noCount">Bars whose mark no structure carries.</param>
        /// <param name="noParameter">Bars without the number parameter.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(
            int structures,
            int marks,
            int bars,
            int updated,
            int noHostMark,
            int noCount,
            int noParameter,
            int failed) =>
            CountStrings.Result.Resolve(
                _culture, structures, marks, bars, updated, noHostMark, noCount, noParameter, failed);

        /// <summary>Gets the line of one refused bar.</summary>
        /// <param name="element">Description of the bar.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string element, string reason) =>
            CountStrings.WriteFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => CountStrings.MoreLine.Resolve(_culture, count);
    }
}
