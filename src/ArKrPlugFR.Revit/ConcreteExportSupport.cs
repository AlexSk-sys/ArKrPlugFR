using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Core.Concrete;
using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads one element as the cells of the concrete data file: what it is, where it sits,
    /// how big it is and what it is called, with the volume of its material.
    /// </summary>
    public static class ConcreteExportSupport
    {
        /// <summary>The header of the file; English whatever the interface language.</summary>
        public static readonly string[] Header =
        {
            "Id",
            "Category",
            "Family",
            "Type",
            "Level",
            "Materials",
            "Volume_m3",
            "Width_mm",
            "Depth_mm",
            "Height_mm",
            "Mark"
        };

        /// <summary>
        /// Determines whether an element counts as concrete: the name of its family, of its
        /// type or of one of its materials carries the word.
        /// </summary>
        /// <param name="document">The project the materials belong to.</param>
        /// <param name="element">The element to test.</param>
        public static bool IsConcrete(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var names = new List<string?>
            {
                ParameterSupport.TypeOf(element)?.Name,
                (element as FamilyInstance)?.Symbol?.Family?.Name
            };

            foreach (string material in StructureFactsReader.MaterialNames(document, element))
            {
                names.Add(material);
            }

            return ConcreteRules.AnyConcrete(names);
        }

        /// <summary>Builds the cells of one row of the file.</summary>
        /// <param name="document">The project the element belongs to.</param>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyList<string?> Row(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            double width = 0d;
            double depth = 0d;
            double height = 0d;
            BoundingBox? measured = ModelGeometry.GetModelBounds(element);

            if (measured != null)
            {
                width = measured.Value.Width.Millimeters;
                depth = measured.Value.Depth.Millimeters;
                height = measured.Value.Height.Millimeters;
            }

            return new[]
            {
                ElementIdCompat.GetValue(element.Id).ToString(CultureInfo.InvariantCulture),
                ElementCategory.Name(element),
                (element as FamilyInstance)?.Symbol?.Family?.Name,
                ParameterSupport.TypeOf(element)?.Name,
                LevelName(document, element),
                string.Join("; ", StructureFactsReader.MaterialNames(document, element)),
                CsvRules.Number(VolumeCubicMeters(document, element)),
                CsvRules.Number(width),
                CsvRules.Number(depth),
                CsvRules.Number(height),
                Mark(element)
            };
        }

        /// <summary>Reads the volume of an element in cubic metres.</summary>
        /// <param name="document">The project the element belongs to.</param>
        /// <param name="element">The element to measure.</param>
        public static double VolumeCubicMeters(Document document, Element element)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Parameter? volume = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);

            if (volume != null && volume.HasValue && volume.StorageType == StorageType.Double)
            {
                return Volume.FromCubicFeet(volume.AsDouble()).CubicMeters;
            }

            double cubicFeet = 0d;

            foreach (Solid solid in Solids.All(element))
            {
                cubicFeet += solid.Volume;
            }

            return Volume.FromCubicFeet(cubicFeet).CubicMeters;
        }

        private static string? LevelName(Document document, Element element)
        {
            ElementId levelId = element.LevelId;

            if (levelId == null || levelId == ElementId.InvalidElementId)
            {
                return null;
            }

            return document.GetElement(levelId)?.Name;
        }

        private static string? Mark(Element element)
        {
            Parameter? mark = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

            if (mark == null)
            {
                return null;
            }

            return mark.AsString() ?? mark.AsValueString();
        }
    }
}
