using ArKrPlugFR.Core.Marking;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the engine take this object instead of a culture code, so a message
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class MarkText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public MarkText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => MarkStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => MarkStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the switch that protects existing marks.</summary>
        public string OnlyEmpty => MarkStrings.OnlyEmpty.Resolve(_culture);

        /// <summary>Gets the caption of the mark code section.</summary>
        public string MarksCaption => MarkStrings.MarksCaption.Resolve(_culture);

        /// <summary>Gets the caption of the material suffix section.</summary>
        public string SuffixesCaption => MarkStrings.SuffixesCaption.Resolve(_culture);

        /// <summary>Gets the caption of the concrete suffix field.</summary>
        public string ConcreteSuffixLabel => MarkStrings.ConcreteSuffixLabel.Resolve(_culture);

        /// <summary>Gets the caption of the steel suffix field.</summary>
        public string SteelSuffixLabel => MarkStrings.SteelSuffixLabel.Resolve(_culture);

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => MarkStrings.Hint.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => MarkStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => MarkStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => MarkStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => MarkStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when every mark code is empty.</summary>
        public string MarksRequired => MarkStrings.MarksRequired.Resolve(_culture);

        /// <summary>Gets the caption of a kind.</summary>
        /// <param name="kind">The kind of structure.</param>
        public string LabelOf(StructureKind kind) => MarkStrings.LabelOf(kind).Resolve(_culture);

        /// <summary>Gets the line of an element whose kind has an empty code.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="kind">The kind of the element.</param>
        public string NoMarkCode(string element, StructureKind kind) =>
            MarkStrings.NoMarkCode.Resolve(_culture, element, LabelOf(kind));

        /// <summary>Gets the line of an element whose mark parameter cannot receive the mark.</summary>
        /// <param name="element">Description of the element.</param>
        public string NoTargetParameter(string element) => MarkStrings.NoTargetParameter.Resolve(_culture, element);

        /// <summary>Gets the line of a mark the platform refused.</summary>
        /// <param name="element">Description of the element.</param>
        public string WriteRefused(string element) => MarkStrings.WriteRefused.Resolve(_culture, element);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string element, string reason) => MarkStrings.WriteFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="considered">Elements read.</param>
        /// <param name="marked">Elements marked.</param>
        /// <param name="alreadyMarked">Elements that already carried a mark.</param>
        /// <param name="noKind">Elements without a kind.</param>
        /// <param name="noMarkCode">Elements whose kind has an empty code.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(int considered, int marked, int alreadyMarked, int noKind, int noMarkCode, int failed) =>
            MarkStrings.Result.Resolve(_culture, considered, marked, alreadyMarked, noKind, noMarkCode, failed);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => MarkStrings.MoreLine.Resolve(_culture, count);
    }
}
