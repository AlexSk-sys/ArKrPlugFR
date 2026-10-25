namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class AlignmentText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public AlignmentText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => AlignmentStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => AlignmentStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the target.</summary>
        public string TargetLabel => AlignmentStrings.TargetLabel.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the target.</summary>
        public string PickTarget => AlignmentStrings.PickTarget.Resolve(_culture);

        /// <summary>Gets the caption of the elements to move.</summary>
        public string MembersLabel => AlignmentStrings.MembersLabel.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the elements.</summary>
        public string PickMembers => AlignmentStrings.PickMembers.Resolve(_culture);

        /// <summary>Gets the caption of the nodes to move.</summary>
        public string NodesLabel => AlignmentStrings.NodesLabel.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the nodes.</summary>
        public string PickNodes => AlignmentStrings.PickNodes.Resolve(_culture);

        /// <summary>Gets the caption of the member alignment button.</summary>
        public string AlignMembers => AlignmentStrings.AlignMembers.Resolve(_culture);

        /// <summary>Gets the caption of the node alignment button.</summary>
        public string AlignNodes => AlignmentStrings.AlignNodes.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => AlignmentStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => AlignmentStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the member alignment in the undo list.</summary>
        public string MembersTransaction => AlignmentStrings.MembersTransaction.Resolve(_culture);

        /// <summary>Gets the name of the node alignment in the undo list.</summary>
        public string NodesTransaction => AlignmentStrings.NodesTransaction.Resolve(_culture);

        /// <summary>Gets the state shown before a target is picked.</summary>
        public string TargetNone => AlignmentStrings.TargetNone.Resolve(_culture);

        /// <summary>Gets the state shown before elements are picked.</summary>
        public string MembersNone => AlignmentStrings.MembersNone.Resolve(_culture);

        /// <summary>Gets the state shown before nodes are picked.</summary>
        public string NodesNone => AlignmentStrings.NodesNone.Resolve(_culture);

        /// <summary>Gets the prompt of the target pick.</summary>
        public string PickTargetPrompt => AlignmentStrings.PickTargetPrompt.Resolve(_culture);

        /// <summary>Gets the prompt of the element pick.</summary>
        public string PickMembersPrompt => AlignmentStrings.PickMembersPrompt.Resolve(_culture);

        /// <summary>Gets the prompt of the node pick.</summary>
        public string PickNodesPrompt => AlignmentStrings.PickNodesPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when a pick was cancelled.</summary>
        public string PickCancelled => AlignmentStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the message shown when no target was picked.</summary>
        public string NoTarget => AlignmentStrings.NoTarget.Resolve(_culture);

        /// <summary>Gets the message shown when no element was picked.</summary>
        public string NoMembers => AlignmentStrings.NoMembers.Resolve(_culture);

        /// <summary>Gets the message shown when no node was picked.</summary>
        public string NoNodes => AlignmentStrings.NoNodes.Resolve(_culture);

        /// <summary>Gets the message shown for a target without a location line.</summary>
        public string NoTargetLine => AlignmentStrings.NoTargetLine.Resolve(_culture);

        /// <summary>Gets the message shown when everything is already aligned.</summary>
        public string NothingMoved => AlignmentStrings.NothingMoved.Resolve(_culture);

        /// <summary>Gets the line that counts the member alignment.</summary>
        /// <param name="moved">Elements that were moved.</param>
        /// <param name="inPlace">Elements that were already in place.</param>
        /// <param name="skipped">Elements that were skipped.</param>
        /// <param name="refused">Refusals.</param>
        public string MembersResultLine(int moved, int inPlace, int skipped, int refused) =>
            AlignmentStrings.MembersResult.Resolve(_culture, moved, inPlace, skipped, refused);

        /// <summary>Gets the line that counts the node alignment.</summary>
        /// <param name="moved">Nodes that were moved.</param>
        /// <param name="inPlace">Nodes that were already in place.</param>
        /// <param name="skipped">Elements that were skipped.</param>
        /// <param name="refused">Refusals.</param>
        /// <param name="dragged">Member ends that were dragged along.</param>
        public string NodesResultLine(int moved, int inPlace, int skipped, int refused, int dragged) =>
            AlignmentStrings.NodesResult.Resolve(_culture, moved, inPlace, skipped, refused, dragged);

        /// <summary>Gets the line that reports an element without a location line.</summary>
        /// <param name="element">Description of the element.</param>
        public string SkippedNoLine(string element) => AlignmentStrings.SkippedNoLine.Resolve(_culture, element);

        /// <summary>Gets the line that reports an element that is not a node.</summary>
        /// <param name="element">Description of the element.</param>
        public string SkippedNotANode(string element) => AlignmentStrings.SkippedNotANode.Resolve(_culture, element);

        /// <summary>Gets the line that reports a refusal.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string element, string reason) => AlignmentStrings.Refused.Resolve(_culture, element, reason);
    }
}
