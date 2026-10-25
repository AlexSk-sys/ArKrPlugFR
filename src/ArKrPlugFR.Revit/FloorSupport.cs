using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the contours of a flat floor and builds new floors from them.
    /// <para>
    /// A floor is described by the edge loops of the top faces of its solids: one outer
    /// loop per part and one loop per opening. The loops are handed to the planar rules
    /// as polygons, and a part is created as a floor on the level and with the type of
    /// its source, carrying the height offset over.
    /// </para>
    /// </summary>
    public static class FloorSupport
    {
        /// <summary>How close to horizontal a face normal must be to count as a top face.</summary>
        private const double HorizontalTolerance = 0.999d;

        /// <summary>
        /// Reads the edge loops of the top faces of a floor, in model coordinates.
        /// </summary>
        /// <param name="floor">The floor to read.</param>
        /// <returns>The loops, or <c>null</c> when the floor has no flat top face.</returns>
        public static IReadOnlyList<CurveLoop>? ReadTopLoops(Floor floor)
        {
            if (floor == null)
            {
                throw new ArgumentNullException(nameof(floor));
            }

            var loops = new List<CurveLoop>();

            foreach (Solid solid in Solids.All(floor))
            {
                PlanarFace? top = TopFace(solid);

                if (top == null)
                {
                    continue;
                }

                foreach (CurveLoop loop in top.GetEdgesAsCurveLoops())
                {
                    loops.Add(loop);
                }
            }

            return loops.Count == 0 ? null : loops;
        }

        /// <summary>Tessellates a loop into points of the horizontal plane, in millimetres.</summary>
        /// <param name="loop">The loop to read.</param>
        public static IReadOnlyList<Point3> ToPoints(CurveLoop loop)
        {
            if (loop == null)
            {
                throw new ArgumentNullException(nameof(loop));
            }

            var points = new List<Point3>();

            foreach (Curve curve in loop)
            {
                foreach (XYZ point in curve.Tessellate())
                {
                    points.Add(ModelGeometry.ToPoint3(point));
                }
            }

            return points;
        }

        /// <summary>
        /// Creates a floor from one part of a source floor and carries the height offset
        /// of the source over.
        /// </summary>
        /// <param name="document">The document to edit.</param>
        /// <param name="profile">Profile of the part: the outer loop first, then its openings.</param>
        /// <param name="source">Floor the part came from.</param>
        /// <returns>The new floor.</returns>
        public static Floor CreateLike(Document document, IReadOnlyList<CurveLoop> profile, Floor source)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Floor created = FloorCompat.CreateLike(document, profile, source);

            CopyHeightOffset(source, created);
            CopyStructuralFlag(source, created);
            CopyWorkset(document, source, created);

            return created;
        }

        private static void CopyHeightOffset(Floor source, Floor created)
        {
            Parameter? from = source.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
            Parameter? to = created.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

            if (from == null || to == null || to.IsReadOnly)
            {
                // A floor type without an editable height offset keeps the offset of its
                // type; there is nothing to carry over.
                return;
            }

            if (!to.Set(from.AsDouble()))
            {
                // A floor that cannot be given the offset of its source would silently end
                // up at another elevation, so the whole split of this floor is refused.
                throw new InvalidOperationException("The height offset of the source floor could not be written.");
            }
        }

        private static void CopyStructuralFlag(Floor source, Floor created)
        {
            Parameter? from = source.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL);
            Parameter? to = created.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL);

            if (from == null || to == null || to.IsReadOnly)
            {
                return;
            }

            to.Set(from.AsInteger());
        }

        private static void CopyWorkset(Document document, Floor source, Floor created)
        {
            if (!WorksetSupport.IsWorkshared(document))
            {
                return;
            }

            // A new element lands on the active workset by default; the split floors belong
            // to the workset of the floor they came from. A workset the platform refuses
            // leaves the floor where it is instead of failing the split.
            WorksetSupport.TrySetWorkset(created, source.WorksetId);
        }

        private static PlanarFace? TopFace(Solid solid)
        {
            PlanarFace? upward = null;
            PlanarFace? horizontal = null;

            foreach (Face face in solid.Faces)
            {
                if (!(face is PlanarFace planar) || Math.Abs(planar.FaceNormal.Z) < HorizontalTolerance)
                {
                    continue;
                }

                if (horizontal == null || face.Area > horizontal.Area)
                {
                    horizontal = planar;
                }

                if (planar.FaceNormal.Z > 0d && (upward == null || face.Area > upward.Area))
                {
                    upward = planar;
                }
            }

            // The top face has an upward normal; the bottom is the fallback for a floor
            // whose geometry carries no upward face at all.
            return upward ?? horizontal;
        }
    }
}
