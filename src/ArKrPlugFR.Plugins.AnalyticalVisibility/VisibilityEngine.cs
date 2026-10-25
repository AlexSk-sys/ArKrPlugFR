using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Visibility;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AnalyticalVisibility
{
    /// <summary>What one run did.</summary>
    internal sealed class VisibilityOutcome
    {
        internal VisibilityOutcome(
            string? stopped,
            int elements,
            int views,
            int changed,
            int inPlace,
            int cannot,
            int refused,
            IReadOnlyList<string> failures)
        {
            Stopped = stopped;
            Elements = elements;
            Views = views;
            Changed = changed;
            InPlace = inPlace;
            Cannot = cannot;
            Refused = refused;
            Failures = failures ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal int Elements { get; }

        internal int Views { get; }

        internal int Changed { get; }

        internal int InPlace { get; }

        internal int Cannot { get; }

        internal int Refused { get; }

        internal IReadOnlyList<string> Failures { get; }
    }

    /// <summary>
    /// Hides or shows the analytical model of the project in every view that shows the model.
    /// <para>
    /// The run never deletes anything: the analytical elements, their loads and their
    /// boundary conditions stay in the project, and only the per-view hiding changes. What
    /// to do with an element follows <see cref="VisibilityRules"/>, which is tested without
    /// Revit; the elements of the analytical model come from
    /// <see cref="AnalyticalCompat"/>, which carries the release differences.
    /// </para>
    /// </summary>
    internal static class VisibilityEngine
    {
        /// <summary>Runs the hiding or showing over the whole project.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="action">Whether the analytical model is being hidden or shown.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static VisibilityOutcome Run(
            Document document,
            VisibilityAction action,
            VisibilityText text,
            Log log)
        {
            IReadOnlyList<ElementId> analytical = AnalyticalCompat.CollectAnalyticalIds(document);

            if (analytical.Count == 0)
            {
                return new VisibilityOutcome(text.NoAnalyticalModel, 0, 0, 0, 0, 0, 0, Array.Empty<string>());
            }

            IReadOnlyList<View> views = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => !view.IsTemplate && VisibilityRules.IsModelView(AnalyticalCompat.KindOf(view.ViewType)))
                .OrderBy(view => ElementIdCompat.GetValue(view.Id))
                .ToList();

            if (views.Count == 0)
            {
                return new VisibilityOutcome(text.NoViews, analytical.Count, 0, 0, 0, 0, 0, Array.Empty<string>());
            }

            int changed = 0;
            int inPlace = 0;
            int cannot = 0;
            int refused = 0;
            var failures = new List<string>();

            DocumentEdit.Run(document, text.TransactionName(action), () =>
            {
                foreach (View view in views)
                {
                    foreach (ElementId id in analytical)
                    {
                        if (!(document.GetElement(id) is Element element))
                        {
                            continue;
                        }

                        bool isHidden;
                        bool canBeHidden;

                        try
                        {
                            isHidden = element.IsHidden(view);
                            canBeHidden = element.CanBeHidden(view);
                        }
                        catch (Exception)
                        {
                            // An element the view knows nothing about cannot be asked.
                            continue;
                        }

                        switch (VisibilityRules.Decide(action, isHidden, canBeHidden))
                        {
                            case VisibilityDecision.AlreadyInPlace:
                                inPlace++;
                                break;

                            case VisibilityDecision.CannotChange:
                                cannot++;
                                break;

                            default:
                                try
                                {
                                    var single = new List<ElementId> { id };

                                    if (action == VisibilityAction.Hide)
                                    {
                                        view.HideElements(single);
                                    }
                                    else
                                    {
                                        view.UnhideElements(single);
                                    }

                                    changed++;
                                }
                                catch (Exception exception)
                                {
                                    refused++;
                                    failures.Add(text.Refused(ElementDescription.Describe(element), view.Name, exception.Message));

                                    log.Warn(
                                        "AnalyticalVisibility: " + ElementDescription.Describe(element) +
                                        " in «" + view.Name + "» was refused: " + exception.Message,
                                        nameof(VisibilityEngine),
                                        exception);
                                }

                                break;
                        }
                    }
                }

                return changed > 0;
            });

            log.Info(
                "AnalyticalVisibility: " + action + " over " + analytical.Count + " analytical element(s) in " +
                views.Count + " view(s): " + changed + " changed, " + inPlace + " already in place, " +
                cannot + " cannot be changed, " + refused + " refused.",
                nameof(VisibilityEngine));

            return new VisibilityOutcome(null, analytical.Count, views.Count, changed, inPlace, cannot, refused, failures);
        }
    }
}
