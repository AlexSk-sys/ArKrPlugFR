using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// The workset part of the Revit API, kept in one place.
    /// <para>
    /// Reading the user worksets, creating one and moving an element into one are all
    /// the API surface the workset tools need; the rules that decide *which* workset an
    /// element belongs to live in the domain assembly and never touch this.
    /// </para>
    /// </summary>
    public static class WorksetSupport
    {
        /// <summary>Determines whether a document takes part in worksharing.</summary>
        /// <param name="document">The document to check.</param>
        public static bool IsWorkshared(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.IsWorkshared;
        }

        /// <summary>
        /// Reads the name of every user workset of the document; names are matched
        /// without case, because the platform itself does not allow two names that
        /// differ only in case.
        /// </summary>
        /// <param name="document">The document to read.</param>
        /// <returns>Workset names mapped to their ids.</returns>
        public static Dictionary<string, WorksetId> ReadUserWorksetMap(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var map = new Dictionary<string, WorksetId>(StringComparer.OrdinalIgnoreCase);

            foreach (Workset workset in new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset))
            {
                map[workset.Name] = workset.Id;
            }

            return map;
        }

        /// <summary>
        /// Moves an element into a workset by writing the workset parameter, which is
        /// the documented way to reassign an element.
        /// </summary>
        /// <param name="element">The element to move.</param>
        /// <param name="worksetId">The workset to move it to.</param>
        /// <returns><c>true</c> when the platform accepted the value.</returns>
        public static bool TrySetWorkset(Element element, WorksetId worksetId)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (worksetId == null)
            {
                throw new ArgumentNullException(nameof(worksetId));
            }

            Parameter? parameter = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);

            if (parameter == null || parameter.IsReadOnly)
            {
                return false;
            }

            // WorksetId keeps its int accessor in every supported release: only
            // ElementId changed from int to long in Revit 2024.
            return parameter.Set(worksetId.IntegerValue);        }
    }
}
