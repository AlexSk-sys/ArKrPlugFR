using System.Globalization;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the tree rows and the run take this object instead of a culture
    /// code, so a label is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class DuplicateViewsText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public DuplicateViewsText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => DuplicateViewsStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => DuplicateViewsStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the views tree.</summary>
        public string ViewsHeader => DuplicateViewsStrings.ViewsHeader.Resolve(_culture);

        /// <summary>Gets the caption of the schedules tree.</summary>
        public string SchedulesHeader => DuplicateViewsStrings.SchedulesHeader.Resolve(_culture);

        /// <summary>Gets the caption of the templates tree.</summary>
        public string TemplatesHeader => DuplicateViewsStrings.TemplatesHeader.Resolve(_culture);

        /// <summary>Gets the hint of the tree search field.</summary>
        public string SearchHint => DuplicateViewsStrings.SearchHint.Resolve(_culture);

        /// <summary>Gets the hint about ticking rows and Shift multi-selection.</summary>
        public string TreeHint => DuplicateViewsStrings.TreeHint.Resolve(_culture);

        /// <summary>Gets the caption of the template section.</summary>
        public string TemplateSection => DuplicateViewsStrings.TemplateSection.Resolve(_culture);

        /// <summary>Gets the hint of the template search field.</summary>
        public string TemplateSearchHint => DuplicateViewsStrings.TemplateSearchHint.Resolve(_culture);

        /// <summary>Gets the picker entry that applies no template.</summary>
        public string NoTemplateOption => DuplicateViewsStrings.NoTemplateOption.Resolve(_culture);

        /// <summary>Gets the note about what the chosen template is applied to.</summary>
        public string TemplateHint => DuplicateViewsStrings.TemplateHint.Resolve(_culture);

        /// <summary>Gets the caption of the duplicate button.</summary>
        public string Duplicate => DuplicateViewsStrings.Duplicate.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => DuplicateViewsStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => DuplicateViewsStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the message shown when nothing is ticked.</summary>
        public string NothingSelected => DuplicateViewsStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => DuplicateViewsStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the chosen template does not fit.</summary>
        public string TemplateMismatch => DuplicateViewsStrings.TemplateMismatch.Resolve(_culture);

        /// <summary>Gets the caption of the help button.</summary>
        public string Help => DuplicateViewsStrings.Help.Resolve(_culture);

        /// <summary>Gets the title of the help window.</summary>
        public string HelpTitle => DuplicateViewsStrings.HelpTitle.Resolve(_culture);

        /// <summary>Gets the usage instructions shown in the help window.</summary>
        public string HelpText => DuplicateViewsStrings.HelpText.Resolve(_culture);

        /// <summary>Gets the caption of the duplicate button with the number of copies.</summary>
        /// <param name="count">Number of copies the run would make.</param>
        public string DuplicateWithCount(int count) =>
            DuplicateViewsStrings.DuplicateWithCount.Resolve(_culture, count);

        /// <summary>Gets the report line of a finished run.</summary>
        /// <param name="created">Number of copies that were created.</param>
        public string Success(int created) => DuplicateViewsStrings.Success.Resolve(_culture, created);

        /// <summary>Gets the line that reports one copy that failed.</summary>
        /// <param name="name">Name of the element being copied.</param>
        /// <param name="reason">What went wrong.</param>
        public string FailedLine(string name, string reason) =>
            DuplicateViewsStrings.FailedLine.Resolve(_culture, name, reason);

        /// <summary>Gets the line that says how many report lines were left out.</summary>
        /// <param name="count">Number of left-out lines.</param>
        public string MoreLine(int count) => DuplicateViewsStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the line that names the log folder.</summary>
        /// <param name="folder">Path of the log folder.</param>
        public string LogFolder(string folder) => DuplicateViewsStrings.LogFolder.Resolve(_culture, folder);

        /// <summary>Gets the reason reported when the source element has disappeared.</summary>
        public string ElementMissing => DuplicateViewsStrings.ElementMissing.Resolve(_culture);

        /// <summary>Gets the reason reported when the project refuses to copy an element.</summary>
        public string CopyRefused => DuplicateViewsStrings.CopyRefused.Resolve(_culture);

        /// <summary>Gets the label of the group the schedules of one category fall into.</summary>
        /// <param name="category">Name of the category.</param>
        public string GroupCategorySchedulesOf(string category) =>
            DuplicateViewsStrings.GroupCategorySchedulesOf.Resolve(_culture, category);

        /// <summary>Gets the label of the 3D views and walkthroughs group.</summary>
        public string GroupThreeDimensional => DuplicateViewsStrings.GroupThreeDimensional.Resolve(_culture);

        /// <summary>Gets the label of the plans group.</summary>
        public string GroupPlans => DuplicateViewsStrings.GroupPlans.Resolve(_culture);

        /// <summary>Gets the label of the elevations, sections and details group.</summary>
        public string GroupSections => DuplicateViewsStrings.GroupSections.Resolve(_culture);

        /// <summary>Gets the label of the drafting views group.</summary>
        public string GroupDrafting => DuplicateViewsStrings.GroupDrafting.Resolve(_culture);

        /// <summary>Gets the label of the legends group.</summary>
        public string GroupLegends => DuplicateViewsStrings.GroupLegends.Resolve(_culture);

        /// <summary>Gets the label of the group for every other view.</summary>
        public string GroupOtherViews => DuplicateViewsStrings.GroupOtherViews.Resolve(_culture);

        /// <summary>Gets the label of the group for schedules without a definition.</summary>
        public string GroupOtherSchedules => DuplicateViewsStrings.GroupOtherSchedules.Resolve(_culture);

        /// <summary>Gets the label of the schedule templates group.</summary>
        public string GroupScheduleTemplates => DuplicateViewsStrings.GroupScheduleTemplates.Resolve(_culture);

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
