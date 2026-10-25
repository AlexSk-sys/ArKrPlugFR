namespace ArKrPlugFR.Core.HostChange
{
    /// <summary>What a picked element is, as far as reassigning cares.</summary>
    public enum RebarKind
    {
        /// <summary>A real bar of the project.</summary>
        Bar = 0,

        /// <summary>A bar inside a rebar system.</summary>
        BarInSystem = 1,

        /// <summary>A bar that arrived from IFC: a family instance of the rebar category.</summary>
        IfcBar = 2,

        /// <summary>Anything else that happened to be picked.</summary>
        Other = 3
    }

    /// <summary>What the tool does with an element of a kind.</summary>
    public enum HostChangeAction
    {
        /// <summary>Change the host of the bar.</summary>
        ChangeHost = 0,

        /// <summary>Write the host's mark into the bar.</summary>
        UpdateIfcMark = 1,

        /// <summary>Leave the element alone.</summary>
        Skip = 2
    }

    /// <summary>
    /// The rule that turns the kind of a picked element into the action of the run.
    /// <para>
    /// A real bar can be re-hosted; a bar inside a system cannot, because the platform
    /// gives systems no host setter; a bar from IFC is not a real bar of the project and
    /// only carries the mark; anything else is left alone. Keeping the decision out of
    /// the Revit layer makes it a rule that can be tested without a running Revit.
    /// </para>
    /// </summary>
    public static class HostChangeRules
    {
        /// <summary>Reads the action for one kind.</summary>
        /// <param name="kind">The kind of the picked element.</param>
        /// <returns>What the run does with it.</returns>
        public static HostChangeAction Route(RebarKind kind)
        {
            switch (kind)
            {
                case RebarKind.Bar:
                    return HostChangeAction.ChangeHost;

                case RebarKind.IfcBar:
                    return HostChangeAction.UpdateIfcMark;

                default:
                    return HostChangeAction.Skip;
            }
        }
    }
}
