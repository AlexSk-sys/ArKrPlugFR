using System.Globalization;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Plugins.HoleCuttingPlugin
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The command and the cut take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class HoleCuttingText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public HoleCuttingText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => HoleCuttingStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => HoleCuttingStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the line that says where the wall and the service came from.</summary>
        public string SelectionLine => HoleCuttingStrings.SelectionLine.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => HoleCuttingStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => HoleCuttingStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when nothing usable is selected.</summary>
        public string NothingSelected => HoleCuttingStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the message shown when the wall is curved.</summary>
        public string CurvedWall => HoleCuttingStrings.CurvedWall.Resolve(_culture);

        /// <summary>Gets the message shown when the service misses the wall.</summary>
        public string NoCrossing => HoleCuttingStrings.NoCrossing.Resolve(_culture);

        /// <summary>Gets the message shown when the crossing is too small to cut.</summary>
        public string TooSmall => HoleCuttingStrings.TooSmall.Resolve(_culture);

        /// <summary>Gets the message that tells what the selection held instead.</summary>
        /// <param name="walls">Walls in the selection.</param>
        /// <param name="services">Service elements in the selection.</param>
        /// <param name="others">Other elements in the selection.</param>
        public string WrongSelection(int walls, int services, int others) =>
            HoleCuttingStrings.WrongSelection.Resolve(
                _culture,
                HoleCuttingStrings.FoundCounts.Resolve(_culture, walls, services, others));

        /// <summary>Gets the message shown when the platform refuses the opening.</summary>
        /// <param name="reason">What the platform reported.</param>
        public string CutFailed(string reason) => HoleCuttingStrings.CutFailed.Resolve(_culture, reason);

        /// <summary>Gets the success message with both sizes and the wall name.</summary>
        /// <param name="opening">Rectangle of the opening.</param>
        /// <param name="crossing">Rectangle of the crossing.</param>
        /// <param name="wallName">Name of the wall that was cut.</param>
        public string Success(OpeningRectangle opening, OpeningRectangle crossing, string wallName) =>
            HoleCuttingStrings.Success.Resolve(
                _culture,
                Size(opening.Width),
                Size(opening.Height),
                wallName,
                Size(crossing.Width),
                Size(crossing.Height));

        /// <summary>Gets the line that names the log folder.</summary>
        /// <param name="folder">Path of the log folder.</param>
        public string LogFolder(string folder) => HoleCuttingStrings.LogFolder.Resolve(_culture, folder);

        /// <summary>Formats a size in millimetres the same way in every language.</summary>
        /// <param name="size">Size to format.</param>
        /// <returns>Whole millimetres, without a decimal tail when there is none.</returns>
        public static string Size(Length size)
        {
            return size.Millimeters.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
