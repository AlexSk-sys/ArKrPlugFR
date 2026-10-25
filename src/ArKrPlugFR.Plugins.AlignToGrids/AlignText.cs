namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the engine and the report take this object instead of a culture
    /// code, so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class AlignText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public AlignText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => AlignStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => AlignStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the apply button.</summary>
        public string Apply => AlignStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => AlignStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => AlignStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => AlignStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks columns.</summary>
        public string PickColumns => AlignStrings.PickColumns.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks grids.</summary>
        public string PickGrids => AlignStrings.PickGrids.Resolve(_culture);

        /// <summary>Gets the prompt shown while the user picks columns.</summary>
        public string PickColumnsPrompt => AlignStrings.PickColumnsPrompt.Resolve(_culture);

        /// <summary>Gets the prompt shown while the user picks grids.</summary>
        public string PickGridsPrompt => AlignStrings.PickGridsPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => AlignStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the caption of the capture field.</summary>
        public string CaptureLabel => AlignStrings.CaptureLabel.Resolve(_culture);

        /// <summary>Gets the hint under the capture field.</summary>
        public string OptionsHint => AlignStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the message shown when the capture cannot be read.</summary>
        public string CaptureRequired => AlignStrings.CaptureRequired.Resolve(_culture);

        /// <summary>Gets the message shown when no columns were picked.</summary>
        public string NoColumns => AlignStrings.NoColumns.Resolve(_culture);

        /// <summary>Gets the message shown when no usable grid was picked.</summary>
        public string NoGrids => AlignStrings.NoGrids.Resolve(_culture);

        /// <summary>Gets the line that counts the picked columns.</summary>
        /// <param name="columns">Columns picked.</param>
        public string ColumnsLine(int columns) => AlignStrings.ColumnsLine.Resolve(_culture, columns);

        /// <summary>Gets the line that counts the picked grids.</summary>
        /// <param name="grids">Grids picked.</param>
        public string GridsLine(int grids) => AlignStrings.GridsLine.Resolve(_culture, grids);

        /// <summary>Gets the line of a curved grid.</summary>
        /// <param name="grid">Name of the grid.</param>
        public string GridNotStraight(string grid) => AlignStrings.GridNotStraight.Resolve(_culture, grid);

        /// <summary>Gets the line of a grid outside the horizontal plane.</summary>
        /// <param name="grid">Name of the grid.</param>
        public string GridNotInPlane(string grid) => AlignStrings.GridNotInPlane.Resolve(_culture, grid);

        /// <summary>Gets the line of a column without a location point.</summary>
        /// <param name="column">Description of the column.</param>
        public string NotPointBased(string column) => AlignStrings.NotPointBased.Resolve(_culture, column);

        /// <summary>Gets the line of a column the platform refused to move.</summary>
        /// <param name="element">Description of the column.</param>
        /// <param name="reason">What the platform reported.</param>
        public string MoveFailed(string element, string reason) =>
            AlignStrings.MoveFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="moved">Columns that were moved.</param>
        /// <param name="columns">Columns that were read.</param>
        /// <param name="alreadyAligned">Columns already on a grid.</param>
        /// <param name="noGrid">Columns without a grid in reach.</param>
        /// <param name="inGroup">Columns skipped as group members.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(int moved, int columns, int alreadyAligned, int noGrid, int inGroup, int failed) =>
            AlignStrings.Result.Resolve(_culture, moved, columns, alreadyAligned, noGrid, inGroup, failed);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => AlignStrings.MoreLine.Resolve(_culture, count);
    }
}
