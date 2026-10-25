using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Core.Duplication;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the families of a project into the plain records the duplicate scan
    /// works with.
    /// <para>
    /// This is the only place that knows how a family, a type and a parameter are
    /// reached in the Revit API, so the comparison stays free of it.
    /// </para>
    /// </summary>
    public static class FamilyInventory
    {
        /// <summary>Reads every loadable family of the project.</summary>
        /// <param name="document">The project to read.</param>
        /// <param name="errors">Collects the elements that could not be read, for the report.</param>
        public static IReadOnlyList<FamilyRecord> Read(Document document, out IReadOnlyList<string> errors)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            IReadOnlyDictionary<long, int> instanceCounts = CountInstances(document);
            var families = new List<FamilyRecord>();
            var problems = new List<string>();

            var collector = new FilteredElementCollector(document).OfClass(typeof(Family));

            foreach (Element element in collector)
            {
                if (element is not Family family || !family.IsValidObject)
                {
                    continue;
                }

                string familyName = family.Name;

                if (string.IsNullOrWhiteSpace(familyName))
                {
                    problems.Add("A family without a name was skipped.");
                    continue;
                }

                var types = new List<TypeRecord>();

                foreach (ElementId typeId in family.GetFamilySymbolIds())
                {
                    if (document.GetElement(typeId) is not ElementType type)
                    {
                        problems.Add("Family '" + familyName + "': a type could not be read and was skipped.");
                        continue;
                    }

                    long id = ElementIdCompat.GetValue(typeId);

                    types.Add(new TypeRecord(
                        id,
                        ElementIdCompat.GetValue(family.Id),
                        type.Name ?? string.Empty,
                        instanceCounts.TryGetValue(id, out int count) ? count : 0,
                        ReadValues(type)));
                }

                families.Add(new FamilyRecord(
                    familyName,
                    family.FamilyCategory?.Name ?? string.Empty,
                    types,
                    ElementIdCompat.GetValue(family.Id)));
            }

            errors = problems;

            return families;
        }

        /// <summary>Counts the placed instances of every family type in the project.</summary>
        public static IReadOnlyDictionary<long, int> CountInstances(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var counts = new Dictionary<long, int>();

            var collector = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance));

            foreach (Element element in collector)
            {
                ElementId typeId = element.GetTypeId();

                if (typeId == null)
                {
                    continue;
                }

                long value = ElementIdCompat.GetValue(typeId);
                counts[value] = counts.TryGetValue(value, out int count) ? count + 1 : 1;
            }

            return counts;
        }

        /// <summary>Finds one placed instance of a type, so the user can be shown where it is.</summary>
        /// <param name="document">The project to search.</param>
        /// <param name="typeId">Element id of the family type.</param>
        /// <returns>The instance id, or <c>null</c> when the type is not used in the project.</returns>
        public static ElementId? FindInstance(Document document, long typeId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var collector = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance));

            foreach (Element element in collector)
            {
                ElementId elementTypeId = element.GetTypeId();

                if (elementTypeId != null && ElementIdCompat.GetValue(elementTypeId) == typeId)
                {
                    return element.Id;
                }
            }

            return null;
        }

        private static IReadOnlyDictionary<string, string> ReadValues(ElementType type)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (Parameter parameter in type.Parameters)
            {
                if (parameter == null || parameter.IsReadOnly)
                {
                    continue;
                }

                Definition? definition = parameter.Definition;

                if (definition == null || string.IsNullOrWhiteSpace(definition.Name))
                {
                    continue;
                }

                string? value = ReadValue(parameter);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    values[definition.Name] = value!;
                }
            }

            return values;
        }

        private static string? ReadValue(Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString() ?? parameter.AsValueString();

                case StorageType.Integer:
                    return parameter.AsValueString()
                        ?? parameter.AsInteger().ToString(CultureInfo.InvariantCulture);

                case StorageType.Double:
                    return parameter.AsValueString()
                        ?? parameter.AsDouble().ToString("R", CultureInfo.InvariantCulture);

                case StorageType.ElementId:
                    return parameter.AsValueString()
                        ?? DescribeElementId(parameter);

                default:
                    return parameter.AsValueString();
            }
        }

        private static string? DescribeElementId(Parameter parameter)
        {
            ElementId id = parameter.AsElementId();

            return id == null ? null : ElementIdCompat.GetValue(id).ToString(CultureInfo.InvariantCulture);
        }
    }
}
