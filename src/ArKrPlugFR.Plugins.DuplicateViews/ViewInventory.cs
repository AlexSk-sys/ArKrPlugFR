using System.Collections.Generic;
using ArKrPlugFR.Core.Views;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// Reads what a project offers for copying and sorts it into the three trees.
    /// <para>
    /// Sheets and panel schedules are never listed, served schedules are left out, and
    /// view templates are kept apart from ordinary views, because each kind is copied
    /// by a different call and only ordinary views take a view template afterwards.
    /// </para>
    /// </summary>
    internal static class ViewInventory
    {
        /// <summary>Reads the copyable elements of a project.</summary>
        /// <param name="document">Project to read.</param>
        /// <param name="text">Text used for the group labels.</param>
        /// <returns>One candidate per element, in no particular order.</returns>
        internal static IReadOnlyList<CopyCandidate> Read(Document document, DuplicateViewsText text)
        {
            var candidates = new List<CopyCandidate>();

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(View)))
            {
                if (element is not View view || !view.IsValidObject)
                {
                    continue;
                }

                // Sheets and panel schedules are never listed and never copied.
                if (view is ViewSheet || view.ViewType == ViewType.PanelSchedule)
                {
                    continue;
                }

                string name = view.Name;
                long id = ElementIdCompat.GetValue(view.Id);

                if (view.IsTemplate)
                {
                    candidates.Add(new CopyCandidate(
                        id,
                        name,
                        CopyTree.Templates,
                        GroupOfTemplate(view, name, text),
                        KindOfTemplate(view, name)));

                    continue;
                }

                if (view is ViewSchedule schedule)
                {
                    if (CanCopySchedule(schedule, name))
                    {
                        candidates.Add(new CopyCandidate(
                            id,
                            name,
                            CopyTree.Schedules,
                            GroupOfSchedule(document, schedule, text),
                            TemplateKind.Schedule));
                    }

                    continue;
                }

                candidates.Add(new CopyCandidate(id, name, CopyTree.Views, GroupOfView(view, text), KindOfView(view)));
            }

            return candidates;
        }

        /// <summary>Reads the name of every view in the project, for uniqueness checks.</summary>
        /// <param name="document">Project to read.</param>
        /// <returns>The names of all views, sheets included.</returns>
        internal static IReadOnlyList<string> ReadViewNames(Document document)
        {
            var names = new List<string>();

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(View)))
            {
                if (element is View view && view.IsValidObject)
                {
                    names.Add(view.Name);
                }
            }

            return names;
        }

        private static bool CanCopySchedule(ViewSchedule schedule, string name)
        {
            if (ScheduleRules.IsServedByName(name))
            {
                return false;
            }

            // The revision schedules of the title block, the keynote legends and any
            // schedule without a definition belong to the platform and cannot be
            // duplicated.
            if (schedule.IsTitleblockRevisionSchedule || schedule.IsInternalKeynoteSchedule)
            {
                return false;
            }

            return schedule.Definition != null;
        }

        private static string GroupOfSchedule(Document document, ViewSchedule schedule, DuplicateViewsText text)
        {
            ScheduleDefinition? definition = schedule.Definition;

            Category? category = definition == null
                ? null
                : Category.GetCategory(document, definition.CategoryId);

            return category == null
                ? text.GroupOtherSchedules
                : text.GroupCategorySchedulesOf(category.Name);
        }

        private static string GroupOfView(View view, DuplicateViewsText text)
        {
            switch (view.ViewType)
            {
                case ViewType.ThreeD:
                case ViewType.Walkthrough:
                    return text.GroupThreeDimensional;

                case ViewType.FloorPlan:
                case ViewType.CeilingPlan:
                case ViewType.AreaPlan:
                case ViewType.EngineeringPlan:
                    return text.GroupPlans;

                case ViewType.Elevation:
                case ViewType.Section:
                case ViewType.Detail:
                    return text.GroupSections;

                case ViewType.DraftingView:
                    return text.GroupDrafting;

                case ViewType.Legend:
                    return text.GroupLegends;

                default:
                    return text.GroupOtherViews;
            }
        }

        private static string GroupOfTemplate(View view, string name, DuplicateViewsText text)
        {
            if (IsScheduleTemplate(view, name))
            {
                return text.GroupScheduleTemplates;
            }

            return GroupOfView(view, text);
        }

        private static TemplateKind KindOfView(View view)
        {
            switch (view.ViewType)
            {
                case ViewType.Legend:
                    return TemplateKind.Legend;

                case ViewType.Undefined:
                    return TemplateKind.Undefined;

                default:
                    return TemplateKind.View;
            }
        }

        private static TemplateKind KindOfTemplate(View view, string name)
        {
            if (IsScheduleTemplate(view, name))
            {
                return TemplateKind.Schedule;
            }

            return KindOfView(view);
        }

        /// <summary>
        /// Tells whether a template serves schedules: either the platform says so, or
        /// the name carries a schedule word.
        /// </summary>
        private static bool IsScheduleTemplate(View view, string name)
        {
            if (view.ViewType == ViewType.Schedule)
            {
                return true;
            }

            return ScheduleRules.LooksLikeSchedule(name);
        }
    }
}
