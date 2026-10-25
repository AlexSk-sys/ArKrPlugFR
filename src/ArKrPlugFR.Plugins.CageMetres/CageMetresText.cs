using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class CageMetresText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public CageMetresText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => CageMetresStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => CageMetresStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the layout lines.</summary>
        public string LinesLabel => CageMetresStrings.LinesLabel.Resolve(_culture);

        /// <summary>Gets the caption of the cage.</summary>
        public string CageLabel => CageMetresStrings.CageLabel.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the lines.</summary>
        public string PickLines => CageMetresStrings.PickLines.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the cage.</summary>
        public string PickCage => CageMetresStrings.PickCage.Resolve(_culture);

        /// <summary>Gets the state shown before any line is picked.</summary>
        public string LinesNone => CageMetresStrings.LinesNone.Resolve(_culture);

        /// <summary>Gets the state shown before a cage is picked.</summary>
        public string CageNone => CageMetresStrings.CageNone.Resolve(_culture);

        /// <summary>Gets the hint about the parameter names.</summary>
        public string OptionsHint => CageMetresStrings.OptionsHint.Resolve(_culture);

        /// <summary>Gets the caption of the calculated-length parameter field.</summary>
        public string ResultParameterLabel => CageMetresStrings.ResultParameterLabel.Resolve(_culture);

        /// <summary>Gets the caption of the write switch.</summary>
        public string WriteSwitch => CageMetresStrings.WriteSwitch.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => CageMetresStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => CageMetresStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => CageMetresStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => CageMetresStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the prompt of the line pick.</summary>
        public string PickLinesPrompt => CageMetresStrings.PickLinesPrompt.Resolve(_culture);

        /// <summary>Gets the prompt of the cage pick.</summary>
        public string PickCagePrompt => CageMetresStrings.PickCagePrompt.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string PickCancelled => CageMetresStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the message shown when no line was picked.</summary>
        public string NoLines => CageMetresStrings.NoLines.Resolve(_culture);

        /// <summary>Gets the message shown when no cage was picked.</summary>
        public string NoCage => CageMetresStrings.NoCage.Resolve(_culture);

        /// <summary>Gets the caption of the warnings block.</summary>
        public string WarningsCaption => CageMetresStrings.WarningsCaption.Resolve(_culture);

        /// <summary>Formats a number of metres the way the report shows it.</summary>
        /// <param name="metres">Running metres.</param>
        /// <returns>The value with two decimals.</returns>
        public static string Metres(double metres)
        {
            return metres.ToString("0.00", CultureInfo.InvariantCulture);
        }

        /// <summary>Gets the line that names the cage.</summary>
        /// <param name="family">Family name of the cage.</param>
        public string CageLine(string family) => CageMetresStrings.CageLine.Resolve(_culture, family);

        /// <summary>Gets the line that counts the picked lines.</summary>
        /// <param name="count">Number of lines.</param>
        /// <param name="metres">Their total length in metres.</param>
        public string LinesLine(int count, double metres) =>
            CageMetresStrings.LinesLine.Resolve(_culture, count, Metres(metres));

        /// <summary>Gets the line of the longitudinal group.</summary>
        /// <param name="bars">Number of longitudinal bars.</param>
        /// <param name="lengthMetres">Cage length in metres.</param>
        /// <param name="metres">Running metres of the group.</param>
        public string LongitudinalLine(int bars, double lengthMetres, double metres) =>
            CageMetresStrings.LongitudinalLine.Resolve(_culture, bars, Metres(lengthMetres), Metres(metres));

        /// <summary>Gets the line of the inclined group.</summary>
        /// <param name="positions">Number of positions.</param>
        /// <param name="barMetres">Length of one inclined bar in metres.</param>
        /// <param name="metres">Running metres of the group.</param>
        public string InclinedLine(int positions, double barMetres, double metres) =>
            CageMetresStrings.InclinedLine.Resolve(_culture, positions, Metres(barMetres), Metres(metres));

        /// <summary>Gets the line of the transverse group.</summary>
        /// <param name="positions">Number of positions.</param>
        /// <param name="lengths">The distinct bar lengths, in metres.</param>
        /// <param name="metres">Running metres of the group.</param>
        public string TransverseLine(int positions, IEnumerable<double> lengths, double metres) =>
            CageMetresStrings.TransverseLine.Resolve(
                _culture,
                positions,
                string.Join(" + ", lengths.Select(Metres)),
                Metres(metres));

        /// <summary>Gets the line with the total.</summary>
        /// <param name="metres">Total running metres.</param>
        public string TotalLine(double metres) =>
            CageMetresStrings.TotalLine.Resolve(_culture, Metres(metres));

        /// <summary>Gets the line that reports where the total went.</summary>
        /// <param name="parameter">Name of the parameter written into.</param>
        public string Written(string parameter) => CageMetresStrings.Written.Resolve(_culture, parameter);

        /// <summary>Gets the message shown when writing is switched off.</summary>
        public string WriteOff => CageMetresStrings.WriteOff.Resolve(_culture);

        /// <summary>Gets the message shown when the parameter does not exist.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string NoParameter(string parameter) => CageMetresStrings.NoParameter.Resolve(_culture, parameter);

        /// <summary>Gets the message shown when the parameter is read-only.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string ReadOnly(string parameter) => CageMetresStrings.ReadOnly.Resolve(_culture, parameter);

        /// <summary>Gets the message shown when the parameter cannot hold a number.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string NotANumber(string parameter) => CageMetresStrings.NotANumber.Resolve(_culture, parameter);

        /// <summary>Gets the message shown when the write was refused.</summary>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string reason) => CageMetresStrings.WriteFailed.Resolve(_culture, reason);

        /// <summary>Gets the warning about the number of longitudinal bars.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string MissingCount(string parameter) => CageMetresStrings.MissingCount.Resolve(_culture, parameter);

        /// <summary>Gets the warning about a step.</summary>
        /// <param name="parameter">Name of the parameter.</param>
        public string MissingStep(string parameter) => CageMetresStrings.MissingStep.Resolve(_culture, parameter);

        /// <summary>Gets the warning about the inclined bar length.</summary>
        public string MissingInclinedLength => CageMetresStrings.MissingInclinedLength.Resolve(_culture);

        /// <summary>Gets the warning about the transverse bar lengths.</summary>
        public string MissingTransverseLength => CageMetresStrings.MissingTransverseLength.Resolve(_culture);

        /// <summary>Gets the warning about sub-components without a flag.</summary>
        /// <param name="count">How many were left out.</param>
        public string Unclassified(int count) => CageMetresStrings.Unclassified.Resolve(_culture, count);

        /// <summary>Gets the warning about a cage without sub-components.</summary>
        public string NoSubComponents => CageMetresStrings.NoSubComponents.Resolve(_culture);
    }
}
