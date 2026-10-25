namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class CompareText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public CompareText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => CompareStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => CompareStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the source element.</summary>
        public string PickSource => CompareStrings.PickSource.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the target element.</summary>
        public string PickTarget => CompareStrings.PickTarget.Resolve(_culture);

        /// <summary>Gets the prompt shown while the source element is picked.</summary>
        public string PickSourcePrompt => CompareStrings.PickSourcePrompt.Resolve(_culture);

        /// <summary>Gets the prompt shown while the target element is picked.</summary>
        public string PickTargetPrompt => CompareStrings.PickTargetPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => CompareStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the caption of the source line.</summary>
        public string SourceLabel => CompareStrings.SourceLabel.Resolve(_culture);

        /// <summary>Gets the caption of the target line.</summary>
        public string TargetLabel => CompareStrings.TargetLabel.Resolve(_culture);

        /// <summary>Gets the words shown for a side that was not picked.</summary>
        public string NotPicked => CompareStrings.NotPicked.Resolve(_culture);

        /// <summary>Gets the caption of the comparison list.</summary>
        public string ComparisonCaption => CompareStrings.ComparisonCaption.Resolve(_culture);

        /// <summary>Gets the line that counts the rows and the differences.</summary>
        /// <param name="rows">Rows of the comparison.</param>
        /// <param name="differences">Rows whose values differ.</param>
        public string ComparisonLine(int rows, int differences) =>
            CompareStrings.ComparisonLine.Resolve(_culture, rows, differences);

        /// <summary>Gets the caption of the copy button.</summary>
        public string Copy => CompareStrings.Copy.Resolve(_culture);

        /// <summary>Gets the hint under the list.</summary>
        public string Hint => CompareStrings.Hint.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => CompareStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => CompareStrings.Close.Resolve(_culture);

        /// <summary>Gets the name of the copy in the undo list.</summary>
        public string TransactionName => CompareStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when one of the sides was not picked.</summary>
        public string NoPicks => CompareStrings.NoPicks.Resolve(_culture);

        /// <summary>Gets the message shown when nothing differs.</summary>
        public string NothingToCopy => CompareStrings.NothingToCopy.Resolve(_culture);

        /// <summary>Gets the line of a value the target would not take.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string NoTarget(string parameter) => CompareStrings.NoTarget.Resolve(_culture, parameter);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string parameter, string reason) => CompareStrings.WriteFailed.Resolve(_culture, parameter, reason);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="compared">Rows compared.</param>
        /// <param name="different">Rows whose values differ.</param>
        /// <param name="copied">Values copied.</param>
        /// <param name="skipped">Rows skipped because the source value is empty.</param>
        /// <param name="refused">Refusals.</param>
        public string Result(int compared, int different, int copied, int skipped, int refused) =>
            CompareStrings.Result.Resolve(_culture, compared, different, copied, skipped, refused);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => CompareStrings.MoreLine.Resolve(_culture, count);
    }
}
