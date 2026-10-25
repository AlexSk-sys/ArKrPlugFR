using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Deletes elements by id, as one undoable step, without letting one refusal
    /// stop the rest.
    /// <para>
    /// A delete that silently stops at the first element the modelling software
    /// refuses leaves the project half cleaned, which is worse than a report that
    /// names what could not be removed.
    /// </para>
    /// </summary>
    public static class ElementDeletion
    {
        /// <summary>Deletes the given element ids.</summary>
        /// <param name="document">The document to edit.</param>
        /// <param name="ids">Element ids to delete.</param>
        /// <param name="transactionName">Name shown in the undo list.</param>
        public static DeletionOutcome Delete(Document document, IReadOnlyCollection<long> ids, string transactionName)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var deleted = new List<long>();
            var failures = new List<string>();

            if (ids.Count == 0)
            {
                return new DeletionOutcome(deleted, failures);
            }

            DocumentEdit.Run(document, transactionName, () =>
            {
                foreach (long id in ids)
                {
                    string label = "#" + id.ToString(CultureInfo.InvariantCulture);

                    try
                    {
                        ElementId elementId = ElementIdCompat.Create(id);

                        if (document.GetElement(elementId) == null)
                        {
                            failures.Add(label + ": the element no longer exists.");
                            continue;
                        }

                        ICollection<ElementId> removed = document.Delete(elementId);

                        if (removed == null || removed.Count == 0)
                        {
                            failures.Add(label + ": the element could not be deleted.");
                            continue;
                        }

                        deleted.Add(id);
                    }
                    catch (Exception exception)
                    {
                        failures.Add(label + ": " + exception.Message);
                    }
                }

                return deleted.Count > 0;
            });

            return new DeletionOutcome(deleted, failures);
        }
    }
}
