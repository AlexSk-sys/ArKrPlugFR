using System;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Alignment
{
    /// <summary>
    /// One straight coordination grid read as a line of the horizontal plane.
    /// <para>
    /// A grid in a project is a curve in plan; the rules here only ever need a point of
    /// it and its direction, both in millimetres, and never touch the vertical axis.
    /// </para>
    /// </summary>
    public readonly struct GridLine
    {
        /// <summary>Creates a line from a point of it and its direction.</summary>
        /// <param name="origin">A point of the line, in millimetres.</param>
        /// <param name="direction">The direction the line runs in, in millimetres; it does not have to be a unit vector, but must not be a single point.</param>
        public GridLine(Point3 origin, Point3 direction)
        {
            double length = Math.Sqrt(
                (direction.X * direction.X) + (direction.Y * direction.Y));

            if (length <= 0d)
            {
                throw new ArgumentException(
                    "The direction of a grid must not be a single point in the horizontal plane.",
                    nameof(direction));
            }

            Origin = origin;
            Direction = direction;
        }

        /// <summary>Gets a point of the line, in millimetres.</summary>
        public Point3 Origin { get; }

        /// <summary>Gets the direction the line runs in, in the horizontal plane.</summary>
        public Point3 Direction { get; }
    }
}
