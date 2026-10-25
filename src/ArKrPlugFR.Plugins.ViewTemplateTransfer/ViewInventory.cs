using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.ViewTransfer;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// Reads views out of a Revit project and copies them into another one.
    /// <para>
    /// This is the only place of the tool that knows how views are reached in the
    /// Revit API; the rules that decide what may be transferred live in
    /// <see cref="ArKrPlugFR.Core.ViewTransfer"/> and are unit-tested there.
    /// </para>
    /// </summary>
    public static class ViewInventory
    {
        /// <summary>Reads every view of the project as a transfer candidate.</summary>
        /// <param name="source">The project to read.</param>
        /// <returns>The views of the project, in collector order.</returns>
        public static IReadOnlyList<ViewCandidate> Read(Document source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var candidates = new List<ViewCandidate>();
            var collector = new FilteredElementCollector(source).OfClass(typeof(View));

            foreach (Element element in collector)
            {
                if (element is not View view || !view.IsValidObject)
                {
                    continue;
                }

                bool isSystemSchedule =
                    view is ViewSchedule schedule && schedule.IsTitleblockRevisionSchedule;

                candidates.Add(new ViewCandidate(
                    ElementIdCompat.GetValue(view.Id),
                    view.Name ?? string.Empty,
                    view.ViewType.ToString(),
                    view.IsTemplate,
                    view.IsValidObject,
                    view is ViewSheet,
                    isSystemSchedule));
            }

            return candidates;
        }

        /// <summary>Reads the names of every view of a project, used to plan unique copy names.</summary>
        /// <param name="document">The project to read.</param>
        /// <returns>The view names.</returns>
        public static IReadOnlyList<string> ReadViewNames(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var names = new List<string>();
            var collector = new FilteredElementCollector(document).OfClass(typeof(View));

            foreach (Element element in collector)
            {
                if (element is View view && view.IsValidObject)
                {
                    names.Add(view.Name ?? string.Empty);
                }
            }

            return names;
        }

        /// <summary>
        /// Copies the given views from the source project into the target, as one
        /// undoable step.
        /// <para>
        /// A view that fails is reported and skipped; the rest are still copied.
        /// When nothing could be copied at all, the step leaves no trace in the
        /// target.
        /// </para>
        /// </summary>
        /// <param name="source">The project the views come from; must still be open.</param>
        /// <param name="target">The project the views are copied into.</param>
        /// <param name="elementIds">Element ids of the views to copy.</param>
        /// <param name="transactionName">Name shown in the undo list.</param>
        /// <returns>How many views were copied and which ones failed.</returns>
        public static TransferOutcome Transfer(
            Document source,
            Document target,
            IReadOnlyList<long> elementIds,
            string transactionName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (elementIds == null)
            {
                throw new ArgumentNullException(nameof(elementIds));
            }

            var failures = new List<CopyFailure>();
            int copiedCount = 0;

            bool committed = DocumentEdit.Run(target, transactionName, () =>
            {
                var options = new CopyPasteOptions();

                foreach (long id in elementIds)
                {
                    try
                    {
                        var copySet = new List<ElementId> { ElementIdCompat.Create(id) };

                        ICollection<ElementId> copied = ElementTransformUtils.CopyElements(
                            source,
                            copySet,
                            target,
                            Transform.Identity,
                            options);

                        copiedCount += copied?.Count ?? 0;
                    }
                    catch (Exception exception)
                    {
                        failures.Add(new CopyFailure(id, exception.Message));
                    }
                }

                // The transfer is one step: a single view that could not be copied
                // rolls the whole batch back, so the target is never left with half
                // of what was asked for.
                return failures.Count == 0 && copiedCount > 0;
            });

            return new TransferOutcome(
                committed ? copiedCount : 0,
                failures.OrderBy(failure => failure.Id).ToList());
        }
    }
}
