using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the solids of an element and combines them.
    /// <para>
    /// Several tools ask the same questions of geometry — what is the first usable solid
    /// of this element, where do two solids meet, what is the box of that meeting — and
    /// every one of them has to survive shapes the boolean cannot combine, which is why
    /// the answers live in one place instead of in each tool.
    /// </para>
    /// </summary>
    public static class Solids
    {
        /// <summary>
        /// Returns the first usable solid of an element, or <c>null</c> when the element
        /// has none. Nested instances are searched too.
        /// </summary>
        /// <param name="element">The element to read.</param>
        public static Solid? FirstSolid(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            try
            {
                var options = new Options
                {
                    DetailLevel = ViewDetailLevel.Fine,
                    ComputeReferences = false,
                    IncludeNonVisibleObjects = false
                };

                GeometryElement? geometry = element.get_Geometry(options);

                return geometry == null ? null : FirstSolid(geometry);
            }
            catch (Exception)
            {
                // An element whose geometry cannot be read is treated as one without
                // geometry; the caller falls back to boxes.
                return null;
            }
        }

        /// <summary>
        /// Returns the first usable solid of an element, carried into another coordinate
        /// system; for example a linked element read in the coordinates of the host.
        /// </summary>
        /// <param name="element">The element to read.</param>
        /// <param name="transform">The transform to apply, such as the total transform of a link.</param>
        /// <returns>The solid, or <c>null</c> when there is none or it cannot be transformed.</returns>
        public static Solid? FirstSolid(Element element, Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            Solid? solid = FirstSolid(element);

            if (solid == null)
            {
                return null;
            }

            try
            {
                return SolidUtils.CreateTransformed(solid, transform);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns every usable solid of an element, nested instances included, or an
        /// empty list when the element has none.
        /// </summary>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyList<Solid> All(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var solids = new List<Solid>();

            try
            {
                var options = new Options
                {
                    DetailLevel = ViewDetailLevel.Fine,
                    ComputeReferences = false,
                    IncludeNonVisibleObjects = false
                };

                GeometryElement? geometry = element.get_Geometry(options);

                if (geometry != null)
                {
                    Collect(geometry, solids);
                }
            }
            catch (Exception)
            {
                // An element whose geometry cannot be read is treated as one without
                // geometry; the caller reports what it could not do.
            }

            return solids;
        }

        /// <summary>
        /// Intersects two solids and returns the part they share, or <c>null</c> when they
        /// do not share a volume or the boolean refuses the shapes.
        /// </summary>
        /// <param name="first">First solid.</param>
        /// <param name="second">Second solid.</param>
        public static Solid? Intersection(Solid first, Solid second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            try
            {
                Solid result = BooleanOperationsUtils.ExecuteBooleanOperation(
                    first,
                    second,
                    BooleanOperationsType.Intersect);

                return result != null && result.Volume > 0d ? result : null;
            }
            catch (Exception)
            {
                // The boolean refuses some shapes it cannot combine; the caller falls
                // back to boxes, which is coarser but never blocks the run.
                return null;
            }
        }

        /// <summary>Returns the model-space box of a solid in millimetres, or <c>null</c>.</summary>
        /// <param name="solid">The solid to measure.</param>
        public static BoundingBox? BoxOf(Solid solid)
        {
            if (solid == null)
            {
                throw new ArgumentNullException(nameof(solid));
            }

            try
            {
                BoundingBoxXYZ box = solid.GetBoundingBox();

                return box == null ? null : ModelGeometry.ToBoundingBox(box);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the model-space box of an element as it appears in another coordinate
        /// system, in millimetres; the box is re-fitted around the eight transformed
        /// corners, so a rotated link still reports a box that contains the element.
        /// </summary>
        /// <param name="element">The element to measure.</param>
        /// <param name="transform">The transform to apply.</param>
        /// <returns>The box, or <c>null</c> for an element that has no geometry.</returns>
        public static BoundingBox? TransformedBounds(Element element, Transform transform)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            BoundingBoxXYZ? box = element.get_BoundingBox(null);

            if (box == null)
            {
                return null;
            }

            Point3 min = ModelGeometry.ToPoint3(transform.OfPoint(box.Min));
            Point3 max = ModelGeometry.ToPoint3(transform.OfPoint(box.Max));

            foreach (XYZ corner in Corners(box))
            {
                Point3 point = ModelGeometry.ToPoint3(transform.OfPoint(corner));

                min = Point3.Min(min, point);
                max = Point3.Max(max, point);
            }

            return new BoundingBox(min, max, extremesAreOrdered: true);
        }

        private static System.Collections.Generic.IEnumerable<XYZ> Corners(BoundingBoxXYZ box)
        {
            foreach (double x in new[] { box.Min.X, box.Max.X })
            {
                foreach (double y in new[] { box.Min.Y, box.Max.Y })
                {
                    foreach (double z in new[] { box.Min.Z, box.Max.Z })
                    {
                        yield return new XYZ(x, y, z);
                    }
                }
            }
        }

        private static Solid? FirstSolid(GeometryElement geometry)
        {
            foreach (GeometryObject item in geometry)
            {
                if (item is Solid solid && solid.Volume > 0d)
                {
                    return solid;
                }

                if (item is GeometryInstance instance && FirstSolid(instance.GetInstanceGeometry()) is Solid nested)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void Collect(GeometryElement geometry, List<Solid> solids)
        {
            foreach (GeometryObject item in geometry)
            {
                if (item is Solid solid && solid.Volume > 0d)
                {
                    solids.Add(solid);
                }
                else if (item is GeometryInstance instance)
                {
                    Collect(instance.GetInstanceGeometry(), solids);
                }
            }
        }
    }
}
