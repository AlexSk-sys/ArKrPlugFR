namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class ExtensionText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public ExtensionText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => ExtensionStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => ExtensionStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the sources.</summary>
        public string SourcesLabel => ExtensionStrings.SourcesLabel.Resolve(_culture);

        /// <summary>Gets the caption of the targets.</summary>
        public string TargetsLabel => ExtensionStrings.TargetsLabel.Resolve(_culture);

        /// <summary>Gets the caption of the source pick button.</summary>
        public string PickSources => ExtensionStrings.PickSources.Resolve(_culture);

        /// <summary>Gets the caption of the target pick button.</summary>
        public string PickTargets => ExtensionStrings.PickTargets.Resolve(_culture);

        /// <summary>Gets the caption of the reach field.</summary>
        public string ReachLabel => ExtensionStrings.ReachLabel.Resolve(_culture);

        /// <summary>Gets the state shown before sources are picked.</summary>
        public string SourcesNone => ExtensionStrings.SourcesNone.Resolve(_culture);

        /// <summary>Gets the state shown before targets are picked.</summary>
        public string TargetsNone => ExtensionStrings.TargetsNone.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => ExtensionStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => ExtensionStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => ExtensionStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => ExtensionStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the prompt of the source pick.</summary>
        public string PickSourcesPrompt => ExtensionStrings.PickSourcesPrompt.Resolve(_culture);

        /// <summary>Gets the prompt of the target pick.</summary>
        public string PickTargetsPrompt => ExtensionStrings.PickTargetsPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string PickCancelled => ExtensionStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the message shown when no source was picked.</summary>
        public string NoSources => ExtensionStrings.NoSources.Resolve(_culture);

        /// <summary>Gets the message shown when no target was picked.</summary>
        public string NoTargets => ExtensionStrings.NoTargets.Resolve(_culture);

        /// <summary>Gets the message shown when no target has a bounding box.</summary>
        public string NoTargetBoxes => ExtensionStrings.NoTargetBoxes.Resolve(_culture);

        /// <summary>Gets the message shown when nothing needed extending.</summary>
        public string NothingExtended => ExtensionStrings.NothingExtended.Resolve(_culture);

        /// <summary>Gets the line that counts the run.</summary>
        /// <param name="sources">Sources considered.</param>
        /// <param name="extended">Sources that were extended.</param>
        /// <param name="noTarget">Sources without a target above.</param>
        /// <param name="noLine">Sources without an analytical line.</param>
        /// <param name="attached">Sources attached at the top.</param>
        /// <param name="refused">Refusals.</param>
        public string ResultLine(int sources, int extended, int noTarget, int noLine, int attached, int refused) =>
            ExtensionStrings.Result.Resolve(_culture, sources, extended, noTarget, noLine, attached, refused);

        /// <summary>Gets the line that reports a source without an analytical line.</summary>
        /// <param name="source">Description of the source.</param>
        public string NoAnalyticalLine(string source) => ExtensionStrings.NoAnalyticalLine.Resolve(_culture, source);

        /// <summary>Gets the line that reports a source without a target above.</summary>
        /// <param name="source">Description of the source.</param>
        public string NoTargetAbove(string source) => ExtensionStrings.NoTargetAbove.Resolve(_culture, source);

        /// <summary>Gets the line that reports a source attached at the top.</summary>
        /// <param name="source">Description of the source.</param>
        public string Attached(string source) => ExtensionStrings.Attached.Resolve(_culture, source);

        /// <summary>Gets the line that reports a refusal.</summary>
        /// <param name="source">Description of the source.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string source, string reason) => ExtensionStrings.Refused.Resolve(_culture, source, reason);

        /// <summary>Gets the line that reports a column raised through its top level.</summary>
        /// <param name="source">Description of the source.</param>
        public string RaisedColumn(string source) => ExtensionStrings.RaisedColumn.Resolve(_culture, source);
    }
}
