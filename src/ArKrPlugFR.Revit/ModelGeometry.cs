using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Converts between the modelling software's geometry types and the domain
    /// geometry types.
    /// <para>
    /// Revit stores lengths in feet; the domain layer holds millimetres. Every
    /// conversion happens here so that no rule has to know about feet.
    /// </para>
    /// </summary>
    public static class ModelGeometry
    {
        /// <summary>
        /// Returns the model-space box of an element as it appears in the project.
        /// </summary>
        /// <param name="element">The element to measure.</param>
        /// <returns>The box, or <c>null</c> for an element that has no geometry.</returns>
        public static BoundingBox? GetModelBounds(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            BoundingBoxXYZ? box = element.get_BoundingBox(null);

            if (box == null)
            {
                return null;
            }

            return new BoundingBox(ToPoint3(box.Min), ToPoint3(box.Max));
        }

        /// <summary>Converts a Revit point in feet to a domain point in millimetres.</summary>
        public static Point3 ToPoint3(XYZ point)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point));
            }

            return new Point3(
                Length.FromFeet(point.X).Millimeters,
                Length.FromFeet(point.Y).Millimeters,
                Length.FromFeet(point.Z).Millimeters);
        }

        /// <summary>Converts a domain point in millimetres to a Revit point in feet.</summary>
        public static XYZ ToXyz(Point3 point)
        {
            return new XYZ(
                Length.FromMillimeters(point.X).Feet,
                Length.FromMillimeters(point.Y).Feet,
                Length.FromMillimeters(point.Z).Feet);
        }

        /// <summary>Converts a Revit box in feet to a domain box in millimetres.</summary>
        /// <param name="box">The box to convert.</param>
        /// <returns>The same box, measured in millimetres.</returns>
        public static BoundingBox ToBoundingBox(BoundingBoxXYZ box)
        {
            if (box == null)
            {
                throw new ArgumentNullException(nameof(box));
            }

            return new BoundingBox(ToPoint3(box.Min), ToPoint3(box.Max));
        }
    }
}
