namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window takes this object instead of a culture code, so a message is written
    /// once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class SheetBrowserText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SheetBrowserText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SheetBrowserStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => SheetBrowserStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the button that copies the path of the selected row.</summary>
        public string CopyPath => SheetBrowserStrings.CopyPath.Resolve(_culture);

        /// <summary>Gets the hint under the tree.</summary>
        public string Hint => SheetBrowserStrings.Hint.Resolve(_culture);

        /// <summary>Gets the line that counts the sheets and groups.</summary>
        /// <param name="sheets">Sheets in the tree.</param>
        /// <param name="groups">Groups in the tree.</param>
        /// <param name="excluded">Sheets the scheme leaves out.</param>
        /// <param name="unreadable">Sheets whose folders could not be read.</param>
        public string Summary(int sheets, int groups, int excluded, int unreadable) =>
            SheetBrowserStrings.Summary.Resolve(_culture, sheets, groups, excluded, unreadable);

        /// <summary>Gets the line shown when the project holds no sheets.</summary>
        public string NoSheets => SheetBrowserStrings.NoSheets.Resolve(_culture);

        /// <summary>Gets the line shown when the project has no organization scheme.</summary>
        public string NoScheme => SheetBrowserStrings.NoScheme.Resolve(_culture);

        /// <summary>Gets the message shown when no row was selected.</summary>
        public string NoSelection => SheetBrowserStrings.NoSelection.Resolve(_culture);

        /// <summary>Gets the message shown after a path was copied.</summary>
        /// <param name="path">Path that was copied.</param>
        public string Copied(string path) => SheetBrowserStrings.Copied.Resolve(_culture, path);

        /// <summary>Gets the message shown when the clipboard refused the text.</summary>
        /// <param name="reason">What the clipboard reported.</param>
        public string CopyFailed(string reason) => SheetBrowserStrings.CopyFailed.Resolve(_culture, reason);

        /// <summary>Gets the name of the group that holds sheets with unreadable folders.</summary>
        public string GroupingError => SheetBrowserStrings.GroupingError.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SheetBrowserStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SheetBrowserStrings.Close.Resolve(_culture);
    }
}
