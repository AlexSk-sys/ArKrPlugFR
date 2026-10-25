namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the engine and the report take this object instead of a culture
    /// code, so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class SelectText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SelectText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SelectStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => SelectStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the scope list.</summary>
        public string ScopeCaption => SelectStrings.ScopeCaption.Resolve(_culture);

        /// <summary>Gets the scope that finds every family.</summary>
        public string ScopeAll => SelectStrings.ScopeAll.Resolve(_culture);

        /// <summary>Gets the scope that finds the placed types.</summary>
        public string ScopePlaced => SelectStrings.ScopePlaced.Resolve(_culture);

        /// <summary>Gets the scope that finds the types visible in the current view.</summary>
        public string ScopeVisible => SelectStrings.ScopeVisible.Resolve(_culture);

        /// <summary>Gets the line about the view scope not being available on a sheet.</summary>
        public string ScopeUnavailableOnSheets => SelectStrings.ScopeUnavailableOnSheets.Resolve(_culture);

        /// <summary>Gets the caption of the search field.</summary>
        public string SearchCaption => SelectStrings.SearchCaption.Resolve(_culture);

        /// <summary>Gets the hint under the search field.</summary>
        public string SearchHint => SelectStrings.SearchHint.Resolve(_culture);

        /// <summary>Gets the caption of the type tree.</summary>
        public string TreeCaption => SelectStrings.TreeCaption.Resolve(_culture);

        /// <summary>Gets the caption of the button that ticks every type.</summary>
        public string TickAll => SelectStrings.TickAll.Resolve(_culture);

        /// <summary>Gets the caption of the button that clears the ticks.</summary>
        public string UntickAll => SelectStrings.UntickAll.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => SelectStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SelectStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SelectStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the hint under the buttons.</summary>
        public string Hint => SelectStrings.Hint.Resolve(_culture);

        /// <summary>Gets the label used for types that carry no category.</summary>
        public string NoCategory => SelectStrings.NoCategory.Resolve(_culture);

        /// <summary>Gets the message shown when nothing is ticked.</summary>
        public string NoTypes => SelectStrings.NoTypes.Resolve(_culture);

        /// <summary>Gets the message shown when the scope holds no types.</summary>
        public string NoTypesInScope => SelectStrings.NoTypesInScope.Resolve(_culture);

        /// <summary>Gets the line that counts the types the tree shows.</summary>
        /// <param name="types">Types shown.</param>
        public string TypesLine(int types) => SelectStrings.TypesLine.Resolve(_culture, types);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="ticked">Types ticked.</param>
        /// <param name="selected">Elements selected.</param>
        /// <param name="withoutInstances">Types that hold no instances in the scope.</param>
        public string Result(int ticked, int selected, int withoutInstances) =>
            SelectStrings.Result.Resolve(_culture, ticked, selected, withoutInstances);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => SelectStrings.MoreLine.Resolve(_culture, count);
    }
}
