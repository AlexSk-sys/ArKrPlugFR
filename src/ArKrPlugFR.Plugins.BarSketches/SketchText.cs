namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class SketchText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SketchText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SketchStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => SketchStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the bars.</summary>
        public string BarsLabel => SketchStrings.BarsLabel.Resolve(_culture);

        /// <summary>Gets the caption of the bar pick button.</summary>
        public string PickBars => SketchStrings.PickBars.Resolve(_culture);

        /// <summary>Gets the state shown before bars are picked.</summary>
        public string BarsNone => SketchStrings.BarsNone.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => SketchStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SketchStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SketchStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => SketchStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the prompt of the bar pick.</summary>
        public string PickBarsPrompt => SketchStrings.PickBarsPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string PickCancelled => SketchStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the message shown when no bar was picked.</summary>
        public string NoBars => SketchStrings.NoBars.Resolve(_culture);

        /// <summary>Gets the message shown when the project has no title block.</summary>
        public string NoTitleBlock => SketchStrings.NoTitleBlock.Resolve(_culture);

        /// <summary>Gets the message shown when the project has no 3D view family.</summary>
        public string NoViewFamily => SketchStrings.NoViewFamily.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was made.</summary>
        public string NothingMade => SketchStrings.NothingMade.Resolve(_culture);

        /// <summary>Gets the word used for a fact that could not be read.</summary>
        public string Unknown => SketchStrings.Unknown.Resolve(_culture);

        /// <summary>Gets the line that counts the run.</summary>
        /// <param name="made">Sheets that were made.</param>
        /// <param name="bars">Bars considered.</param>
        /// <param name="skipped">Bars that were skipped.</param>
        /// <param name="refused">Refusals.</param>
        public string ResultLine(int made, int bars, int skipped, int refused) =>
            SketchStrings.Result.Resolve(_culture, made, bars, skipped, refused);

        /// <summary>Gets the line that reports how many bars are left.</summary>
        /// <param name="remaining">Bars the next run will take.</param>
        public string RemainingLine(int remaining) => SketchStrings.Remaining.Resolve(_culture, remaining);

        /// <summary>Gets the line of one made sheet.</summary>
        /// <param name="view">Name of the view.</param>
        /// <param name="number">Number of the sheet.</param>
        public string SheetLine(string view, string number) => SketchStrings.SheetLine.Resolve(_culture, view, number);

        /// <summary>Gets the line that reports a bar without a bounding box.</summary>
        /// <param name="bar">Description of the bar.</param>
        public string NoBox(string bar) => SketchStrings.NoBox.Resolve(_culture, bar);

        /// <summary>Gets the line that reports a refusal.</summary>
        /// <param name="bar">Description of the bar.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string bar, string reason) => SketchStrings.Refused.Resolve(_culture, bar, reason);

        /// <summary>Builds the note written on a sheet.</summary>
        /// <param name="shape">Name of the shape, or the word for an unknown fact.</param>
        /// <param name="diameterMillimeters">Bar diameter in millimetres, or zero when unknown.</param>
        /// <param name="lengthMillimeters">Bar length in millimetres, or zero when unknown.</param>
        /// <param name="mark">Schedule mark of the bar, or the word for an unknown fact.</param>
        public string Note(string shape, double diameterMillimeters, double lengthMillimeters, string mark)
        {
            string shapeValue = string.IsNullOrWhiteSpace(shape) ? Unknown : shape.Trim();
            string markValue = string.IsNullOrWhiteSpace(mark) ? Unknown : mark.Trim();

            string diameterValue = diameterMillimeters > 0
                ? ArKrPlugFR.Core.Sketches.SketchRules.Millimeters(diameterMillimeters) + " mm"
                : Unknown;

            string lengthValue = lengthMillimeters > 0
                ? ArKrPlugFR.Core.Sketches.SketchRules.Millimeters(lengthMillimeters) + " mm"
                : Unknown;

            return SketchStrings.ShapeLabel.Resolve(_culture) + ": " + shapeValue + "\n"
                + SketchStrings.DiameterLabel.Resolve(_culture) + ": " + diameterValue + "\n"
                + SketchStrings.LengthLabel.Resolve(_culture) + ": " + lengthValue + "\n"
                + SketchStrings.MarkLabel.Resolve(_culture) + ": " + markValue;
        }
    }
}
