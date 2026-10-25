namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The command, the window and the cut take this object instead of a culture code,
    /// so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class CutHolesText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public CutHolesText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => CutHolesStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => CutHolesStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the hint that explains where the equipment comes from.</summary>
        public string LinkLine => CutHolesStrings.LinkLine.Resolve(_culture);

        /// <summary>Gets the caption of the clearance field.</summary>
        public string ClearanceLabel => CutHolesStrings.ClearanceLabel.Resolve(_culture);

        /// <summary>Gets the hint under the clearance field.</summary>
        public string OptionsHint => CutHolesStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the apply button.</summary>
        public string Apply => CutHolesStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => CutHolesStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => CutHolesStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => CutHolesStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the prompt shown while the user picks a link.</summary>
        public string PickPrompt => CutHolesStrings.PickPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => CutHolesStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the message shown when the clearance cannot be read.</summary>
        public string ClearanceRequired => CutHolesStrings.ClearanceRequired.Resolve(_culture);

        /// <summary>Gets the message shown when the link has no document.</summary>
        public string LinkNotLoaded => CutHolesStrings.LinkNotLoaded.Resolve(_culture);

        /// <summary>Gets the message shown when the link holds nothing to cut from.</summary>
        public string NoEquipment => CutHolesStrings.NoEquipment.Resolve(_culture);

        /// <summary>Gets the message shown when the project holds no concrete.</summary>
        public string NoConcrete => CutHolesStrings.NoConcrete.Resolve(_culture);

        /// <summary>Gets the message that tells what the run started from.</summary>
        /// <param name="equipment">Equipment elements read from the link.</param>
        /// <param name="hosts">Concrete elements found in the project.</param>
        public string Counts(int equipment, int hosts) =>
            CutHolesStrings.Counts.Resolve(_culture, equipment, hosts);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="cut">Openings that were cut.</param>
        /// <param name="alreadyThere">Openings that were already present.</param>
        /// <param name="notThrough">Equipment that does not pass through.</param>
        /// <param name="tooSmall">Crossings too small to cut.</param>
        /// <param name="refused">Openings the platform refused.</param>
        public string Result(int cut, int alreadyThere, int notThrough, int tooSmall, int refused) =>
            CutHolesStrings.Result.Resolve(_culture, cut, alreadyThere, notThrough, tooSmall, refused);

        /// <summary>Gets the line of one refused opening.</summary>
        /// <param name="element">Description of the element the opening belongs to.</param>
        /// <param name="reason">What the platform reported.</param>
        public string CutFailed(string element, string reason) =>
            CutHolesStrings.CutFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line of a wall that cannot be read as a frame.</summary>
        /// <param name="element">Description of the wall.</param>
        public string WallNotStraight(string element) =>
            CutHolesStrings.WallNotStraight.Resolve(_culture, element);

        /// <summary>Gets the line of a column that is not a family instance.</summary>
        /// <param name="element">Description of the column.</param>
        public string NotAFamilyInstance(string element) =>
            CutHolesStrings.NotAFamilyInstance.Resolve(_culture, element);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => CutHolesStrings.MoreLine.Resolve(_culture, count);
    }
}
