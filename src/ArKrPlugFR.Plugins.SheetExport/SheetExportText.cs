namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class SheetExportText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SheetExportText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SheetExportStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => SheetExportStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the sheet list.</summary>
        public string SheetsLabel => SheetExportStrings.SheetsLabel.Resolve(_culture);

        /// <summary>Gets the caption of the button that ticks every sheet.</summary>
        public string TickAll => SheetExportStrings.TickAll.Resolve(_culture);

        /// <summary>Gets the caption of the button that untickes every sheet.</summary>
        public string UntickAll => SheetExportStrings.UntickAll.Resolve(_culture);

        /// <summary>Gets the caption of the folder field.</summary>
        public string FolderLabel => SheetExportStrings.FolderLabel.Resolve(_culture);

        /// <summary>Gets the caption of the browse button.</summary>
        public string Browse => SheetExportStrings.Browse.Resolve(_culture);

        /// <summary>Gets the caption of the export button.</summary>
        public string Apply => SheetExportStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SheetExportStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SheetExportStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the message shown when the project holds no sheets.</summary>
        public string NoSheets => SheetExportStrings.NoSheets.Resolve(_culture);

        /// <summary>Gets the message shown when no sheet is ticked.</summary>
        public string NoTicked => SheetExportStrings.NoTicked.Resolve(_culture);

        /// <summary>Gets the message shown when no folder is given.</summary>
        public string NoFolder => SheetExportStrings.NoFolder.Resolve(_culture);

        /// <summary>Gets the message shown when the folder does not exist.</summary>
        /// <param name="folder">The folder that was given.</param>
        public string FolderMissing(string folder) => SheetExportStrings.FolderMissing.Resolve(_culture, folder);

        /// <summary>Gets the message shown on a release without the PDF export.</summary>
        public string NotSupported => SheetExportStrings.NotSupported.Resolve(_culture);

        /// <summary>Gets the line that counts the export.</summary>
        /// <param name="sheets">Sheets that were exported.</param>
        /// <param name="folder">Folder they were written into.</param>
        public string ResultLine(int sheets, string folder) => SheetExportStrings.Result.Resolve(_culture, sheets, folder);

        /// <summary>Gets the line of one produced file.</summary>
        /// <param name="sheet">Number and name of the sheet.</param>
        /// <param name="file">File name without its extension.</param>
        public string FileLine(string sheet, string file) => SheetExportStrings.FileLine.Resolve(_culture, sheet, file);

        /// <summary>Gets the warning about a file name two sheets share.</summary>
        /// <param name="file">The file name they share.</param>
        public string Duplicate(string file) => SheetExportStrings.Duplicates.Resolve(_culture, file);

        /// <summary>Gets the message shown when the platform refused the export.</summary>
        /// <param name="reason">What the platform reported.</param>
        public string Failed(string reason) => SheetExportStrings.Failed.Resolve(_culture, reason);
    }
}
