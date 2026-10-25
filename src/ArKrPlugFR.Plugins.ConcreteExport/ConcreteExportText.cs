namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class ConcreteExportText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public ConcreteExportText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => ConcreteExportStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => ConcreteExportStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the switch that narrows the scope to the selection.</summary>
        public string OnlySelection => ConcreteExportStrings.OnlySelection.Resolve(_culture);

        /// <summary>Gets the caption of the button that chooses the file.</summary>
        public string ChooseFile => ConcreteExportStrings.ChooseFile.Resolve(_culture);

        /// <summary>Gets the caption of the file line.</summary>
        public string FileLabel => ConcreteExportStrings.FileLabel.Resolve(_culture);

        /// <summary>Gets the words shown for a file that was not chosen.</summary>
        public string NotChosen => ConcreteExportStrings.NotChosen.Resolve(_culture);

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => ConcreteExportStrings.Hint.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Export => ConcreteExportStrings.Export.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => ConcreteExportStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => ConcreteExportStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the message shown when no file was chosen.</summary>
        public string FileRequired => ConcreteExportStrings.FileRequired.Resolve(_culture);

        /// <summary>Gets the message shown when the scope holds no concrete.</summary>
        public string NothingToExport => ConcreteExportStrings.NothingToExport.Resolve(_culture);

        /// <summary>Gets the message shown when the file could not be written.</summary>
        /// <param name="path">Path of the file.</param>
        /// <param name="reason">What the system reported.</param>
        public string FileFailed(string path, string reason) => ConcreteExportStrings.FileFailed.Resolve(_culture, path, reason);

        /// <summary>Gets the line of an element that could not be read.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="reason">What the platform reported.</param>
        public string RowFailed(string element, string reason) => ConcreteExportStrings.RowFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="scanned">Elements scanned.</param>
        /// <param name="exported">Rows written.</param>
        /// <param name="notConcrete">Elements left out as not concrete.</param>
        /// <param name="failed">Refusals.</param>
        /// <param name="path">Path of the file.</param>
        public string Result(int scanned, int exported, int notConcrete, int failed, string path) =>
            ConcreteExportStrings.Result.Resolve(_culture, scanned, exported, notConcrete, failed, path);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => ConcreteExportStrings.MoreLine.Resolve(_culture, count);
    }
}
