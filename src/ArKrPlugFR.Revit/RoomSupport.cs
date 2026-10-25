using System;
using System.Collections.Generic;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// The elements around a room: its boundary walls, the floor below it and the ceiling
    /// above it.
    /// </summary>
    public sealed class RoomStructures
    {
        /// <summary>Creates the set of structures around a room.</summary>
        /// <param name="walls">Boundary walls of the room.</param>
        /// <param name="floor">Element under the room, or <c>null</c> when none was found.</param>
        /// <param name="ceiling">Element above the room, or <c>null</c> when none was found.</param>
        public RoomStructures(IReadOnlyList<Element> walls, Element? floor, Element? ceiling)
        {
            Walls = walls ?? throw new ArgumentNullException(nameof(walls));
            Floor = floor;
            Ceiling = ceiling;
        }

        /// <summary>Gets the boundary walls of the room.</summary>
        public IReadOnlyList<Element> Walls { get; }

        /// <summary>Gets the element under the room, or <c>null</c>.</summary>
        public Element? Floor { get; }

        /// <summary>Gets the element above the room, or <c>null</c>.</summary>
        public Element? Ceiling { get; }
    }

    /// <summary>
    /// Finds the structures around a room.
    /// <para>
    /// The walls come from the boundary segments of the room. The floor below and the
    /// ceiling above come from the room's own geometry: the lowest horizontal face belongs
    /// to the element under the room and the highest to the element above it, which is the
    /// floor of the storey over when the model has no ceilings.
    /// </para>
    /// </summary>
    public static class RoomSupport
    {
        /// <summary>Reads the structures around a room.</summary>
        /// <param name="document">The document the room belongs to.</param>
        /// <param name="room">The room to read.</param>
        public static RoomStructures Read(Document document, Room room)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            Element? floor;
            Element? ceiling;
            ReadSurroundings(document, room, out floor, out ceiling);

            return new RoomStructures(ReadWalls(document, room), floor, ceiling);
        }

        private static IReadOnlyList<Element> ReadWalls(Document document, Room room)
        {
            var walls = new List<Element>();
            var seen = new HashSet<long>();
            IList<IList<BoundarySegment>>? loops = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            if (loops == null)
            {
                return walls;
            }

            foreach (IList<BoundarySegment> loop in loops)
            {
                foreach (BoundarySegment segment in loop)
                {
                    ElementId id = segment.ElementId;

                    if (id == null || id == ElementId.InvalidElementId || !seen.Add(ElementIdCompat.GetValue(id)))
                    {
                        continue;
                    }

                    if (document.GetElement(id) is Wall wall)
                    {
                        walls.Add(wall);
                    }
                }
            }

            return walls;
        }

        private static void ReadSurroundings(Document document, Room room, out Element? floor, out Element? ceiling)
        {
            floor = null;
            ceiling = null;

            try
            {
                var calculator = new SpatialElementGeometryCalculator(document);
                SpatialElementGeometryResults? results = calculator.CalculateSpatialElementGeometry(room);

                if (results == null)
                {
                    return;
                }

                Solid? solid = results.GetGeometry();

                if (solid == null)
                {
                    return;
                }

                double lowest = double.MaxValue;
                double highest = double.MinValue;

                foreach (Face face in solid.Faces)
                {
                    if (!(face is PlanarFace planar) || Math.Abs(planar.FaceNormal.Z) < 0.9d)
                    {
                        continue;
                    }

                    Element? bounding = BoundingElement(document, results, face, planar);

                    if (bounding == null)
                    {
                        continue;
                    }

                    // The lowest horizontal face belongs to the element under the room and
                    // the highest to the element above it, whatever way the face normals
                    // point.
                    double elevation = planar.Origin.Z;

                    if (elevation < lowest)
                    {
                        lowest = elevation;
                        floor = bounding;
                    }

                    if (elevation > highest)
                    {
                        highest = elevation;
                        ceiling = bounding;
                    }
                }
            }
            catch (Exception)
            {
                // A room whose geometry cannot be read keeps whatever was found before
                // the failure; the report counts what is missing.
            }
        }

        private static Element? BoundingElement(
            Document document,
            SpatialElementGeometryResults results,
            Face face,
            PlanarFace planar)
        {
            IList<SpatialElementBoundarySubface>? subfaces = results.GetBoundaryFaceInfo(face);

            if (subfaces == null)
            {
                return null;
            }

            foreach (SpatialElementBoundarySubface subface in subfaces)
            {
                if (subface == null)
                {
                    continue;
                }

                Face? boundingFace = subface.GetBoundingElementFace();

                if (boundingFace?.Reference == null)
                {
                    continue;
                }

                Element? element = document.GetElement(boundingFace.Reference.ElementId);

                // A boundary that comes from a linked model carries an id of that model;
                // the check keeps the run from writing into an unrelated element of this
                // project that happens to carry the same id.
                if (element != null && ElementHoldsPoint(element, planar.Origin))
                {
                    return element;
                }
            }

            return null;
        }

        private static bool ElementHoldsPoint(Element element, XYZ point)
        {
            const double tolerance = 1d;

            BoundingBoxXYZ? box = element.get_BoundingBox(null);

            if (box == null)
            {
                return false;
            }

            return point.X >= box.Min.X - tolerance && point.X <= box.Max.X + tolerance
                && point.Y >= box.Min.Y - tolerance && point.Y <= box.Max.Y + tolerance
                && point.Z >= box.Min.Z - tolerance && point.Z <= box.Max.Z + tolerance;
        }
    }
}
