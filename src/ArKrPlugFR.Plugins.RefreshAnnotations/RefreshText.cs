namespace ArKrPlugFR.Plugins.RefreshAnnotations
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The command and the refresh take this object instead of a culture code, so a
    /// message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class RefreshText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public RefreshText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => RefreshStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => RefreshStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => RefreshStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => RefreshStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when there is no active view.</summary>
        public string NoView => RefreshStrings.NoView.Resolve(_culture);

        /// <summary>Gets the message shown when the view holds no tags.</summary>
        public string NoTags => RefreshStrings.NoTags.Resolve(_culture);

        /// <summary>Gets the message shown when every tag of the view is orphaned.</summary>
        public string AllOrphaned => RefreshStrings.AllOrphaned.Resolve(_culture);

        /// <summary>Gets the message shown when the selection carries no tag.</summary>
        public string NothingSelected => RefreshStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="refreshed">Tags that were nudged.</param>
        /// <param name="tags">Tags the view holds.</param>
        /// <param name="view">Name of the active view.</param>
        /// <param name="orphaned">Tags whose element is gone.</param>
        /// <param name="refused">Tags the platform refused to move.</param>
        public string Result(int refreshed, int tags, string view, int orphaned, int refused) =>
            RefreshStrings.Result.Resolve(_culture, refreshed, tags, view, orphaned, refused);

        /// <summary>Gets the line of one refused tag.</summary>
        /// <param name="element">Description of the tag.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string element, string reason) =>
            RefreshStrings.Refused.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => RefreshStrings.MoreLine.Resolve(_culture, count);
    }
}
