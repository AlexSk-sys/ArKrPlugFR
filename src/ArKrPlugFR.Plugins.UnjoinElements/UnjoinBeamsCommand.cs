using System.Collections.Generic;
using ArKrPlugFR.Core.Localization;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>Removes the cuts that beams make in the selected walls.</summary>
    public sealed class UnjoinBeamsCommand : UnjoinCommandBase
    {
        private static readonly IReadOnlyCollection<BuiltInCategory> Categories = new[]
        {
            BuiltInCategory.OST_StructuralFraming,
        };

        /// <inheritdoc />
        protected override LocalizedText CommandTitle => UnjoinStrings.BeamsTitle;

        /// <inheritdoc />
        protected override LocalizedText NeighbourNoun => UnjoinStrings.BeamsNoun;

        /// <inheritdoc />
        protected override IReadOnlyCollection<BuiltInCategory> NeighbourCategories => Categories;
    }
}
