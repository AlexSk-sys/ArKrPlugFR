namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class AreaRebarText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public AreaRebarText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => AreaRebarStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => AreaRebarStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the switch that chooses the whole project.</summary>
        public string WholeProject => AreaRebarStrings.WholeProject.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the sources.</summary>
        public string PickSources => AreaRebarStrings.PickSources.Resolve(_culture);

        /// <summary>Gets the prompt shown while the sources are picked.</summary>
        public string PickPrompt => AreaRebarStrings.PickPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => AreaRebarStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the line that counts the picked sources.</summary>
        /// <param name="sources">Sources picked.</param>
        public string SourcesLine(int sources) => AreaRebarStrings.SourcesLine.Resolve(_culture, sources);

        /// <summary>Gets the caption of the parameter list.</summary>
        public string ParameterLabel => AreaRebarStrings.ParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the switch that protects existing values.</summary>
        public string OnlyEmpty => AreaRebarStrings.OnlyEmpty.Resolve(_culture);

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => AreaRebarStrings.Hint.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => AreaRebarStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => AreaRebarStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => AreaRebarStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => AreaRebarStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the scope holds no source.</summary>
        public string NoSources => AreaRebarStrings.NoSources.Resolve(_culture);

        /// <summary>Gets the message shown when no parameter was chosen.</summary>
        public string ParameterRequired => AreaRebarStrings.ParameterRequired.Resolve(_culture);

        /// <summary>Gets the line of a source without a value.</summary>
        /// <param name="source">Description of the source.</param>
        public string NoSourceValue(string source) => AreaRebarStrings.NoSourceValue.Resolve(_culture, source);

        /// <summary>Gets the line of a source without bars.</summary>
        /// <param name="source">Description of the source.</param>
        public string NoBars(string source) => AreaRebarStrings.NoBars.Resolve(_culture, source);

        /// <summary>Gets the line of a bar that cannot receive the value.</summary>
        /// <param name="bar">Description of the bar.</param>
        public string NoTargetParameter(string bar) => AreaRebarStrings.NoTargetParameter.Resolve(_culture, bar);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="bar">Description of the bar.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string bar, string reason) => AreaRebarStrings.WriteFailed.Resolve(_culture, bar, reason);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="sources">Sources read.</param>
        /// <param name="noValue">Sources without a value.</param>
        /// <param name="noBars">Sources without bars.</param>
        /// <param name="filled">Bars filled.</param>
        /// <param name="kept">Bars kept.</param>
        /// <param name="refused">Refusals.</param>
        public string Result(int sources, int noValue, int noBars, int filled, int kept, int refused) =>
            AreaRebarStrings.Result.Resolve(_culture, sources, noValue, noBars, filled, kept, refused);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => AreaRebarStrings.MoreLine.Resolve(_culture, count);
    }
}
