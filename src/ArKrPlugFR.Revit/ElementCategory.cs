using System;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Names of the category of an element: the name a user sees and the stable built-in
    /// name such as <c>OST_Walls</c>.
    /// </summary>
    public static class ElementCategory
    {
        /// <summary>Returns the name of the category as shown in the interface, or <c>null</c>.</summary>
        /// <param name="element">The element to read.</param>
        public static string? Name(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return element.Category?.Name;
        }

        /// <summary>
        /// Returns the built-in name of the category such as <c>OST_Walls</c>, or <c>null</c>
        /// for an element without a category or with one the platform does not name.
        /// </summary>
        /// <param name="element">The element to read.</param>
        public static string? Identifier(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Category? category = element.Category;

            if (category == null)
            {
                return null;
            }

            string name = ((BuiltInCategory)ElementIdCompat.GetValue(category.Id)).ToString();

            return name.StartsWith("OST_", StringComparison.Ordinal) ? name : null;
        }
    }
}
