using ArKrPlugFR.Core.Worksets;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the engine and the report take this object instead of a culture
    /// code, so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class WorksetText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public WorksetText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => WorksetStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => WorksetStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the rule list.</summary>
        public string RulesCaption => WorksetStrings.RulesCaption.Resolve(_culture);

        /// <summary>Gets the caption of the kind column.</summary>
        public string KindCaption => WorksetStrings.KindCaption.Resolve(_culture);

        /// <summary>Gets the caption of the match column.</summary>
        public string PatternCaption => WorksetStrings.PatternCaption.Resolve(_culture);

        /// <summary>Gets the caption of the workset column.</summary>
        public string TargetCaption => WorksetStrings.TargetCaption.Resolve(_culture);

        /// <summary>Gets the hint that explains what each kind matches.</summary>
        public string KindHint => WorksetStrings.KindHint.Resolve(_culture);

        /// <summary>Gets the caption of the button that adds a rule.</summary>
        public string AddRule => WorksetStrings.AddRule.Resolve(_culture);

        /// <summary>Gets the caption of the button that removes a rule.</summary>
        public string RemoveRule => WorksetStrings.RemoveRule.Resolve(_culture);

        /// <summary>Gets the caption of the switch that allows creating missing worksets.</summary>
        public string CreateMissing => WorksetStrings.CreateMissing.Resolve(_culture);

        /// <summary>Gets the caption of the apply button.</summary>
        public string Apply => WorksetStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => WorksetStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ReportCaption => WorksetStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => WorksetStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the project is not workshared.</summary>
        public string NotWorkshared => WorksetStrings.NotWorkshared.Resolve(_culture);

        /// <summary>Gets the message shown when no usable rule is left.</summary>
        public string NoRules => WorksetStrings.NoRules.Resolve(_culture);

        /// <summary>Gets the four choices of the kind list, in the order of <see cref="WorksetRuleKinds.All"/>.</summary>
        public string[] Kinds => new[]
        {
            WorksetStrings.KindCategory.Resolve(_culture),
            WorksetStrings.KindFamilyPrefix.Resolve(_culture),
            WorksetStrings.KindTypePrefix.Resolve(_culture),
            WorksetStrings.KindParameterValue.Resolve(_culture)
        };

        /// <summary>Gets the line that counts the worksets the run created.</summary>
        /// <param name="created">Worksets created.</param>
        public string CreatedLine(int created) => WorksetStrings.CreatedLine.Resolve(_culture, created);

        /// <summary>Gets the line that counts the outcome of the run.</summary>
        /// <param name="considered">Elements read.</param>
        /// <param name="assigned">Elements put into their workset.</param>
        /// <param name="alreadyInTarget">Elements that were already in place.</param>
        /// <param name="noMatch">Elements without a matching rule.</param>
        /// <param name="worksetMissing">Elements whose workset does not exist and may not be created.</param>
        /// <param name="failed">Refusals.</param>
        public string Result(int considered, int assigned, int alreadyInTarget, int noMatch, int worksetMissing, int failed) =>
            WorksetStrings.Result.Resolve(_culture, considered, assigned, alreadyInTarget, noMatch, worksetMissing, failed);

        /// <summary>Gets the line that counts the failure lines left out of the report.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => WorksetStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the line of an element the platform refused to change.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="reason">What the platform reported.</param>
        public string AssignmentFailed(string element, string reason) =>
            WorksetStrings.AssignmentFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line of an element the platform refused without a reason.</summary>
        /// <param name="element">Description of the element.</param>
        public string AssignmentRefused(string element) => WorksetStrings.AssignmentRefused.Resolve(_culture, element);

        /// <summary>Gets the line of a workset that could not be created.</summary>
        /// <param name="workset">Name of the workset.</param>
        /// <param name="reason">What the platform reported.</param>
        public string CreateFailed(string workset, string reason) =>
            WorksetStrings.CreateFailed.Resolve(_culture, workset, reason);

        /// <summary>Gets the line of a name that is taken but cannot be resolved.</summary>
        /// <param name="workset">Name of the workset.</param>
        public string NotUnique(string workset) => WorksetStrings.NotUnique.Resolve(_culture, workset);
    }
}
