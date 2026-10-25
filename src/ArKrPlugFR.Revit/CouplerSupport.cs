using System;
using ArKrPlugFR.Core.Geometry;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads the geometry the coupler marks work with: the central axis of a coupler and
    /// the centre of a bar.
    /// <para>
    /// Both come from the bounding boxes the model reports, so the measurement works the
    /// same on any release and on any shape of coupler family.
    /// </para>
    /// </summary>
    public static class CouplerSupport
    {
        /// <summary>
        /// Returns the central axis of an element: the longest of its three axes through the
        /// centre of its bounding box.
        /// </summary>
        /// <param name="element">The element to measure.</param>
        /// <returns>The axis, or <c>null</c> when the element has no box with a size.</returns>
        public static Segment3? AxisOf(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            BoundingBox? measured = ModelGeometry.GetModelBounds(element);

            if (measured == null)
            {
                return null;
            }

            BoundingBox box = measured.Value;
            Point3 centre = box.Center;
            double width = box.Width.Millimeters;
            double depth = box.Depth.Millimeters;
            double height = box.Height.Millimeters;

            if (width <= 0d && depth <= 0d && height <= 0d)
            {
                return null;
            }

            if (width >= depth && width >= height)
            {
                return new Segment3(
                    new Point3(box.Min.X, centre.Y, centre.Z),
                    new Point3(box.Max.X, centre.Y, centre.Z));
            }

            if (depth >= width && depth >= height)
            {
                return new Segment3(
                    new Point3(centre.X, box.Min.Y, centre.Z),
                    new Point3(centre.X, box.Max.Y, centre.Z));
            }

            return new Segment3(
                new Point3(centre.X, centre.Y, box.Min.Z),
                new Point3(centre.X, centre.Y, box.Max.Z));
        }

        /// <summary>Returns the centre of the bounding box of an element.</summary>
        /// <param name="element">The element to measure.</param>
        /// <returns>The centre, or <c>null</c> when the element has no box.</returns>
        public static Point3? CentreOf(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return ModelGeometry.GetModelBounds(element)?.Center;
        }
    }
}
