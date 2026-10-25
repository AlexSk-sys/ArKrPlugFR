using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Concrete;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Tells whether an element is concrete, from the names it carries.
    /// <para>
    /// The names of an element's type, its family and every material assigned through its
    /// parameters are the sources; the decision itself is the domain rule. The reader
    /// works the same for walls, floors, columns and any other element, so every tool
    /// that has to find concrete uses one answer.
    /// </para>
    /// </summary>
    public static class ConcreteFilter
    {
        /// <summary>Tells whether an element's names mark it as concrete.</summary>
        /// <param name="document">Document the element belongs to.</param>
        /// <param name="element">Element to read.</param>
        /// <returns><c>true</c> for concrete.</returns>
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

            var names = new List<string?> { element.Name };

            if (element is FamilyInstance familyInstance)
            {
                names.Add(familyInstance.Symbol?.Family?.Name);
            }

            AddMaterialNames(document, element, names);

            if (document.GetElement(element.GetTypeId()) is ElementType type)
            {
                names.Add(type.Name);
                AddMaterialNames(document, type, names);
            }

            return ConcreteRules.AnyConcrete(names);
        }

        private static void AddMaterialNames(Document document, Element element, List<string?> names)
        {
            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.StorageType != StorageType.ElementId)
                {
                    continue;
                }

                ElementId? id = parameter.AsElementId();

                if (id == null || id == ElementId.InvalidElementId)
                {
                    continue;
                }

                if (document.GetElement(id) is Material material)
                {
                    names.Add(material.Name);
                }
            }
        }
    }
}
