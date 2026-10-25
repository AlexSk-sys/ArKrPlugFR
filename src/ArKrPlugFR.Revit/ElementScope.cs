using System;
using System.Collections.Generic;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Turns a user selection into the flat list of model elements a tool works on.
    /// <para>
    /// Selecting a group in Revit selects the group element, not the walls inside
    /// it. Users do it anyway, so every tool that asks for a selection resolves it
    /// through here instead of trusting the raw element ids.
    /// </para>
    /// </summary>
    public static class ElementScope
    {
        /// <summary>
        /// Expands a selection into individual elements, replacing every group with
        /// its members, including members of nested groups.
        /// </summary>
        /// <param name="document">The document the ids belong to.</param>
        /// <param name="selection">Element ids as reported by Revit.</param>
        /// <returns>
        /// The selected elements without groups, ordered by element id, with each
        /// element appearing once.
        /// </returns>
        public static IReadOnlyList<Element> Resolve(Document document, ICollection<ElementId> selection)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            var seen = new HashSet<long>();
            var resolved = new List<Element>();
            var pending = new Queue<ElementId>(selection);

            while (pending.Count > 0)
            {
                ElementId id = pending.Dequeue();

                if (id == null || !seen.Add(ElementIdCompat.GetValue(id)))
                {
                    continue;
                }

                Element element = document.GetElement(id);

                if (element == null)
                {
                    continue;
                }

                if (element is Group group)
                {
                    foreach (ElementId member in group.GetMemberIds())
                    {
                        pending.Enqueue(member);
                    }

                    continue;
                }

                resolved.Add(element);
            }

            resolved.Sort((left, right) => ElementIdCompat.GetValue(left.Id).CompareTo(ElementIdCompat.GetValue(right.Id)));

            return resolved;
        }

        /// <summary>Determines whether an element belongs to the given built-in category.</summary>
        public static bool HasCategory(Element element, BuiltInCategory category)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Category? elementCategory = element.Category;

            return elementCategory != null
                && ElementIdCompat.GetValue(elementCategory.Id) == (long)category;
        }

        /// <summary>Returns only the elements of the given category.</summary>
        public static IReadOnlyList<Element> WithCategory(IEnumerable<Element> elements, BuiltInCategory category)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var filtered = new List<Element>();

            foreach (Element element in elements)
            {
                if (element != null && HasCategory(element, category))
                {
                    filtered.Add(element);
                }
            }

            return filtered;
        }
    }
}
