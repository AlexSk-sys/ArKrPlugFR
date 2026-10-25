namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class PileText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public PileText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => PileStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => PileStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the piles.</summary>
        public string PickPiles => PileStrings.PickPiles.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the slabs.</summary>
        public string PickSlabs => PileStrings.PickSlabs.Resolve(_culture);

        /// <summary>Gets the prompt shown while the piles are picked.</summary>
        public string PickPilesPrompt => PileStrings.PickPilesPrompt.Resolve(_culture);

        /// <summary>Gets the prompt shown while the slabs are picked.</summary>
        public string PickSlabsPrompt => PileStrings.PickSlabsPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string PickCancelled => PileStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the caption of the pile list.</summary>
        public string PilesCaption => PileStrings.PilesCaption.Resolve(_culture);

        /// <summary>Gets the caption of the slab list.</summary>
        public string SlabsCaption => PileStrings.SlabsCaption.Resolve(_culture);

        /// <summary>Gets the line that counts the picked piles.</summary>
        /// <param name="piles">Piles picked.</param>
        public string PilesLine(int piles) => PileStrings.PilesLine.Resolve(_culture, piles);

        /// <summary>Gets the line that counts the picked slabs.</summary>
        /// <param name="slabs">Slabs picked.</param>
        public string SlabsLine(int slabs) => PileStrings.SlabsLine.Resolve(_culture, slabs);

        /// <summary>Gets the caption of the start number field.</summary>
        public string StartLabel => PileStrings.StartLabel.Resolve(_culture);

        /// <summary>Gets the caption of the direction switch.</summary>
        public string TopDown => PileStrings.TopDown.Resolve(_culture);

        /// <summary>Gets the caption of the number parameter field.</summary>
        public string NumberParameterLabel => PileStrings.NumberParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the cut height parameter field.</summary>
        public string CutParameterLabel => PileStrings.CutParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the raft bottom parameter field.</summary>
        public string SlabParameterLabel => PileStrings.SlabParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the pile bottom parameter field.</summary>
        public string PileParameterLabel => PileStrings.PileParameterLabel.Resolve(_culture);

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => PileStrings.Hint.Resolve(_culture);

        /// <summary>Gets the caption of the numbering button.</summary>
        public string NumberButton => PileStrings.NumberButton.Resolve(_culture);

        /// <summary>Gets the caption of the elevation button.</summary>
        public string ElevationsButton => PileStrings.ElevationsButton.Resolve(_culture);

        /// <summary>Gets the caption of the cut height button.</summary>
        public string CutButton => PileStrings.CutButton.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => PileStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => PileStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the numbering in the undo list.</summary>
        public string NumberTransactionName => PileStrings.NumberTransaction.Resolve(_culture);

        /// <summary>Gets the name of the elevation write in the undo list.</summary>
        public string ElevationTransactionName => PileStrings.ElevationTransaction.Resolve(_culture);

        /// <summary>Gets the name of the cut height write in the undo list.</summary>
        public string CutTransactionName => PileStrings.CutTransaction.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was picked.</summary>
        public string NoPiles => PileStrings.NoPiles.Resolve(_culture);

        /// <summary>Gets the message shown when no slab was picked.</summary>
        public string NoSlabs => PileStrings.NoSlabs.Resolve(_culture);

        /// <summary>Gets the message shown when the start number cannot be read.</summary>
        public string StartRequired => PileStrings.StartRequired.Resolve(_culture);

        /// <summary>Gets the message shown when the number parameter is missing.</summary>
        public string NumberParameterRequired => PileStrings.NumberParameterRequired.Resolve(_culture);

        /// <summary>Gets the line of a pile whose parameter cannot receive the value.</summary>
        /// <param name="pile">Description of the pile.</param>
        /// <param name="parameter">Name of the parameter.</param>
        public string NoTarget(string pile, string parameter) => PileStrings.NoTarget.Resolve(_culture, pile, parameter);

        /// <summary>Gets the line of a pile without a box.</summary>
        /// <param name="pile">Description of the pile.</param>
        public string NoGeometry(string pile) => PileStrings.NoGeometry.Resolve(_culture, pile);

        /// <summary>Gets the line of a pile no slab takes in.</summary>
        /// <param name="pile">Description of the pile.</param>
        public string NoSlabForPile(string pile) => PileStrings.NoSlabForPile.Resolve(_culture, pile);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="pile">Description of the pile.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string pile, string reason) => PileStrings.WriteFailed.Resolve(_culture, pile, reason);

        /// <summary>Gets the result line of the numbering.</summary>
        /// <param name="considered">Piles read.</param>
        /// <param name="written">Piles numbered.</param>
        /// <param name="refused">Refusals.</param>
        public string NumberResult(int considered, int written, int refused) =>
            PileStrings.NumberResult.Resolve(_culture, considered, written, refused);

        /// <summary>Gets the result line of the elevation write.</summary>
        /// <param name="considered">Piles read.</param>
        /// <param name="written">Piles written.</param>
        /// <param name="skipped">Piles skipped.</param>
        /// <param name="refused">Refusals.</param>
        public string ElevationResult(int considered, int written, int skipped, int refused) =>
            PileStrings.ElevationResult.Resolve(_culture, considered, written, skipped, refused);

        /// <summary>Gets the result line of the cut height write.</summary>
        /// <param name="considered">Piles read.</param>
        /// <param name="written">Piles written.</param>
        /// <param name="skipped">Piles skipped.</param>
        /// <param name="refused">Refusals.</param>
        public string CutResult(int considered, int written, int skipped, int refused) =>
            PileStrings.CutResult.Resolve(_culture, considered, written, skipped, refused);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => PileStrings.MoreLine.Resolve(_culture, count);
    }
}
