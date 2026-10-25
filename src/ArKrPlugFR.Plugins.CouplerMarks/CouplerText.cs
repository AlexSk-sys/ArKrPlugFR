namespace ArKrPlugFR.Plugins.CouplerMarks
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class CouplerText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public CouplerText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => CouplerStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => CouplerStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the couplers.</summary>
        public string PickCouplers => CouplerStrings.PickCouplers.Resolve(_culture);

        /// <summary>Gets the prompt shown while the user picks couplers.</summary>
        public string PickPrompt => CouplerStrings.PickPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => CouplerStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the caption of the picked coupler list.</summary>
        public string CouplersCaption => CouplerStrings.CouplersCaption.Resolve(_culture);

        /// <summary>Gets the line that counts the picked couplers.</summary>
        /// <param name="couplers">Couplers picked.</param>
        public string CouplersLine(int couplers) => CouplerStrings.CouplersLine.Resolve(_culture, couplers);

        /// <summary>Gets the caption of the tolerance field.</summary>
        public string ToleranceLabel => CouplerStrings.ToleranceLabel.Resolve(_culture);

        /// <summary>Gets the hint under the tolerance field.</summary>
        public string ToleranceHint => CouplerStrings.ToleranceHint.Resolve(_culture);

        /// <summary>Gets the caption of the field that names the mark parameter.</summary>
        public string MarkParameterLabel => CouplerStrings.MarkParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the field that names the category parameter.</summary>
        public string CategoryParameterLabel => CouplerStrings.CategoryParameterLabel.Resolve(_culture);

        /// <summary>Gets the hint about how the two values are written.</summary>
        public string OptionsHint => CouplerStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => CouplerStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => CouplerStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => CouplerStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => CouplerStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was picked.</summary>
        public string NoCouplers => CouplerStrings.NoCouplers.Resolve(_culture);

        /// <summary>Gets the message shown when a parameter name is missing.</summary>
        public string ParametersRequired => CouplerStrings.ParametersRequired.Resolve(_culture);

        /// <summary>Gets the message shown when the tolerance cannot be read.</summary>
        public string ToleranceRequired => CouplerStrings.ToleranceRequired.Resolve(_culture);

        /// <summary>Gets the line of a coupler without geometry.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        public string NoGeometry(string coupler) => CouplerStrings.NoGeometry.Resolve(_culture, coupler);

        /// <summary>Gets the line of a coupler without a bar in reach.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        public string NoBarNearby(string coupler) => CouplerStrings.NoBarNearby.Resolve(_culture, coupler);

        /// <summary>Gets the line of a bar whose host carries no data.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        public string NoHostData(string coupler) => CouplerStrings.NoHostData.Resolve(_culture, coupler);

        /// <summary>Gets the line of a coupler whose parameters cannot receive the values.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        public string NoTargetParameter(string coupler) => CouplerStrings.NoTargetParameter.Resolve(_culture, coupler);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string coupler, string reason) => CouplerStrings.WriteFailed.Resolve(_culture, coupler, reason);

        /// <summary>Gets the line of a coupler that only received one of the two values.</summary>
        /// <param name="coupler">Description of the coupler.</param>
        public string HalfPair(string coupler) => CouplerStrings.HalfPair.Resolve(_culture, coupler);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="considered">Couplers read.</param>
        /// <param name="updated">Couplers updated.</param>
        /// <param name="noBarNearby">Couplers without a bar in reach.</param>
        /// <param name="noHostData">Couplers whose bar host carries no data.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(int considered, int updated, int noBarNearby, int noHostData, int failed) =>
            CouplerStrings.Result.Resolve(_culture, considered, updated, noBarNearby, noHostData, failed);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => CouplerStrings.MoreLine.Resolve(_culture, count);
    }
}
