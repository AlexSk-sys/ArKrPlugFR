using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Annotations;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.RefreshAnnotations
{
    /// <summary>What one run did.</summary>
    internal sealed class RefreshOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        /// <param name="tags">Tags the active view holds.</param>
        /// <param name="view">Name of the active view.</param>
        /// <param name="refreshed">Tags that were nudged.</param>
        /// <param name="orphaned">Tags whose element is gone.</param>
        /// <param name="refused">Tags the platform refused to move.</param>
        /// <param name="failures">One line per refusal.</param>
        /// <param name="stopped">Why the run did not start, or <c>null</c> when it ran.</param>
        internal RefreshOutcome(
            int tags,
            string view,
            int refreshed,
            int orphaned,
            int refused,
            IReadOnlyList<string> failures,
            string? stopped)
        {
            Tags = tags;
            View = view;
            Refreshed = refreshed;
            Orphaned = orphaned;
            Refused = refused;
            Failures = failures ?? Array.Empty<string>();
            Stopped = stopped;
        }

        /// <summary>Gets the number of tags the active view holds.</summary>
        internal int Tags { get; }

        /// <summary>Gets the name of the active view.</summary>
        internal string View { get; }

        /// <summary>Gets the number of tags that were nudged.</summary>
        internal int Refreshed { get; }

        /// <summary>Gets the number of tags whose element is gone.</summary>
        internal int Orphaned { get; }

        /// <summary>Gets the number of tags the platform refused to move.</summary>
        internal int Refused { get; }

        /// <summary>Gets one line per refusal.</summary>
        internal IReadOnlyList<string> Failures { get; }

        /// <summary>Gets why the run did not start, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; }
    }

    /// <summary>
    /// Makes the tags of the active view recompute their text.
    /// <para>
    /// The platform recomputes a tag when the tag itself changes: the tool moves the head
    /// of every planned tag by a millimetre and moves it back, inside one transaction,
    /// and regenerates the document. The tag ends exactly where it started; what changed
    /// is that its text was computed again against the current model.
    /// </para>
    /// </summary>
    internal static class AnnotationRefresher
    {
        /// <summary>The distance a tag head is moved and moved back, in feet.</summary>
        private static readonly double NudgeFeet = Length.FromMillimeters(1d).Feet;

        /// <summary>Refreshes the tags of one view.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="view">The view whose tags are refreshed.</param>
        /// <param name="selection">Elements the user selected; empty means the whole view.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static RefreshOutcome Refresh(
            UIDocument uiDocument,
            View? view,
            ICollection<ElementId> selection,
            RefreshText text,
            Log log)
        {
            if (view == null)
            {
                return Stopped(text.NoView, log);
            }

            Document document = uiDocument.Document;

            var tags = new List<IndependentTag>();
            var byId = new Dictionary<long, IndependentTag>();
            var records = new List<TagRecord>();

            foreach (Element element in new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType())
            {
                if (element is not IndependentTag tag)
                {
                    continue;
                }

                long id = ElementIdCompat.GetValue(tag.Id);

                tags.Add(tag);
                byId[id] = tag;
                records.Add(new TagRecord(id, TagCompat.GetHostIds(tag), tag.IsOrphaned));
            }

            if (tags.Count == 0)
            {
                return Stopped(text.NoTags, log);
            }

            var selected = new List<long>();

            foreach (ElementId id in selection)
            {
                selected.Add(ElementIdCompat.GetValue(id));
            }

            RefreshPlan plan = RefreshPlanner.Build(records, selected);

            if (plan.Tags.Count == 0)
            {
                string message = selected.Count > 0 && plan.Orphaned == 0
                    ? text.NothingSelected
                    : text.AllOrphaned;

                return new RefreshOutcome(tags.Count, view.Name, 0, plan.Orphaned, 0, Array.Empty<string>(), message);
            }

            int refreshed = 0;
            int refused = 0;
            var failures = new List<string>();

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                foreach (long id in plan.Tags)
                {
                    IndependentTag tag = byId[id];

                    try
                    {
                        XYZ head = tag.TagHeadPosition;
                        tag.TagHeadPosition = new XYZ(head.X + NudgeFeet, head.Y, head.Z);
                        tag.TagHeadPosition = head;

                        refreshed++;
                    }
                    catch (Exception exception)
                    {
                        refused++;

                        failures.Add(text.Refused(ElementDescription.Describe(document, id), exception.Message));

                        log.Warn(
                            "RefreshAnnotations: " + ElementDescription.Describe(document, id) +
                            " refused to move: " + exception.Message,
                            nameof(AnnotationRefresher),
                            exception);
                    }
                }

                if (refreshed > 0)
                {
                    document.Regenerate();
                }

                return refreshed > 0;
            });

            if (refreshed > 0)
            {
                try
                {
                    uiDocument.RefreshActiveView();
                }
                catch (Exception exception)
                {
                    log.Warn(
                        "RefreshAnnotations: view '" + view.Name + "' refused to refresh: " + exception.Message,
                        nameof(AnnotationRefresher),
                        exception);
                }
            }

            log.Info(
                "RefreshAnnotations: refreshed " + refreshed + " of " + tags.Count + " tag(s) in view '" +
                view.Name + "', " + plan.Orphaned + " orphaned, " + refused + " refused.",
                nameof(AnnotationRefresher));

            return new RefreshOutcome(
                tags.Count, view.Name, refreshed, plan.Orphaned, refused, failures, null);
        }

        private static RefreshOutcome Stopped(string message, Log log)
        {
            log.Info("RefreshAnnotations: nothing to do — " + message, nameof(AnnotationRefresher));

            return new RefreshOutcome(0, string.Empty, 0, 0, 0, Array.Empty<string>(), message);
        }
    }
}
