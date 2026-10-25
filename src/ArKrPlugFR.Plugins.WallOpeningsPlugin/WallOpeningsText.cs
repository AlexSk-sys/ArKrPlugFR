using System.Globalization;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class WallOpeningsText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public WallOpeningsText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => WallOpeningsStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => WallOpeningsStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the doors switch.</summary>
        public string DoorsOption => WallOpeningsStrings.DoorsOption.Resolve(_culture);

        /// <summary>Gets the caption of the windows switch.</summary>
        public string WindowsOption => WallOpeningsStrings.WindowsOption.Resolve(_culture);

        /// <summary>Gets the hint under the switches.</summary>
        public string OptionsHint => WallOpeningsStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the apply button.</summary>
        public string Apply => WallOpeningsStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => WallOpeningsStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => WallOpeningsStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => WallOpeningsStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when no wall is selected.</summary>
        public string NothingSelected => WallOpeningsStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the message shown when the walls carry no doors or windows.</summary>
        public string NoOpenings => WallOpeningsStrings.NoOpenings.Resolve(_culture);

        /// <summary>Gets the reason a door or window could not be measured.</summary>
        public string NoSize => WallOpeningsStrings.NoSize.Resolve(_culture);

        /// <summary>Gets the line that names the walls in the selection.</summary>
        /// <param name="count">Number of walls.</param>
        public string SelectionLine(int count) =>
            WallOpeningsStrings.SelectionLine.Resolve(_culture, Number(count));

        /// <summary>Gets the summary line of a finished run.</summary>
        /// <param name="cut">Openings that were cut.</param>
        /// <param name="alreadyThere">Openings that were already in place.</param>
        /// <param name="skipped">Doors and windows that were skipped.</param>
        public string Result(int cut, int alreadyThere, int skipped) =>
            WallOpeningsStrings.Result.Resolve(
                _culture,
                Number(cut),
                Number(alreadyThere),
                Number(skipped));

        /// <summary>Gets the line that reports a wall the opening was refused on.</summary>
        /// <param name="wallName">Name of the wall.</param>
        /// <param name="reason">What went wrong.</param>
        public string OpeningFailed(string wallName, string reason) =>
            WallOpeningsStrings.OpeningFailed.Resolve(_culture, wallName, reason);

        /// <summary>Gets the note that a curved wall was skipped.</summary>
        /// <param name="wallName">Name of the wall.</param>
        public string WallNotStraight(string wallName) =>
            WallOpeningsStrings.WallNotStraight.Resolve(_culture, wallName);

        /// <summary>Gets the line that reports a door or window that could not be measured.</summary>
        /// <param name="name">Name of the instance.</param>
        /// <param name="reason">Why it could not be read.</param>
        public string OpeningNotRead(string name, string reason) =>
            WallOpeningsStrings.OpeningNotRead.Resolve(_culture, name, reason);

        /// <summary>Gets the line that reports a run that failed as a whole.</summary>
        /// <param name="reason">What went wrong.</param>
        public string RunFailed(string reason) => WallOpeningsStrings.RunFailed.Resolve(_culture, reason);

        /// <summary>Gets the line that says how many report lines were left out.</summary>
        /// <param name="count">Number of left-out lines.</param>
        public string MoreLine(int count) => WallOpeningsStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the line that names the log folder.</summary>
        /// <param name="folder">Path of the log folder.</param>
        public string LogFolder(string folder) => WallOpeningsStrings.LogFolder.Resolve(_culture, folder);

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
