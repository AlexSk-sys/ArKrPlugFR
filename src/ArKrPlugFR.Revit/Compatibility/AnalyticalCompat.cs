using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Visibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// The analytical model part of the Revit API, kept in one place.
    /// <para>
    /// The analytical model changed shape between the supported releases: Revit 2020 knows
    /// only the model that hangs off a physical element, Revit 2022 adds the first
    /// analytical members beside it, and Revit 2024 replaces the old model with analytical
    /// elements of its own. Collecting the elements of the analytical model therefore has a
    /// branch per release, and no tool has to know which one it is built against.
    /// </para>
    /// </summary>
    public static class AnalyticalCompat
    {
        /// <summary>The categories whose elements carry an analytical model.</summary>
        private static readonly BuiltInCategory[] LoadableCategories =
        {
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_StructuralTruss
        };

        /// <summary>Collects the elements of the analytical model of the project.</summary>
        /// <param name="document">The document to read.</param>
        /// <returns>The ids of the analytical elements, ordered by id.</returns>
        public static IReadOnlyList<ElementId> CollectAnalyticalIds(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var seen = new HashSet<long>();
            var ids = new List<ElementId>();

            void Add(ElementId? id)
            {
                if (id != null && id != ElementId.InvalidElementId && seen.Add(ElementIdCompat.GetValue(id)))
                {
                    ids.Add(id);
                }
            }

#if REVIT_2020 || REVIT_2022
            var filter = new ElementMulticategoryFilter(new List<BuiltInCategory>(LoadableCategories));

            foreach (Element element in new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(filter))
            {
                try
                {
                    Add(element.GetAnalyticalModelId());
                }
                catch (Exception)
                {
                    // An element without an analytical model answers with nothing.
                }
            }
#endif

#if REVIT_2022 || REVIT_2024
            // The class of an analytical member is not public in every supported release,
            // so the elements are collected by their category instead.
            foreach (Element member in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_AnalyticalMember)
                .WhereElementIsNotElementType())
            {
                Add(member.Id);
            }
#endif

#if REVIT_2024
            foreach (Element panel in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_AnalyticalPanel)
                .WhereElementIsNotElementType())
            {
                Add(panel.Id);
            }
#endif

            return ids
                .OrderBy(id => ElementIdCompat.GetValue(id))
                .ToList();
        }

        /// <summary>Determines whether an element is part of the analytical model itself.</summary>
        /// <param name="element">The element to look at.</param>
        /// <returns><c>true</c> for an analytical member or an analytical node.</returns>
        public static bool IsAnalyticalElement(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Category? category = element.Category;

            if (category == null)
            {
                return false;
            }

            long id = ElementIdCompat.GetValue(category.Id);

#if REVIT_2022 || REVIT_2024
            if (id == (long)BuiltInCategory.OST_AnalyticalMember)
            {
                return true;
            }
#endif

            return id == (long)BuiltInCategory.OST_AnalyticalNodes
                || id == (long)BuiltInCategory.OST_AnalyticalNodes_Lines
                || id == (long)BuiltInCategory.OST_AnalyticalNodes_Points;
        }

        /// <summary>
        /// Returns the analytical element the platform associates with a physical element,
        /// or an invalid id when there is none.
        /// </summary>
        /// <param name="document">The document to read.</param>
        /// <param name="physicalId">Id of the physical element.</param>
        public static ElementId FindAnalyticalId(Document document, ElementId physicalId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (physicalId == null)
            {
                throw new ArgumentNullException(nameof(physicalId));
            }

            Element? physical = document.GetElement(physicalId);

            if (physical == null)
            {
                return ElementId.InvalidElementId;
            }

#if REVIT_2024
            // From Revit 2024 an analytical element names the physical element it belongs to,
            // and the association manager of the release is not part of the public API.
            foreach (ElementId candidate in CollectAnalyticalIds(document))
            {
                Element? analytical = document.GetElement(candidate);
                Parameter? asset = analytical?.get_Parameter(BuiltInParameter.ANALYTICAL_ELEMENT_PHYSICAL_ASSET);

                if (asset != null && asset.StorageType == StorageType.ElementId && asset.AsElementId() == physicalId)
                {
                    return candidate;
                }
            }
#else
            try
            {
                ElementId id = physical.GetAnalyticalModelId();

                if (id != null && id != ElementId.InvalidElementId)
                {
                    return id;
                }
            }
            catch (Exception)
            {
                // An element without an analytical model answers with nothing.
            }
#endif

            // Last resort: an analytical line that lies inside the box of the physical element
            // belongs to it — the rule the analytical model itself follows when it is created.
            // The candidate's own box must fit inside the physical one, so a beam that merely
            // ends inside a column is not mistaken for the column's analytical model.
            Autodesk.Revit.DB.BoundingBoxXYZ? box = physical.get_BoundingBox(null);

            if (box == null)
            {
                return ElementId.InvalidElementId;
            }

            double tolerance = 0.5d;

            foreach (ElementId candidate in CollectAnalyticalIds(document))
            {
                Element? analytical = document.GetElement(candidate);

                if (analytical == null || !(analytical.Location is LocationCurve location) || location.Curve == null)
                {
                    continue;
                }

                try
                {
                    XYZ start = location.Curve.GetEndPoint(0);
                    XYZ end = location.Curve.GetEndPoint(1);

                    if (!Inside(start, box, tolerance) || !Inside(end, box, tolerance))
                    {
                        continue;
                    }

                    Autodesk.Revit.DB.BoundingBoxXYZ? candidateBox = analytical.get_BoundingBox(null);

                    if (candidateBox != null
                        && (!Inside(candidateBox.Min, box, tolerance) || !Inside(candidateBox.Max, box, tolerance)))
                    {
                        continue;
                    }

                    return candidate;
                }
                catch (Exception)
                {
                    // A curve that cannot be read cannot belong to the element.
                }
            }

            return ElementId.InvalidElementId;
        }

        private static bool Inside(XYZ point, Autodesk.Revit.DB.BoundingBoxXYZ box, double tolerance)
        {
            return point.X >= box.Min.X - tolerance && point.X <= box.Max.X + tolerance
                && point.Y >= box.Min.Y - tolerance && point.Y <= box.Max.Y + tolerance
                && point.Z >= box.Min.Z - tolerance && point.Z <= box.Max.Z + tolerance;
        }

        /// <summary>Maps a view type of the platform onto the kinds the rules know.</summary>
        /// <param name="viewType">The view type of the platform.</param>
        /// <returns>The kind of view.</returns>
        public static ModelViewKind KindOf(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.ThreeD:
                    return ModelViewKind.ThreeDimensional;

                case ViewType.FloorPlan:
                case ViewType.EngineeringPlan:
                case ViewType.AreaPlan:
                    return ModelViewKind.FloorPlan;

                case ViewType.CeilingPlan:
                    return ModelViewKind.CeilingPlan;

                case ViewType.Section:
                case ViewType.Detail:
                    return ModelViewKind.Section;

                case ViewType.Elevation:
                    return ModelViewKind.Elevation;

                default:
                    return ModelViewKind.Other;
            }
        }
    }
}
