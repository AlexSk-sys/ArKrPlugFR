using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Concrete;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using RefFace = Autodesk.Revit.Creation.eRefFace;

namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>What one run did.</summary>
    internal sealed class ConcreteOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        /// <param name="equipment">Equipment elements read from the link.</param>
        /// <param name="hosts">Concrete elements found in the project.</param>
        /// <param name="cut">Openings that were cut.</param>
        /// <param name="alreadyThere">Openings that were already present.</param>
        /// <param name="notThrough">Crossings where the equipment does not pass through.</param>
        /// <param name="tooSmall">Crossings too small to cut.</param>
        /// <param name="failed">Openings the platform refused, or hosts that cannot be cut at all.</param>
        /// <param name="failures">One line per refusal.</param>
        /// <param name="stopped">Why the run did not start, or <c>null</c> when it ran.</param>
        internal ConcreteOutcome(
            int equipment,
            int hosts,
            int cut,
            int alreadyThere,
            int notThrough,
            int tooSmall,
            int failed,
            IReadOnlyList<string> failures,
            string? stopped)
        {
            Equipment = equipment;
            Hosts = hosts;
            Cut = cut;
            AlreadyThere = alreadyThere;
            NotThrough = notThrough;
            TooSmall = tooSmall;
            Failed = failed;
            Failures = failures ?? Array.Empty<string>();
            Stopped = stopped;
        }

        /// <summary>Gets the number of equipment elements read from the link.</summary>
        internal int Equipment { get; }

        /// <summary>Gets the number of concrete elements found in the project.</summary>
        internal int Hosts { get; }

        /// <summary>Gets the number of openings cut.</summary>
        internal int Cut { get; }

        /// <summary>Gets the number of openings that were already present.</summary>
        internal int AlreadyThere { get; }

        /// <summary>Gets the number of crossings where the equipment does not pass through.</summary>
        internal int NotThrough { get; }

        /// <summary>Gets the number of crossings too small to cut.</summary>
        internal int TooSmall { get; }

        /// <summary>Gets the number of refusals.</summary>
        internal int Failed { get; }

        /// <summary>Gets one line per refusal.</summary>
        internal IReadOnlyList<string> Failures { get; }

        /// <summary>Gets why the run did not start, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; }
    }

    /// <summary>
    /// Cuts the openings where the equipment of a linked model passes through the
    /// concrete of the current project.
    /// <para>
    /// The equipment is read from the linked document and carried into the coordinates
    /// of the project by the link's total transform; the concrete is read in place. A
    /// crossing is the intersection of the two solids, and an opening is cut only where
    /// the crossing covers the whole thickness of the concrete, so an element that ends
    /// inside a wall is counted instead of being punched through. The openings are the
    /// platform's own: nothing is loaded into the project and the link is never touched.
    /// </para>
    /// </summary>
    internal static class ConcreteCutter
    {
        /// <summary>How far an existing opening may differ and still count as the same one.</summary>
        internal static readonly Length MatchTolerance = Length.FromMillimeters(10d);

        /// <summary>How far two elements may pull apart and still be tested further.</summary>
        private static readonly Length ProbeTolerance = Length.FromMillimeters(1d);

        private static readonly IReadOnlyCollection<BuiltInCategory> EquipmentCategories = new[]
        {
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_Conduit
        };

        private static readonly IReadOnlyCollection<BuiltInCategory> HostCategories = new[]
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Columns
        };

        /// <summary>Cuts the openings of one linked model in the current project.</summary>
        /// <param name="document">Project the openings are cut in.</param>
        /// <param name="link">Linked model the equipment comes from.</param>
        /// <param name="clearance">Clearance added on every side of a crossing.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static ConcreteOutcome Cut(
            Document document,
            RevitLinkInstance link,
            Length clearance,
            CutHolesText text,
            Log log)
        {
            Document? linked = link.GetLinkDocument();

            if (linked == null)
            {
                log.Warn(
                    "CutHolesInConcrete: the link '" + link.Name + "' is not loaded.",
                    nameof(ConcreteCutter));

                return new ConcreteOutcome(
                    0, 0, 0, 0, 0, 0, 0, Array.Empty<string>(), text.LinkNotLoaded);
            }

            Transform toHost = link.GetTotalTransform();

            List<ServiceItem> services = ReadServices(linked, toHost);
            List<HostItem> hosts = ReadHosts(document);

            if (services.Count == 0)
            {
                return new ConcreteOutcome(
                    0, hosts.Count, 0, 0, 0, 0, 0, Array.Empty<string>(), text.NoEquipment);
            }

            if (hosts.Count == 0)
            {
                return new ConcreteOutcome(
                    services.Count, 0, 0, 0, 0, 0, 0, Array.Empty<string>(), text.NoConcrete);
            }

            var planned = new List<CutPlan>();
            var failures = new List<string>();
            Dictionary<long, List<Opening>> existing = ExistingOpenings(document);

            int notThrough = 0;
            int tooSmall = 0;
            int alreadyThere = 0;
            int failed = 0;

            foreach (ServiceItem service in services)
            {
                foreach (HostItem host in hosts)
                {
                    if (!service.Box.Intersects(host.Box, ProbeTolerance))
                    {
                        continue;
                    }

                    PlanStatus status = TryPlan(service, host, clearance, text, out CutPlan? plan, out string? refusal);

                    if (status == PlanStatus.NotThrough)
                    {
                        notThrough++;
                        continue;
                    }

                    if (status == PlanStatus.TooSmall)
                    {
                        tooSmall++;
                        continue;
                    }

                    if (status == PlanStatus.Refused)
                    {
                        failed++;
                        failures.Add(refusal!);
                        continue;
                    }

                    if (Repeats(plan!, planned, existing))
                    {
                        alreadyThere++;
                        continue;
                    }

                    planned.Add(plan!);
                }
            }

            int cut = 0;

            if (planned.Count > 0)
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    foreach (CutPlan plan in planned)
                    {
                        try
                        {
                            Create(document, plan);
                            cut++;

                            log.Info(
                                "CutHolesInConcrete: cut " + plan + " in " +
                                ElementDescription.Describe(plan.Host.Element) + ".",
                                nameof(ConcreteCutter));
                        }
                        catch (Exception exception)
                        {
                            failed++;

                            failures.Add(text.CutFailed(
                                ElementDescription.Describe(plan.Host.Element),
                                exception.Message));

                            log.Warn(
                                "CutHolesInConcrete: " + ElementDescription.Describe(plan.Host.Element) +
                                " refused an opening: " + exception.Message,
                                nameof(ConcreteCutter),
                                exception);
                        }
                    }

                    return cut > 0;
                });
            }

            log.Info(
                "CutHolesInConcrete: " + services.Count + " equipment element(s), " + hosts.Count +
                " concrete element(s), " + cut + " opening(s) cut, " + alreadyThere + " already there, " +
                notThrough + " not through, " + tooSmall + " too small, " + failed + " refused.",
                nameof(ConcreteCutter));

            return new ConcreteOutcome(
                services.Count, hosts.Count, cut, alreadyThere, notThrough, tooSmall, failed, failures, null);
        }

        private static void Create(Document document, CutPlan plan)
        {
            switch (plan.Host.Kind)
            {
                case HostKind.Wall:
                {
                    Point3 basePoint = plan.Host.WallBase!.Value;
                    Point3 direction = plan.Host.WallDirection!.Value;

                    document.Create.NewOpening(
                        (Wall)plan.Host.Element,
                        WallFrame.PointAt(basePoint, direction, plan.Wall.AlongMin, plan.Wall.HeightMin),
                        WallFrame.PointAt(basePoint, direction, plan.Wall.AlongMax, plan.Wall.HeightMax));

                    return;
                }

                case HostKind.Floor:
                {
                    document.Create.NewOpening(plan.Host.Element, ProfileCurves(plan), false);

                    return;
                }

                default:
                {
                    document.Create.NewOpening(plan.Host.Element, ProfileCurves(plan), ReferenceFace(plan.Axis));

                    return;
                }
            }
        }

        private static List<ServiceItem> ReadServices(Document linked, Transform toHost)
        {
            var services = new List<ServiceItem>();

            foreach (Element element in DocumentInfo.GetElements(linked, EquipmentCategories))
            {
                Solid? solid = Solids.FirstSolid(element, toHost);
                BoundingBox? box = solid != null
                    ? Solids.BoxOf(solid)
                    : Solids.TransformedBounds(element, toHost);

                if (box == null)
                {
                    continue;
                }

                services.Add(new ServiceItem(element, box.Value, solid));
            }

            return services;
        }

        private static List<HostItem> ReadHosts(Document document)
        {
            var hosts = new List<HostItem>();

            foreach (Element element in DocumentInfo.GetElements(document, HostCategories))
            {
                if (!ConcreteFilter.IsConcrete(document, element))
                {
                    continue;
                }

                BoundingBox? box = ModelGeometry.GetModelBounds(element);

                if (box == null)
                {
                    continue;
                }

                Point3? basePoint = null;
                Point3? direction = null;
                double thickness = 0d;

                if (element is Wall wall)
                {
                    if (WallFrame.TryRead(wall, out Point3 point, out Point3 along))
                    {
                        basePoint = point;
                        direction = along;
                    }

                    try
                    {
                        thickness = Length.FromFeet(wall.Width).Millimeters;
                    }
                    catch (Exception)
                    {
                        thickness = 0d;
                    }
                }

                hosts.Add(new HostItem(
                    element,
                    box.Value,
                    Solids.FirstSolid(element),
                    KindOf(element),
                    basePoint,
                    direction,
                    thickness,
                    element as FamilyInstance));
            }

            return hosts;
        }

        private static HostKind KindOf(Element element)
        {
            if (element is Wall)
            {
                return HostKind.Wall;
            }

            return element is Floor ? HostKind.Floor : HostKind.Column;
        }

        private static Dictionary<long, List<Opening>> ExistingOpenings(Document document)
        {
            var map = new Dictionary<long, List<Opening>>();

            var collector = new FilteredElementCollector(document)
                .OfClass(typeof(Opening))
                .WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element is not Opening opening)
                {
                    continue;
                }

                Element? host = opening.Host;

                if (host == null)
                {
                    continue;
                }

                long id = ElementIdCompat.GetValue(host.Id);

                if (!map.TryGetValue(id, out List<Opening>? list))
                {
                    list = new List<Opening>();
                    map[id] = list;
                }

                list.Add(opening);
            }

            return map;
        }

        private static PlanStatus TryPlan(
            ServiceItem service,
            HostItem host,
            Length clearance,
            CutHolesText text,
            out CutPlan? plan,
            out string? refusal)
        {
            plan = null;
            refusal = null;

            BoundingBox? crossing = Crossing(service, host);

            if (crossing == null)
            {
                return PlanStatus.NotThrough;
            }

            switch (host.Kind)
            {
                case HostKind.Wall:
                {
                    if (host.WallBase == null || host.WallDirection == null)
                    {
                        refusal = text.WallNotStraight(ElementDescription.Describe(host.Element));

                        return PlanStatus.Refused;
                    }

                    Point3 direction = host.WallDirection.Value;

                    // The direction of passage through a wall is its normal; the crossing
                    // has to cover the wall's thickness along it.
                    var normal = new Point3(direction.Y, -direction.X, 0d);
                    double covered = CutPlanning.ProjectedExtent(crossing.Value, normal);

                    if (!ConcreteRules.PassesThrough(covered, host.WallThicknessMillimeters, ConcreteRules.PassTolerance))
                    {
                        return PlanStatus.NotThrough;
                    }

                    OpeningRectangle rectangle = OpeningRules.WithClearance(
                        OpeningRules.InWallFrame(crossing.Value, host.WallBase.Value, direction),
                        clearance);

                    if (!OpeningRules.IsUsable(rectangle, OpeningRules.MinimumEdge))
                    {
                        return PlanStatus.TooSmall;
                    }

                    plan = CutPlan.ForWall(host, rectangle);

                    return PlanStatus.Ready;
                }

                case HostKind.Floor:
                {
                    double covered = CutPlanning.ExtentAlong(crossing.Value, CutAxis.Z);

                    if (!ConcreteRules.PassesThrough(
                        covered,
                        CutPlanning.ExtentAlong(host.Box, CutAxis.Z),
                        ConcreteRules.PassTolerance))
                    {
                        return PlanStatus.NotThrough;
                    }

                    AxisRectangle rectangle = CutPlanning.Fit(
                        CutPlanning.WithClearance(CutPlanning.Profile(crossing.Value, CutAxis.Z), clearance),
                        CutPlanning.Face(host.Box, CutAxis.Z));

                    if (!CutPlanning.IsUsable(rectangle, OpeningRules.MinimumEdge))
                    {
                        return PlanStatus.TooSmall;
                    }

                    plan = CutPlan.ForSolid(
                        host,
                        rectangle,
                        CutAxis.Z,
                        CutPlanning.CenterAlong(crossing.Value, CutAxis.Z));

                    return PlanStatus.Ready;
                }

                default:
                {
                    if (host.Instance == null)
                    {
                        refusal = text.NotAFamilyInstance(ElementDescription.Describe(host.Element));

                        return PlanStatus.Refused;
                    }

                    // A column is strung on the run of the service; the opening is based
                    // on the middle plane of the column perpendicular to it.
                    CutAxis axis = ConcreteRules.ServiceAxis(service.Box);
                    double covered = CutPlanning.ExtentAlong(crossing.Value, axis);

                    if (!ConcreteRules.PassesThrough(
                        covered,
                        CutPlanning.ExtentAlong(host.Box, axis),
                        ConcreteRules.PassTolerance))
                    {
                        return PlanStatus.NotThrough;
                    }

                    AxisRectangle rectangle = CutPlanning.Fit(
                        CutPlanning.WithClearance(CutPlanning.Profile(crossing.Value, axis), clearance),
                        CutPlanning.Face(host.Box, axis));

                    if (!CutPlanning.IsUsable(rectangle, OpeningRules.MinimumEdge))
                    {
                        return PlanStatus.TooSmall;
                    }

                    plan = CutPlan.ForSolid(host, rectangle, axis, CutPlanning.CenterAlong(host.Box, axis));

                    return PlanStatus.Ready;
                }
            }
        }

        private static BoundingBox? Crossing(ServiceItem service, HostItem host)
        {
            if (service.Solid != null && host.Solid != null)
            {
                Solid? intersection = Solids.Intersection(service.Solid, host.Solid);

                if (intersection != null)
                {
                    BoundingBox? meeting = Solids.BoxOf(intersection);

                    if (meeting != null)
                    {
                        return meeting;
                    }
                }
            }

            return service.Box.Intersect(host.Box);
        }

        private static bool Repeats(
            CutPlan plan,
            List<CutPlan> planned,
            Dictionary<long, List<Opening>> existing)
        {
            foreach (CutPlan other in planned)
            {
                if (ReferenceEquals(other.Host.Element, plan.Host.Element) && SameShape(plan, other))
                {
                    return true;
                }
            }

            if (existing.TryGetValue(ElementIdCompat.GetValue(plan.Host.Element.Id), out List<Opening>? openings))
            {
                foreach (Opening opening in openings)
                {
                    if (MatchesAnOpening(plan, opening))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool SameShape(CutPlan first, CutPlan second)
        {
            if (first.Host.Kind == HostKind.Wall)
            {
                return second.Host.Kind == HostKind.Wall
                    && OpeningRules.Coincides(first.Wall, second.Wall, MatchTolerance);
            }

            return second.Host.Kind != HostKind.Wall
                && first.Axis == second.Axis
                && CutPlanning.Coincides(first.Rect, second.Rect, MatchTolerance);
        }

        private static bool MatchesAnOpening(CutPlan plan, Opening opening)
        {
            BoundingBox? box = ModelGeometry.GetModelBounds(opening);

            if (box == null)
            {
                return false;
            }

            if (plan.Host.Kind == HostKind.Wall)
            {
                if (plan.Host.WallBase == null || plan.Host.WallDirection == null)
                {
                    return false;
                }

                OpeningRectangle rectangle = OpeningRules.InWallFrame(
                    box.Value,
                    plan.Host.WallBase.Value,
                    plan.Host.WallDirection.Value);

                return OpeningRules.Coincides(plan.Wall, rectangle, MatchTolerance);
            }

            AxisRectangle profile = CutPlanning.Profile(box.Value, plan.Axis);

            return CutPlanning.Coincides(plan.Rect, profile, MatchTolerance);
        }

        private static CurveArray ProfileCurves(CutPlan plan)
        {
            var array = new CurveArray();

            XYZ first = Point(plan, plan.Rect.MinU, plan.Rect.MinV);
            XYZ second = Point(plan, plan.Rect.MaxU, plan.Rect.MinV);
            XYZ third = Point(plan, plan.Rect.MaxU, plan.Rect.MaxV);
            XYZ fourth = Point(plan, plan.Rect.MinU, plan.Rect.MaxV);

            array.Append(Line.CreateBound(first, second));
            array.Append(Line.CreateBound(second, third));
            array.Append(Line.CreateBound(third, fourth));
            array.Append(Line.CreateBound(fourth, first));

            return array;
        }

        private static XYZ Point(CutPlan plan, double across, double along)
        {
            switch (plan.Axis)
            {
                case CutAxis.X:
                    return ModelGeometry.ToXyz(new Point3(plan.Plane, across, along));
                case CutAxis.Y:
                    return ModelGeometry.ToXyz(new Point3(across, plan.Plane, along));
                default:
                    return ModelGeometry.ToXyz(new Point3(across, along, plan.Plane));
            }
        }

        private static RefFace ReferenceFace(CutAxis axis)
        {
            switch (axis)
            {
                case CutAxis.X:
                    return RefFace.CenterX;
                case CutAxis.Y:
                    return RefFace.CenterY;
                default:
                    return RefFace.CenterZ;
            }
        }

        private enum HostKind
        {
            Wall,
            Floor,
            Column
        }

        private enum PlanStatus
        {
            Ready,
            NotThrough,
            TooSmall,
            Refused
        }

        private sealed class ServiceItem
        {
            internal ServiceItem(Element element, BoundingBox box, Solid? solid)
            {
                Element = element;
                Box = box;
                Solid = solid;
            }

            internal Element Element { get; }

            internal BoundingBox Box { get; }

            internal Solid? Solid { get; }
        }

        private sealed class HostItem
        {
            internal HostItem(
                Element element,
                BoundingBox box,
                Solid? solid,
                HostKind kind,
                Point3? wallBase,
                Point3? wallDirection,
                double wallThicknessMillimeters,
                FamilyInstance? instance)
            {
                Element = element;
                Box = box;
                Solid = solid;
                Kind = kind;
                WallBase = wallBase;
                WallDirection = wallDirection;
                WallThicknessMillimeters = wallThicknessMillimeters;
                Instance = instance;
            }

            internal Element Element { get; }

            internal BoundingBox Box { get; }

            internal Solid? Solid { get; }

            internal HostKind Kind { get; }

            internal Point3? WallBase { get; }

            internal Point3? WallDirection { get; }

            internal double WallThicknessMillimeters { get; }

            internal FamilyInstance? Instance { get; }
        }

        private sealed class CutPlan
        {
            private CutPlan(HostItem host)
            {
                Host = host;
            }

            internal HostItem Host { get; }

            internal OpeningRectangle Wall { get; private set; }

            internal AxisRectangle Rect { get; private set; }

            internal CutAxis Axis { get; private set; }

            internal double Plane { get; private set; }

            internal static CutPlan ForWall(HostItem host, OpeningRectangle rectangle)
            {
                return new CutPlan(host) { Wall = rectangle };
            }

            internal static CutPlan ForSolid(HostItem host, AxisRectangle rectangle, CutAxis axis, double plane)
            {
                return new CutPlan(host)
                {
                    Rect = rectangle,
                    Axis = axis,
                    Plane = plane
                };
            }

            public override string ToString()
            {
                return Host.Kind == HostKind.Wall
                    ? "opening " + Wall
                    : "opening " + Rect + " along " + Axis;
            }
        }
    }
}
