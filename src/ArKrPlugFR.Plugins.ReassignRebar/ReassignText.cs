namespace ArKrPlugFR.Plugins.ReassignRebar
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class ReassignText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public ReassignText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => ReassignStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => ReassignStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the prompt of the first pick.</summary>
        public string PickBars => ReassignStrings.PickBars.Resolve(_culture);

        /// <summary>Gets the prompt of the second pick.</summary>
        public string PickHost => ReassignStrings.PickHost.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string Cancelled => ReassignStrings.Cancelled.Resolve(_culture);

        /// <summary>Gets the message shown when no rebar was picked.</summary>
        public string NothingPicked => ReassignStrings.NothingPicked.Resolve(_culture);

        /// <summary>Gets the message shown when the host has no mark.</summary>
        public string HostWithoutMark => ReassignStrings.HostWithoutMark.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => ReassignStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => ReassignStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="reassigned">Bars whose host changed.</param>
        /// <param name="ifcUpdated">IFC bars whose mark was written.</param>
        /// <param name="skipped">Elements that were left alone.</param>
        /// <param name="refused">Refusals.</param>
        public string Result(int reassigned, int ifcUpdated, int skipped, int refused) =>
            ReassignStrings.Result.Resolve(_culture, reassigned, ifcUpdated, skipped, refused);

        /// <summary>Gets the line of one refused bar.</summary>
        /// <param name="element">Description of the bar.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string element, string reason) =>
            ReassignStrings.Refused.Resolve(_culture, element, reason);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => ReassignStrings.MoreLine.Resolve(_culture, count);
    }
}
