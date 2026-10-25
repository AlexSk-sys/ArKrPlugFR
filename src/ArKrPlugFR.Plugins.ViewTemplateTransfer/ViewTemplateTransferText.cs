using System.Globalization;
using ArKrPlugFR.Core.ViewTransfer;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the view model take this object instead of a culture code,
    /// so a label is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class ViewTemplateTransferText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public ViewTemplateTransferText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => ViewTemplateTransferStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line that names the target project.</summary>
        public string TargetLine(string projectName) =>
            ViewTemplateTransferStrings.TargetLine.Resolve(_culture, projectName);

        /// <summary>Gets the line that names the chosen source project.</summary>
        public string SourceLine(string projectName) =>
            ViewTemplateTransferStrings.SourceLine.Resolve(_culture, projectName);

        /// <summary>Gets the caption of the target list.</summary>
        public string TargetsHeader => ViewTemplateTransferStrings.TargetsHeader.Resolve(_culture);

        /// <summary>Gets the hint shown when no other project is open.</summary>
        public string NoTargetsHint => ViewTemplateTransferStrings.NoTargetsHint.Resolve(_culture);

        /// <summary>Gets the message shown when no target project is chosen.</summary>
        public string NoTargetChosen => ViewTemplateTransferStrings.NoTargetChosen.Resolve(_culture);

        /// <summary>Gets the note that existing views in the target are left alone.</summary>
        public string NeverOverwritten => ViewTemplateTransferStrings.NeverOverwritten.Resolve(_culture);

        /// <summary>Gets the caption of the transfer button.</summary>
        public string Transfer => ViewTemplateTransferStrings.Transfer.Resolve(_culture);

        /// <summary>Gets the caption of the transfer button with the number of ticked views.</summary>
        public string TransferWithCount(int count) =>
            ViewTemplateTransferStrings.TransferWithCount.Resolve(_culture, count);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => ViewTemplateTransferStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the help button.</summary>
        public string Help => ViewTemplateTransferStrings.Help.Resolve(_culture);

        /// <summary>Gets the title of the help window.</summary>
        public string HelpTitle => ViewTemplateTransferStrings.HelpTitle.Resolve(_culture);

        /// <summary>Gets the usage instructions shown in the help window.</summary>
        public string HelpText => ViewTemplateTransferStrings.HelpText.Resolve(_culture);

        /// <summary>Gets the hint about ticking rows and Shift multi-selection.</summary>
        public string TreeHint => ViewTemplateTransferStrings.TreeHint.Resolve(_culture);

        /// <summary>Gets the message shown when nothing is ticked.</summary>
        public string NoSelection => ViewTemplateTransferStrings.NoSelection.Resolve(_culture);

        /// <summary>Gets the message shown when the target project was closed.</summary>
        public string TargetLost => ViewTemplateTransferStrings.TargetLost.Resolve(_culture);

        /// <summary>Gets the message shown when the target project is read-only.</summary>
        public string TargetReadOnly => ViewTemplateTransferStrings.TargetReadOnly.Resolve(_culture);

        /// <summary>Gets the note shown when the source project has nothing to transfer.</summary>
        public string EmptyTree => ViewTemplateTransferStrings.EmptyTree.Resolve(_culture);

        /// <summary>Gets the name of the transfer step in the undo list.</summary>
        public string TransactionName => ViewTemplateTransferStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the caption of the result window.</summary>
        public string ResultCaption => ViewTemplateTransferStrings.ResultCaption.Resolve(_culture);

        /// <summary>Gets the line that reports how many views were transferred.</summary>
        public string SuccessLine(int count) =>
            ViewTemplateTransferStrings.SuccessLine.Resolve(_culture, count);

        /// <summary>Gets the line that reports one view that could not be transferred.</summary>
        public string FailureLine(string viewName, string reason) =>
            ViewTemplateTransferStrings.FailureLine.Resolve(_culture, viewName, reason);

        /// <summary>Gets the line that says how many more failures were left out of the report.</summary>
        public string MoreLine(int count) =>
            ViewTemplateTransferStrings.MoreLine.Resolve(_culture, count);

        public string RolledBackLine =>
            ViewTemplateTransferStrings.RolledBackLine.Resolve(_culture);

        /// <summary>Gets the display name of a transfer group.</summary>
        /// <param name="group">The group to name.</param>
        /// <returns>
        /// The localized catalog name, or the raw view-type name of a group the
        /// catalog does not know.
        /// </returns>
        public string GroupDisplayName(TransferGroup group)
        {
            if (group == null || group.Key == null)
            {
                return group?.FallbackViewTypeName ?? string.Empty;
            }

            switch (group.Key)
            {
                case ViewGroupCatalog.PlansKey:
                    return ViewTemplateTransferStrings.GroupPlans.Resolve(_culture);

                case ViewGroupCatalog.CeilingPlansKey:
                    return ViewTemplateTransferStrings.GroupCeilingPlans.Resolve(_culture);

                case ViewGroupCatalog.ThreeDimensionalKey:
                    return ViewTemplateTransferStrings.GroupThreeDimensional.Resolve(_culture);

                case ViewGroupCatalog.SectionsKey:
                    return ViewTemplateTransferStrings.GroupSections.Resolve(_culture);

                case ViewGroupCatalog.DraftingKey:
                    return ViewTemplateTransferStrings.GroupDrafting.Resolve(_culture);

                case ViewGroupCatalog.EngineeringKey:
                    return ViewTemplateTransferStrings.GroupEngineering.Resolve(_culture);

                case ViewGroupCatalog.SchedulesKey:
                    return ViewTemplateTransferStrings.GroupSchedules.Resolve(_culture);

                default:
                    return group.Key;
            }
        }

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
