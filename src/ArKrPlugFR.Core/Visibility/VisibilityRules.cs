using System;

namespace ArKrPlugFR.Core.Visibility
{
    /// <summary>The kinds of view a model is looked at in.</summary>
    public enum ModelViewKind
    {
        /// <summary>A three-dimensional view.</summary>
        ThreeDimensional,

        /// <summary>A floor plan.</summary>
        FloorPlan,

        /// <summary>A ceiling plan.</summary>
        CeilingPlan,

        /// <summary>A section.</summary>
        Section,

        /// <summary>An elevation.</summary>
        Elevation,

        /// <summary>Anything else: a schedule, a sheet, a drafting view, a legend.</summary>
        Other
    }

    /// <summary>Whether the analytical model is being hidden or shown.</summary>
    public enum VisibilityAction
    {
        /// <summary>Hide the analytical model in the views.</summary>
        Hide,

        /// <summary>Show the analytical model in the views.</summary>
        Show
    }

    /// <summary>What is to be done with one element in one view.</summary>
    public enum VisibilityDecision
    {
        /// <summary>The element has to be hidden or shown.</summary>
        Change,

        /// <summary>The element is already hidden or already visible.</summary>
        AlreadyInPlace,

        /// <summary>The element cannot be hidden in this view.</summary>
        CannotChange
    }

    /// <summary>
    /// The rules of hiding and showing the analytical model.
    /// <para>
    /// Only the views a model is looked at in carry an analytical model; schedules, sheets
    /// and legends are left alone. Inside such a view an element is hidden or shown only
    /// when it is not in that state already, and the platform decides whether an element can
    /// be hidden at all. Both rules are plain booleans and are tested without Revit.
    /// </para>
    /// </summary>
    public static class VisibilityRules
    {
        /// <summary>Determines whether a kind of view shows the model and can carry the analytical model.</summary>
        /// <param name="kind">The kind of view.</param>
        /// <returns><c>true</c> for the kinds of view a model is looked at in.</returns>
        public static bool IsModelView(ModelViewKind kind)
        {
            switch (kind)
            {
                case ModelViewKind.ThreeDimensional:
                case ModelViewKind.FloorPlan:
                case ModelViewKind.CeilingPlan:
                case ModelViewKind.Section:
                case ModelViewKind.Elevation:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>Decides what to do with one element in one view.</summary>
        /// <param name="action">Whether the analytical model is being hidden or shown.</param>
        /// <param name="isHidden">Whether the element is hidden in the view.</param>
        /// <param name="canBeHidden">Whether the platform allows the element to be hidden.</param>
        /// <returns>The decision for this element in this view.</returns>
        public static VisibilityDecision Decide(VisibilityAction action, bool isHidden, bool canBeHidden)
        {
            switch (action)
            {
                case VisibilityAction.Hide:
                    if (isHidden)
                    {
                        return VisibilityDecision.AlreadyInPlace;
                    }

                    return canBeHidden ? VisibilityDecision.Change : VisibilityDecision.CannotChange;

                default:
                    return isHidden ? VisibilityDecision.Change : VisibilityDecision.AlreadyInPlace;
            }
        }

        /// <summary>Builds the action that turns the current state of the model around.</summary>
        /// <param name="action">Whether the analytical model is being hidden or shown.</param>
        /// <returns>The opposite action.</returns>
        public static VisibilityAction Opposite(VisibilityAction action)
        {
            return action == VisibilityAction.Hide ? VisibilityAction.Show : VisibilityAction.Hide;
        }
    }
}
