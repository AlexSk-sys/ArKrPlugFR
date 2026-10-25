using ArKrPlugFR.Core.Visibility;

namespace ArKrPlugFR.Plugins.AnalyticalVisibility
{
    /// <summary>
    /// The strings of the tool resolved for one language and one action.
    /// </summary>
    public sealed class VisibilityText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public VisibilityText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the title of the command.</summary>
        /// <param name="action">Whether the analytical model is being hidden or shown.</param>
        public string Title(VisibilityAction action) =>
            (action == VisibilityAction.Hide ? VisibilityStrings.HideTitle : VisibilityStrings.ShowTitle)
                .Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => VisibilityStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        /// <param name="action">Whether the analytical model is being hidden or shown.</param>
        public string TransactionName(VisibilityAction action) =>
            (action == VisibilityAction.Hide ? VisibilityStrings.HideTransaction : VisibilityStrings.ShowTransaction)
                .Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => VisibilityStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the message shown when the project has no analytical model.</summary>
        public string NoAnalyticalModel => VisibilityStrings.NoAnalyticalModel.Resolve(_culture);

        /// <summary>Gets the message shown when the project has no model views.</summary>
        public string NoViews => VisibilityStrings.NoViews.Resolve(_culture);

        /// <summary>Gets the message shown when everything is already in the asked-for state.</summary>
        public string NothingToDo => VisibilityStrings.NothingToDo.Resolve(_culture);

        /// <summary>Gets the note that nothing was deleted.</summary>
        public string Safe => VisibilityStrings.Safe.Resolve(_culture);

        /// <summary>Gets the line that counts the run.</summary>
        /// <param name="elements">Analytical elements considered.</param>
        /// <param name="views">Views considered.</param>
        /// <param name="changed">Elements that changed state.</param>
        /// <param name="inPlace">Elements that were already in the asked-for state.</param>
        /// <param name="cannot">Elements the platform does not let change.</param>
        /// <param name="refused">Refusals.</param>
        public string Result(int elements, int views, int changed, int inPlace, int cannot, int refused) =>
            VisibilityStrings.Result.Resolve(_culture, elements, views, changed, inPlace, cannot, refused);

        /// <summary>Gets the line of one refused element.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="view">Name of the view.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string element, string view, string reason) =>
            VisibilityStrings.Refused.Resolve(_culture, element, view, reason);

        /// <summary>Gets the line that counts the refusals left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => VisibilityStrings.MoreLine.Resolve(_culture, count);
    }
}
