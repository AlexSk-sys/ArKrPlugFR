using System;
using System.Globalization;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Writes a one-line description of an element for a report.
    /// <para>
    /// Every tool that lists elements shows the same three things: the id, the
    /// category and the type name. Users need the id to find the element in Revit,
    /// so it is never dropped in favour of a prettier name.
    /// </para>
    /// </summary>
    public static class ElementDescription
    {
        /// <summary>Describes an element by id; missing ids are reported, not hidden.</summary>
        public static string Describe(Document document, long id)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            Element? element = document.GetElement(ElementIdCompat.Create(id));

            if (element == null)
            {
                return "#" + id.ToString(CultureInfo.InvariantCulture) + " (missing)";
            }

            return Describe(id, element);
        }

        /// <summary>Describes an element.</summary>
        public static string Describe(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return Describe(ElementIdCompat.GetValue(element.Id), element);
        }

        private static string Describe(long id, Element element)
        {
            string category = element.Category?.Name ?? "?";
            string typeName = element.Name ?? "?";

            return "#" + id.ToString(CultureInfo.InvariantCulture) + " " + category + " / " + typeName;
        }
    }
}
