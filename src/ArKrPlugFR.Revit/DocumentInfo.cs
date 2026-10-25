using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Read-only helpers over the active document.
    /// <para>
    /// This is the only layer allowed to touch the Revit API. Domain logic stays
    /// in <c>ArKrPlugFR.Core</c> and receives plain data.
    /// </para>
    /// </summary>
    public static class DocumentInfo
    {
        /// <summary>Returns the Revit release the document was last saved with, e.g. <c>2022</c>.</summary>
        public static string GetRevitVersion(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Application.VersionNumber;
        }

        /// <summary>Returns the document title without the file extension.</summary>
        public static string GetTitle(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Title;
        }

        /// <summary>
        /// Returns every element of the given categories in the document, ordered
        /// by element id so that results are stable between runs.
        /// </summary>
        public static IReadOnlyList<Element> GetElements(Document document, IReadOnlyCollection<BuiltInCategory> categories)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories));
            }

            if (categories.Count == 0)
            {
                return Array.Empty<Element>();
            }

            var filter = new ElementMulticategoryFilter(new List<BuiltInCategory>(categories));

            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .ToElements()
                .OrderBy(element => ElementIdCompat.GetValue(element.Id))
                .ToList();
        }
    }
}
