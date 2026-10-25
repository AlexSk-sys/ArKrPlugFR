using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>What one run did.</summary>
    internal sealed class WallOpeningOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        /// <param name="cut">Openings that were cut.</param>
        /// <param name="alreadyThere">Openings that were already in place.</param>
        /// <param name="skipped">Doors and windows that were skipped.</param>
        /// <param name="failures">One line per wall the platform refused.</param>
        internal WallOpeningOutcome(int cut, int alreadyThere, int skipped, IReadOnlyList<string> failures)
        {
            Cut = cut;
            AlreadyThere = alreadyThere;
            Skipped = skipped;
            Failures = failures ?? Array.Empty<string>();
        }

        internal int Cut { get; }

        internal int AlreadyThere { get; }

        internal int Skipped { get; }

        internal IReadOnlyList<string> Failures { get; }

        /// <summary>Gets a value indicating whether the walls carried anything to work with.</summary>
        internal bool AnyOpening => Cut + AlreadyThere > 0;
    }

    /// <summary>
    /// Finds the finish walls lining the selected walls and cuts the openings of their
    /// doors and windows into them.
    /// <para>
    /// A door or window is measured where it stands: its rectangle is read in the frame of
    /// the wall that hosts it, then carried over to each neighbouring wall by projecting
    /// the corners, which keeps the opening in the right place whichever way round the
    /// neighbour runs.
    /// </para>
    /// </summary>
    internal static class WallOpeningCutter
    {
        /// <summary>How far an existing opening may differ and still count as the same one.</summary>
        private static readonly Length MatchTolerance = Length.FromMillimeters(10d);

        /// <summary>Cuts the openings of the doors and windows of the selected walls.</summary>
        /// <param name="document">Project to cut in.</param>
        /// <param name="selected">Walls the user selected.</param>
        /// <param name="settings">The switches of the run.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static WallOpeningOutcome Cut(
            Document document,
            IReadOnlyList<Wall> selected,
            WallOpeningsSettings settings,
            WallOpeningsText text,
            Log log)
        {
            var hosts = new List<HostOpening>();
            var failures = new List<string>();
            int skipped = 0;

            foreach (Wall wall in selected)
            {
                CollectOpenings(document, wall, settings, hosts, failures, ref skipped, text, log);
            }

            if (hosts.Count == 0)
            {
                return new WallOpeningOutcome(0, 0, skipped, failures);
            }

            var neighbours = new List<NeighbourWall>();

            foreach (Wall wall in Neighbours(document, selected))
            {
                if (WallFrame.TryRead(wall, out Point3 basePoint, out Point3 direction))
                {
                    neighbours.Add(new NeighbourWall(wall, basePoint, direction, ExistingOpenings(document, wall)));
                }
            }

            int cut = 0;
            int alreadyThere = 0;

            try
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    foreach (HostOpening host in hosts)
                    {
                        foreach (NeighbourWall neighbour in neighbours)
                        {
                            if (!WallAdjacency.Lines(
                                host.BasePoint,
                                host.Direction,
                                neighbour.BasePoint,
                                neighbour.Direction,
                                WallAdjacency.DefaultGap))
                            {
                                continue;
                            }

                            OpeningRectangle planned = CarryOver(host, neighbour);

                            if (!OpeningRules.IsUsable(planned, OpeningRules.MinimumEdge))
                            {
                                continue;
                            }

                            if (Carries(neighbour.Openings, planned))
                            {
                                alreadyThere++;
                                continue;
                            }

                            try
                            {
                                document.Create.NewOpening(
                                    neighbour.Wall,
                                    WallFrame.PointAt(neighbour.BasePoint, neighbour.Direction, planned.AlongMin, planned.HeightMin),
                                    WallFrame.PointAt(neighbour.BasePoint, neighbour.Direction, planned.AlongMax, planned.HeightMax));

                                cut++;
                            }
                            catch (Exception exception)
                            {
                                failures.Add(text.OpeningFailed(neighbour.Wall.Name, exception.Message));

                                log.Warn(
                                    "WallOpenings: the opening in wall '" + neighbour.Wall.Name +
                                    "' was refused: " + exception.Message,
                                    nameof(WallOpeningCutter),
                                    exception);
                            }
                        }
                    }

                    if (cut > 0)
                    {
                        // The walls and their joins are redrawn with the new openings in
                        // place.
                        document.Regenerate();
                    }

                    return cut > 0;
                });
            }
            catch (Exception exception)
            {
                failures.Add(text.RunFailed(exception.Message));

                log.Error(
                    "WallOpenings: the run failed: " + exception.Message,
                    nameof(WallOpeningCutter),
                    exception);
            }

            log.Info(
                "WallOpenings: cut " + cut + ", already there " + alreadyThere +
                ", skipped " + skipped + ".",
                nameof(WallOpeningCutter));

            return new WallOpeningOutcome(cut, alreadyThere, skipped, failures);
        }

        private static void CollectOpenings(
            Document document,
            Wall wall,
            WallOpeningsSettings settings,
            List<HostOpening> hosts,
            List<string> failures,
            ref int skipped,
            WallOpeningsText text,
            Log log)
        {
            if (!WallFrame.TryRead(wall, out Point3 basePoint, out Point3 direction))
            {
                skipped++;
                failures.Add(text.WallNotStraight(wall.Name));
                return;
            }

            // The opening the bearing wall itself has there, when it has one.
            foreach (Element element in new FilteredElementCollector(document)
                .OfClass(typeof(Opening))
                .WhereElementIsNotElementType())
            {
                if (element is Opening opening
                    && opening.Host is Element host
                    && host.Id == wall.Id
                    && opening.get_BoundingBox(null) is BoundingBoxXYZ box)
                {
                    hosts.Add(new HostOpening(
                        basePoint,
                        direction,
                        OpeningRules.InWallFrame(ModelGeometry.ToBoundingBox(box), basePoint, direction)));
                }
            }

            foreach (FamilyInstance instance in HostedDoorsAndWindows(document, wall, settings))
            {
                if (!TryReadRectangle(instance, basePoint, direction, out OpeningRectangle rectangle))
                {
                    skipped++;
                    failures.Add(text.OpeningNotRead(instance.Name, text.NoSize));

                    log.Warn(
                        "WallOpenings: the size of '" + instance.Name + "' could not be read.",
                        nameof(WallOpeningCutter));

                    continue;
                }

                hosts.Add(new HostOpening(basePoint, direction, rectangle));
            }
        }

        private static bool TryReadRectangle(
            FamilyInstance instance,
            Point3 basePoint,
            Point3 direction,
            out OpeningRectangle rectangle)
        {
            rectangle = default;

            BoundingBoxXYZ? box = instance.get_BoundingBox(null);

            if (box == null)
            {
                return false;
            }

            rectangle = OpeningRules.InWallFrame(
                ModelGeometry.ToBoundingBox(box),
                basePoint,
                direction);

            return OpeningRules.IsUsable(rectangle, OpeningRules.MinimumEdge);
        }

        private static IEnumerable<FamilyInstance> HostedDoorsAndWindows(
            Document document,
            Wall wall,
            WallOpeningsSettings settings)
        {
            foreach (Element element in new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType())
            {
                if (element is FamilyInstance instance
                    && instance.Host is Wall host
                    && host.Id == wall.Id
                    && IsWanted(instance, settings))
                {
                    yield return instance;
                }
            }
        }

        private static bool IsWanted(FamilyInstance instance, WallOpeningsSettings settings)
        {
            Category? category = instance.Category;

            if (category == null)
            {
                return false;
            }

            long id = ElementIdCompat.GetValue(category.Id);

            if (id == (long)BuiltInCategory.OST_Doors)
            {
                return settings.Doors;
            }

            if (id == (long)BuiltInCategory.OST_Windows)
            {
                return settings.Windows;
            }

            return false;
        }

        private static IEnumerable<Wall> Neighbours(Document document, IReadOnlyList<Wall> selected)
        {
            var selectedIds = new HashSet<long>();

            foreach (Wall wall in selected)
            {
                selectedIds.Add(ElementIdCompat.GetValue(wall.Id));
            }

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(Wall)))
            {
                if (element is Wall wall && !selectedIds.Contains(ElementIdCompat.GetValue(wall.Id)))
                {
                    yield return wall;
                }
            }
        }

        private static IReadOnlyList<OpeningRectangle> ExistingOpenings(Document document, Wall wall)
        {
            var rectangles = new List<OpeningRectangle>();

            if (!WallFrame.TryRead(wall, out Point3 basePoint, out Point3 direction))
            {
                return rectangles;
            }

            foreach (Element element in new FilteredElementCollector(document)
                .OfClass(typeof(Opening))
                .WhereElementIsNotElementType())
            {
                if (element is Opening opening
                    && opening.Host is Element host
                    && host.Id == wall.Id
                    && opening.get_BoundingBox(null) is BoundingBoxXYZ box)
                {
                    rectangles.Add(OpeningRules.InWallFrame(
                        ModelGeometry.ToBoundingBox(box),
                        basePoint,
                        direction));
                }
            }

            // A door or window standing in the wall is an opening too.
            foreach (Element element in new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType())
            {
                if (element is FamilyInstance instance
                    && instance.Host is Wall host
                    && host.Id == wall.Id
                    && instance.get_BoundingBox(null) is BoundingBoxXYZ box)
                {
                    rectangles.Add(OpeningRules.InWallFrame(
                        ModelGeometry.ToBoundingBox(box),
                        basePoint,
                        direction));
                }
            }

            return rectangles;
        }

        private static bool Carries(IReadOnlyList<OpeningRectangle> existing, OpeningRectangle planned)
        {
            return existing.Any(rectangle => OpeningRules.Coincides(rectangle, planned, MatchTolerance));
        }

        /// <summary>
        /// Carries the rectangle of a door or window over to a wall that lines the one
        /// hosting it: the corners are projected onto the neighbour, so an opening lands
        /// in the right place however the neighbour runs.
        /// </summary>
        private static OpeningRectangle CarryOver(HostOpening host, NeighbourWall neighbour)
        {
            Point3 near = CornerAlong(host, host.Rectangle.AlongMin);
            Point3 far = CornerAlong(host, host.Rectangle.AlongMax);

            return new OpeningRectangle(
                WallFrame.Along(neighbour.BasePoint, neighbour.Direction, near),
                WallFrame.Along(neighbour.BasePoint, neighbour.Direction, far),
                host.Rectangle.HeightMin,
                host.Rectangle.HeightMax);
        }

        private static Point3 CornerAlong(HostOpening host, double along)
        {
            double length = WallFrame.LengthOf(host.Direction);

            return new Point3(
                host.BasePoint.X + (host.Direction.X / length * along),
                host.BasePoint.Y + (host.Direction.Y / length * along),
                host.BasePoint.Z);
        }

        /// <summary>An opening of a bearing wall, in the frame of that wall.</summary>
        private sealed class HostOpening
        {
            internal HostOpening(Point3 basePoint, Point3 direction, OpeningRectangle rectangle)
            {
                BasePoint = basePoint;
                Direction = direction;
                Rectangle = rectangle;
            }

            internal Point3 BasePoint { get; }

            internal Point3 Direction { get; }

            internal OpeningRectangle Rectangle { get; }
        }

        /// <summary>A wall that may line a bearing wall, with the openings it already has.</summary>
        private sealed class NeighbourWall
        {
            internal NeighbourWall(Wall wall, Point3 basePoint, Point3 direction, IReadOnlyList<OpeningRectangle> openings)
            {
                Wall = wall;
                BasePoint = basePoint;
                Direction = direction;
                Openings = openings;
            }

            internal Wall Wall { get; }

            internal Point3 BasePoint { get; }

            internal Point3 Direction { get; }

            internal IReadOnlyList<OpeningRectangle> Openings { get; }
        }
    }
}
