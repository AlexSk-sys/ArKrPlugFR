using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Sections;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SectionCollector
{
    /// <summary>What one run collected.</summary>
    internal sealed class SectionOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        internal SectionOutcome(
            IReadOnlyList<SectionLevelGroup> groups,
            IReadOnlyList<ElementId> elementIds,
            string report,
            string? stopped)
        {
            Groups = groups ?? Array.Empty<SectionLevelGroup>();
            ElementIds = elementIds ?? Array.Empty<ElementId>();
            Report = report ?? string.Empty;
            Stopped = stopped;
        }

        /// <summary>Gets the report groups.</summary>
        internal IReadOnlyList<SectionLevelGroup> Groups { get; }

        /// <summary>Gets the ids of the collected elements.</summary>
        internal IReadOnlyList<ElementId> ElementIds { get; }

        /// <summary>Gets the report text.</summary>
        internal string Report { get; }

        /// <summary>Gets why the run did not collect, or <c>null</c> when it collected.</summary>
        internal string? Stopped { get; }
    }

    /// <summary>
    /// Collects the concrete structures of a 3D view into the report shape.
    /// <para>
    /// The view supplies the elements, the concrete filter decides which of them count,
    /// and the domain rules group and count them; the engine itself only reads levels,
    /// categories and type names and hands plain data on.
    /// </para>
    /// </summary>
    internal static class SectionEngine
    {
        private static readonly IReadOnlyCollection<BuiltInCategory> Categories = new[]
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralFraming
        };

        /// <summary>Collects the structures of the active 3D view.</summary>
        /// <param name="document">Project to read.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run collected.</returns>
        internal static SectionOutcome Collect(Document document, SectionText text, Log log)
        {
            View3D? view = ViewOf(document);

            if (view == null)
            {
                return Stopped(text.NoView3D, text, log);
            }

            var rows = new List<SectionRow>();
            var ids = new List<ElementId>();
            var filter = new ElementMulticategoryFilter(new List<BuiltInCategory>(Categories));

            foreach (Element element in new FilteredElementCollector(document, view.Id)
                .WhereElementIsNotElementType()
                .WherePasses(filter))
            {
                if (!ConcreteFilter.IsConcrete(document, element))
                {
                    continue;
                }

                string level = LevelOf(document, element, text);
                string category = CategoryOf(element, text);
                string typeName = element.Name ?? "?";

                rows.Add(new SectionRow(level, category, typeName, SectionNaming.Parse(typeName)));
                ids.Add(element.Id);
            }

            if (rows.Count == 0)
            {
                return Stopped(text.NoStructures, text, log);
            }

            IReadOnlyList<SectionLevelGroup> groups = Core.Sections.SectionCollector.Group(rows);
            int types = groups.Sum(group => group.Categories.Sum(category => category.Items.Count));

            log.Info(
                "SectionCollector: " + rows.Count + " element(s) in " + groups.Count + " level(s) and " +
                types + " type(s) from view '" + view.Name + "'.",
                nameof(SectionEngine));

            return new SectionOutcome(groups, ids, text.BuildReport(groups, rows.Count, types), null);
        }

        private static View3D? ViewOf(Document document)
        {
            if (document.ActiveView is View3D active && !active.IsTemplate)
            {
                return active;
            }

            return new FilteredElementCollector(document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(view => !view.IsTemplate);
        }

        private static string LevelOf(Document document, Element element, SectionText text)
        {
            ElementId levelId = element.LevelId;

            if (levelId == null || levelId == ElementId.InvalidElementId)
            {
                return text.NoLevel;
            }

            return (document.GetElement(levelId) as Level)?.Name ?? text.NoLevel;
        }

        private static string CategoryOf(Element element, SectionText text)
        {
            if (element is Wall)
            {
                return text.CategoryWalls;
            }

            if (element is Floor)
            {
                return text.CategoryFloors;
            }

            if (ElementScope.HasCategory(element, BuiltInCategory.OST_StructuralColumns)
                || ElementScope.HasCategory(element, BuiltInCategory.OST_Columns))
            {
                return text.CategoryColumns;
            }

            return text.CategoryBeams;
        }

        private static SectionOutcome Stopped(string message, SectionText text, Log log)
        {
            log.Info("SectionCollector: nothing to collect — " + message, nameof(SectionEngine));

            return new SectionOutcome(Array.Empty<SectionLevelGroup>(), Array.Empty<ElementId>(), string.Empty, message);
        }
    }
}
