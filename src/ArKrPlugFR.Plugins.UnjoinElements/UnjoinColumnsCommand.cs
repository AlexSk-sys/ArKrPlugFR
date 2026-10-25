using System.Collections.Generic;
using ArKrPlugFR.Core.Localization;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>
    /// Removes the cuts that columns make in the selected walls.
    /// <para>
    /// Both structural and architectural columns are searched, because either kind
    /// can be the one cutting a wall.
    /// </para>
    /// </summary>
    public sealed class UnjoinColumnsCommand : UnjoinCommandBase
    {
        private static readonly IReadOnlyCollection<BuiltInCategory> Categories = new[]
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Columns,
        };

        /// <inheritdoc />
        protected override LocalizedText CommandTitle => UnjoinStrings.ColumnsTitle;

        /// <inheritdoc />
        protected override LocalizedText NeighbourNoun => UnjoinStrings.ColumnsNoun;

        /// <inheritdoc />
        protected override IReadOnlyCollection<BuiltInCategory> NeighbourCategories => Categories;
    }
}
