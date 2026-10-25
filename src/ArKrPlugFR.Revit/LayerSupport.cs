using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Core.Units;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the compound structure of a wall or a floor as the facts a text transfer
    /// works on: the material of every layer and its width.
    /// </summary>
    public static class LayerSupport
    {
        /// <summary>
        /// Reads the layers of a wall or a floor.
        /// </summary>
        /// <param name="document">The document the materials belong to.</param>
        /// <param name="element">Wall or floor to read.</param>
        /// <returns>The layers, or <c>null</c> when the element has no compound structure.</returns>
        public static IReadOnlyList<LayerFact>? ReadLayers(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            CompoundStructure? structure = StructureOf(element);

            if (structure == null)
            {
                return null;
            }

            var layers = new List<LayerFact>();

            foreach (CompoundStructureLayer layer in structure.GetLayers())
            {
                string? material = null;

                if (layer.MaterialId != null && layer.MaterialId != ElementId.InvalidElementId)
                {
                    material = (document.GetElement(layer.MaterialId) as Material)?.Name;
                }

                layers.Add(new LayerFact(material, Length.FromFeet(layer.Width).Millimeters));
            }

            return layers;
        }

        private static CompoundStructure? StructureOf(Element element)
        {
            if (element is Wall wall)
            {
                return wall.WallType?.GetCompoundStructure();
            }

            if (element is Floor floor)
            {
                return floor.FloorType?.GetCompoundStructure();
            }

            return null;
        }
    }
}
