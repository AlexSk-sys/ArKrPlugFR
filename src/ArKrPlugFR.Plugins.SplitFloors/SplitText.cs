namespace ArKrPlugFR.Plugins.SplitFloors
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the engine and the report take this object instead of a culture
    /// code, so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class SplitText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SplitText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SplitStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => SplitStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the picked floor list.</summary>
        public string FloorsCaption => SplitStrings.FloorsCaption.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks floors.</summary>
        public string PickFloors => SplitStrings.PickFloors.Resolve(_culture);

        /// <summary>Gets the caption of the button that clears the list.</summary>
        public string Clear => SplitStrings.Clear.Resolve(_culture);

        /// <summary>Gets the prompt shown while the user picks floors.</summary>
        public string PickPrompt => SplitStrings.PickPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => SplitStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the hint under the list.</summary>
        public string OptionsHint => SplitStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the apply button.</summary>
        public string Apply => SplitStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SplitStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SplitStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of one split in the undo list.</summary>
        public string TransactionName => SplitStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was picked.</summary>
        public string NoFloors => SplitStrings.NoFloors.Resolve(_culture);

        /// <summary>Gets the reason used when the source floor could not be deleted.</summary>
        public string DeleteRefused => SplitStrings.DeleteRefused.Resolve(_culture);

        /// <summary>Gets the line that counts the picked floors.</summary>
        /// <param name="floors">Floors picked.</param>
        public string FloorsLine(int floors) => SplitStrings.FloorsLine.Resolve(_culture, floors);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="considered">Floors read.</param>
        /// <param name="split">Floors split.</param>
        /// <param name="created">Floors created.</param>
        /// <param name="skipped">Floors skipped.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(int considered, int split, int created, int skipped, int failed) =>
            SplitStrings.Result.Resolve(_culture, considered, split, created, skipped, failed);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => SplitStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the line of a floor without a flat top face.</summary>
        /// <param name="floor">Description of the floor.</param>
        public string NoTopFace(string floor) => SplitStrings.NoTopFace.Resolve(_culture, floor);

        /// <summary>Gets the line of a floor with a single contour.</summary>
        /// <param name="floor">Description of the floor.</param>
        public string SingleContour(string floor) => SplitStrings.SingleContour.Resolve(_culture, floor);

        /// <summary>Gets the line that counts the contours left out of a floor.</summary>
        /// <param name="floor">Description of the floor.</param>
        /// <param name="degenerate">Degenerate contours.</param>
        /// <param name="nested">Contours inside an opening.</param>
        /// <param name="duplicates">Duplicate contours.</param>
        public string RejectedContours(string floor, int degenerate, int nested, int duplicates) =>
            SplitStrings.RejectedContours.Resolve(_culture, floor, degenerate, nested, duplicates);

        /// <summary>Gets the line of a floor that could not be split.</summary>
        /// <param name="floor">Description of the floor.</param>
        /// <param name="reason">What the platform reported.</param>
        public string SplitFailed(string floor, string reason) =>
            SplitStrings.SplitFailed.Resolve(_culture, floor, reason);
    }
}
