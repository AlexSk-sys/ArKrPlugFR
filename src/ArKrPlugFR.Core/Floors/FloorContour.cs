using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Floors
{
    /// <summary>
    /// One closed contour of a floor, as a polygon of points in the horizontal plane.
    /// <para>
    /// The Revit side tessellates an edge loop into points before the planar rules run,
    /// so an arc or a spline is decided by its polygon like any other contour. Only the
    /// horizontal coordinates take part in the rules; the elevation is carried along and
    /// ignored by the geometry.
    /// </para>
    /// </summary>
    public sealed class FloorContour
    {
        /// <summary>Area under which a contour is treated as degenerate, in square millimetres.</summary>
        public const double MinimumArea = 1d;

        /// <summary>Creates a contour from its points.</summary>
        /// <param name="points">Points of the polygon; the closing point may be omitted.</param>
        public FloorContour(IReadOnlyList<Point3> points)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
        }

        /// <summary>Gets the points of the polygon.</summary>
        public IReadOnlyList<Point3> Points { get; }

        /// <summary>Gets the area of the polygon in the horizontal plane, in square millimetres.</summary>
        public double Area
        {
            get
            {
                double sum = 0d;

                for (int index = 0; index < Points.Count; index++)
                {
                    Point3 current = Points[index];
                    Point3 next = Points[(index + 1) % Points.Count];

                    sum += (current.X * next.Y) - (next.X * current.Y);
                }

                return Math.Abs(sum) / 2d;
            }
        }

        /// <summary>Gets a value indicating whether the contour is too small to be a profile.</summary>
        public bool IsDegenerate => Points.Count < 3 || Area < MinimumArea;
    }
}
