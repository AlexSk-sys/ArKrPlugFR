using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// What one selection run did.
    /// </summary>
    internal sealed class TypeSelectionOutcome
    {
        /// <summary>Gets or sets how many types were ticked.</summary>
        internal int TypesTicked { get; set; }

        /// <summary>Gets or sets how many elements were selected.</summary>
        internal int Selected { get; set; }

        /// <summary>Gets or sets how many ticked types hold no instances in the scope.</summary>
        internal int WithoutInstances { get; set; }

        /// <summary>Gets the refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Collects the instances of the ticked types and makes them the selection.
    /// <para>
    /// The scope decides where the instances are looked for: the whole project or the
    /// active view. The tool only reads the model — the one thing it changes is the
    /// selection — so the run needs no transaction.
    /// </para>
    /// </summary>
    internal static class TypeSelectionEngine
    {
        /// <summary>Selects the instances of the ticked types.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">The project to read.</param>
        /// <param name="view">The view to look at, or <c>null</c> for the whole project.</param>
        /// <param name="typeIds">Element ids of the ticked types.</param>
        /// <param name="log">Logger of the add-in.</param>
        internal static TypeSelectionOutcome Run(
            UIDocument uiDocument,
            Document document,
            View? view,
            IReadOnlyList<long> typeIds,
            Log log)
        {
            if (uiDocument == null)
            {
                throw new ArgumentNullException(nameof(uiDocument));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (typeIds == null)
            {
                throw new ArgumentNullException(nameof(typeIds));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new TypeSelectionOutcome { TypesTicked = typeIds.Count };

            if (typeIds.Count == 0)
            {
                return outcome;
            }

            IReadOnlyDictionary<long, List<ElementId>> byType = TypeCatalogReader.CollectInstancesByType(document, view, typeIds);
            var selected = new List<ElementId>();

            foreach (long id in typeIds)
            {
                if (byType.TryGetValue(id, out List<ElementId>? instances))
                {
                    selected.AddRange(instances);
                }
                else
                {
                    outcome.WithoutInstances++;
                }
            }

            if (selected.Count > 0)
            {
                try
                {
                    uiDocument.Selection.SetElementIds(selected);
                }
                catch (Exception exception)
                {
                    outcome.Failures.Add(exception.Message);

                    return outcome;
                }
            }

            outcome.Selected = selected.Count;

            log.Info(
                "SelectTypes: ticked " + outcome.TypesTicked + ", selected " + outcome.Selected +
                ", without instances " + outcome.WithoutInstances + ".",
                nameof(TypeSelectionEngine));

            return outcome;
        }
    }
}
