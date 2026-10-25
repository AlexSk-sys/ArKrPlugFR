using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Marking;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the facts the structure marking works on: the category of an element, the
    /// names of its materials — the instance's and its type's — and the three sizes of its
    /// bounding box.
    /// </summary>
    public static class StructureFactsReader
    {
        /// <summary>The categories the marking walks.</summary>
        public static readonly BuiltInCategory[] SupportedCategories =
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_StructuralFoundation
        };

        /// <summary>Reads the facts of one element.</summary>
        /// <param name="document">The project the materials belong to.</param>
        /// <param name="element">The element to read.</param>
        public static StructureFacts Read(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            IReadOnlyList<string> names = MaterialNames(document, element);

            BoundingBox? measured = ModelGeometry.GetModelBounds(element);
            double width = 0d;
            double depth = 0d;
            double height = 0d;

            if (measured != null)
            {
                width = measured.Value.Width.Millimeters;
                depth = measured.Value.Depth.Millimeters;
                height = measured.Value.Height.Millimeters;
            }

            return new StructureFacts(ElementCategory.Identifier(element), names, width, depth, height);
        }

        /// <summary>Reads the names of the materials of an element and of its type.</summary>
        /// <param name="document">The project the materials belong to.</param>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyList<string> MaterialNames(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Collect(document, element, names, seen);

            ElementType? type = ParameterSupport.TypeOf(element);

            if (type != null)
            {
                Collect(document, type, names, seen);
            }

            return names;
        }

        private static void Collect(Document document, Element element, List<string> names, HashSet<string> seen)
        {
            foreach (ElementId id in element.GetMaterialIds(false))
            {
                if (id == null || id == ElementId.InvalidElementId)
                {
                    continue;
                }

                string? name = (document.GetElement(id) as Material)?.Name;

                if (!string.IsNullOrWhiteSpace(name) && seen.Add(name!))
                {
                    names.Add(name!);
                }
            }
        }
    }
}
