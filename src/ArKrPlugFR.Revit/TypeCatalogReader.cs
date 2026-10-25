using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Types;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the family types of a project for the type tree: every type, the types placed
    /// in the model, and the types visible in a view.
    /// <para>
    /// A type counts as placed when at least one instance of it exists, and as visible
    /// when at least one instance shows in the view. The instances themselves are collected
    /// on demand, by type, for the selection.
    /// </para>
    /// </summary>
    public static class TypeCatalogReader
    {
        /// <summary>Reads every family type of the project.</summary>
        /// <param name="document">The project to read.</param>
        public static IReadOnlyList<TypeFact> ReadAll(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var facts = new List<TypeFact>();

            foreach (FamilySymbol symbol in Symbols(document))
            {
                facts.Add(Fact(symbol));
            }

            return facts;
        }

        /// <summary>Reads the family types that have instances in the project.</summary>
        /// <param name="document">The project to read.</param>
        public static IReadOnlyList<TypeFact> ReadPlaced(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return ReadWithInstances(document, null);
        }

        /// <summary>Reads the family types that have instances visible in a view.</summary>
        /// <param name="document">The project to read.</param>
        /// <param name="view">The view to look at.</param>
        public static IReadOnlyList<TypeFact> ReadVisible(Document document, View view)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return ReadWithInstances(document, view);
        }

        /// <summary>Collects the instances of the given types, grouped by type.</summary>
        /// <param name="document">The project to read.</param>
        /// <param name="view">The view to look at, or <c>null</c> for the whole project.</param>
        /// <param name="typeIds">Element ids of the types.</param>
        /// <returns>The instance ids of every type that has any, keyed by the type id.</returns>
        public static IReadOnlyDictionary<long, List<ElementId>> CollectInstancesByType(
            Document document,
            View? view,
            IEnumerable<long> typeIds)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (typeIds == null)
            {
                throw new ArgumentNullException(nameof(typeIds));
            }

            var wanted = new HashSet<long>(typeIds);
            var byType = new Dictionary<long, List<ElementId>>();

            foreach (Element element in Instances(document, view))
            {
                ElementId typeId = element.GetTypeId();

                if (typeId == null || typeId == ElementId.InvalidElementId)
                {
                    continue;
                }

                long value = ElementIdCompat.GetValue(typeId);

                if (!wanted.Contains(value))
                {
                    continue;
                }

                if (!byType.TryGetValue(value, out List<ElementId>? instances))
                {
                    instances = new List<ElementId>();
                    byType[value] = instances;
                }

                instances.Add(element.Id);
            }

            return byType;
        }

        private static IReadOnlyList<TypeFact> ReadWithInstances(Document document, View? view)
        {
            var placed = new HashSet<long>();

            foreach (Element element in Instances(document, view))
            {
                ElementId typeId = element.GetTypeId();

                if (typeId != null && typeId != ElementId.InvalidElementId)
                {
                    placed.Add(ElementIdCompat.GetValue(typeId));
                }
            }

            var facts = new List<TypeFact>();

            foreach (FamilySymbol symbol in Symbols(document))
            {
                if (placed.Contains(ElementIdCompat.GetValue(symbol.Id)))
                {
                    facts.Add(Fact(symbol));
                }
            }

            return facts;
        }

        private static IEnumerable<Element> Instances(Document document, View? view)
        {
            FilteredElementCollector collector = view == null
                ? new FilteredElementCollector(document)
                : new FilteredElementCollector(document, view.Id);

            return collector.WhereElementIsNotElementType();
        }

        private static IEnumerable<FamilySymbol> Symbols(Document document)
        {
            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)))
            {
                if (element is FamilySymbol symbol)
                {
                    yield return symbol;
                }
            }
        }

        private static TypeFact Fact(FamilySymbol symbol)
        {
            return new TypeFact(
                ElementIdCompat.GetValue(symbol.Id),
                symbol.Category?.Name,
                symbol.Family?.Name ?? string.Empty,
                symbol.Name ?? string.Empty);
        }
    }
}
