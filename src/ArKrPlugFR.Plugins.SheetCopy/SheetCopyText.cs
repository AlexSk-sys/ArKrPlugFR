using System.Globalization;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the rows and the run take this object instead of a culture
    /// code, so a label is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class SheetCopyText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SheetCopyText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SheetCopyStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => SheetCopyStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the sheets list.</summary>
        public string SheetsHeader => SheetCopyStrings.SheetsHeader.Resolve(_culture);

        /// <summary>Gets the caption of the target list.</summary>
        public string TargetsHeader => SheetCopyStrings.TargetsHeader.Resolve(_culture);

        /// <summary>Gets the hint under the target list.</summary>
        public string TargetsHint => SheetCopyStrings.TargetsHint.Resolve(_culture);

        /// <summary>Gets the hint shown when no other project is open.</summary>
        public string NoTargetsHint => SheetCopyStrings.NoTargetsHint.Resolve(_culture);

        /// <summary>Gets the note of a project that has not been saved yet.</summary>
        public string TargetUnsaved => SheetCopyStrings.TargetUnsaved.Resolve(_culture);

        /// <summary>Gets the reason of a family document that cannot take sheets.</summary>
        public string TargetFamily => SheetCopyStrings.TargetFamily.Resolve(_culture);

        /// <summary>Gets the reason of a linked model that cannot take sheets.</summary>
        public string TargetLinked => SheetCopyStrings.TargetLinked.Resolve(_culture);

        /// <summary>Gets the reason of a read-only project that cannot take sheets.</summary>
        public string TargetReadOnly => SheetCopyStrings.TargetReadOnly.Resolve(_culture);

        /// <summary>Gets the reason of a project that was closed while the window was open.</summary>
        public string TargetLost => SheetCopyStrings.TargetLost.Resolve(_culture);

        /// <summary>Gets the hint under the sheets list.</summary>
        public string SelectHint => SheetCopyStrings.SelectHint.Resolve(_culture);

        /// <summary>Gets the hint shown when this project has no sheets.</summary>
        public string EmptyHint => SheetCopyStrings.EmptyHint.Resolve(_culture);

        /// <summary>Gets the caption of the select-all button.</summary>
        public string SelectAll => SheetCopyStrings.SelectAll.Resolve(_culture);

        /// <summary>Gets the caption of the clear button.</summary>
        public string ClearSelection => SheetCopyStrings.ClearSelection.Resolve(_culture);

        /// <summary>Gets the caption of the copy button.</summary>
        public string Copy => SheetCopyStrings.Copy.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SheetCopyStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => SheetCopyStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => SheetCopyStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when nothing is ticked.</summary>
        public string NothingSelected => SheetCopyStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the message shown when no target project is ticked.</summary>
        public string NoTargetSelected => SheetCopyStrings.NoTargetSelected.Resolve(_culture);

        /// <summary>Gets the marker that marks the name of every copy.</summary>
        public string CopyMarker => SheetCopyStrings.CopyMarker.Resolve(_culture);

        /// <summary>Gets the reason reported when the copy of a sheet produced nothing.</summary>
        public string CopyReturnedNothing => SheetCopyStrings.CopyReturnedNothing.Resolve(_culture);

        /// <summary>Gets the reason reported when every generated sheet number was refused.</summary>
        public string NumberRefused => SheetCopyStrings.NumberRefused.Resolve(_culture);

        /// <summary>Gets the line that names the source project.</summary>
        /// <param name="title">Title of the source document.</param>
        public string SourceLine(string title) => SheetCopyStrings.SourceLine.Resolve(_culture, title);

        /// <summary>Gets the line that names the log folder.</summary>
        /// <param name="folder">Path of the log folder.</param>
        public string LogFolder(string folder) => SheetCopyStrings.LogFolder.Resolve(_culture, folder);

        /// <summary>Gets the general failure line with a reason.</summary>
        /// <param name="reason">What went wrong.</param>
        public string GeneralFailure(string reason) => SheetCopyStrings.GeneralFailure.Resolve(_culture, reason);

        /// <summary>Gets the summary line of one target project.</summary>
        /// <param name="created">Copies that were created.</param>
        /// <param name="planned">Copies that were planned.</param>
        /// <param name="targetTitle">Title of the target project.</param>
        public string TargetSummary(int created, int planned, string targetTitle) =>
            SheetCopyStrings.TargetSummary.Resolve(_culture, created, planned, targetTitle);

        /// <summary>Gets the line that reports a target project the run could not enter.</summary>
        /// <param name="targetTitle">Title of the target project.</param>
        /// <param name="reason">What went wrong.</param>
        public string TargetFailed(string targetTitle, string reason) =>
            SheetCopyStrings.TargetFailed.Resolve(_culture, targetTitle, reason);

        /// <summary>Gets the line that reports a sheet that could not be copied.</summary>
        /// <param name="number">Sheet number in the source project.</param>
        /// <param name="reason">What went wrong.</param>
        public string SheetFailed(string number, string reason) =>
            SheetCopyStrings.SheetFailed.Resolve(_culture, number, reason);

        /// <summary>Gets the line that says how many report lines were left out.</summary>
        /// <param name="count">Number of left-out lines.</param>
        public string MoreLine(int count) => SheetCopyStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
