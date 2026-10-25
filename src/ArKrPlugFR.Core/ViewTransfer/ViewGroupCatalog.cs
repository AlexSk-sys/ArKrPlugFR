using System;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// Maps view-type names to the display groups the tool lists views under.
    /// <para>
    /// The catalog works with stable keys rather than translated captions: the
    /// plugin resolves a key to a localized name, and a view type without a
    /// mapping gets its own group named after the raw type.
    /// </para>
    /// </summary>
    public static class ViewGroupCatalog
    {
        /// <summary>Key of the group for floor, structural and area plans.</summary>
        public const string PlansKey = "plans";

        /// <summary>Key of the group for ceiling plans.</summary>
        public const string CeilingPlansKey = "ceiling-plans";

        /// <summary>Key of the group for three-dimensional views and walkthroughs.</summary>
        public const string ThreeDimensionalKey = "three-d";

        /// <summary>Key of the group for elevations, sections, legends and detail views.</summary>
        public const string SectionsKey = "sections";

        /// <summary>Key of the group for drafting views.</summary>
        public const string DraftingKey = "drafting";

        /// <summary>Key of the group for engineering plans.</summary>
        public const string EngineeringKey = "engineering";

        /// <summary>Key of the group for schedules.</summary>
        public const string SchedulesKey = "schedules";

        /// <summary>Looks up the display-group key of a view-type name.</summary>
        /// <param name="viewTypeName">Name of the view type, for example <c>FloorPlan</c>.</param>
        /// <param name="key">
        /// When the result is <c>true</c>, the catalog key of the group;
        /// otherwise an empty string.
        /// </param>
        /// <returns><c>true</c> when the view type has a mapping.</returns>
        public static bool TryGetGroupKey(string? viewTypeName, out string key)
        {
            string name = viewTypeName?.Trim() ?? string.Empty;

            if (string.Equals(name, "FloorPlan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "StructuralPlan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "AreaPlan", StringComparison.OrdinalIgnoreCase))
            {
                key = PlansKey;
                return true;
            }

            if (string.Equals(name, "CeilingPlan", StringComparison.OrdinalIgnoreCase))
            {
                key = CeilingPlansKey;
                return true;
            }

            if (string.Equals(name, "ThreeD", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "Walkthrough", StringComparison.OrdinalIgnoreCase))
            {
                key = ThreeDimensionalKey;
                return true;
            }

            if (string.Equals(name, "Elevation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "Section", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "Detail", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "Legend", StringComparison.OrdinalIgnoreCase))
            {
                key = SectionsKey;
                return true;
            }

            if (string.Equals(name, "DraftingView", StringComparison.OrdinalIgnoreCase))
            {
                key = DraftingKey;
                return true;
            }

            if (string.Equals(name, "EngineeringPlan", StringComparison.OrdinalIgnoreCase))
            {
                key = EngineeringKey;
                return true;
            }

            if (string.Equals(name, "Schedule", StringComparison.OrdinalIgnoreCase))
            {
                key = SchedulesKey;
                return true;
            }

            key = string.Empty;
            return false;
        }
    }
}
