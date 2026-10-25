using System;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Geometry
{
    /// <summary>
    /// An axis-aligned box in model space, in millimetres.
    /// <para>
    /// This is the shape the Revit API reports for an element, and it is what most
    /// tools use for a cheap "could these two elements touch?" test before doing
    /// any exact geometry work.
    /// </para>
    /// </summary>
    public readonly struct BoundingBox : IEquatable<BoundingBox>
    {
        /// <summary>Creates a box from two opposite corners; they need not be ordered.</summary>
        public BoundingBox(Point3 corner, Point3 oppositeCorner)
        {
            Min = Point3.Min(corner, oppositeCorner);
            Max = Point3.Max(corner, oppositeCorner);
        }

        /// <summary>Creates a box from its extremes.</summary>
        public BoundingBox(Point3 min, Point3 max, bool extremesAreOrdered)
        {
            if (!extremesAreOrdered)
            {
                Min = Point3.Min(min, max);
                Max = Point3.Max(min, max);
                return;
            }

            Min = min;
            Max = max;
        }

        /// <summary>Gets the lower corner.</summary>
        public Point3 Min { get; }

        /// <summary>Gets the upper corner.</summary>
        public Point3 Max { get; }

        /// <summary>Gets the size along X.</summary>
        public Length Width => Length.FromMillimeters(Max.X - Min.X);

        /// <summary>Gets the size along Y.</summary>
        public Length Depth => Length.FromMillimeters(Max.Y - Min.Y);

        /// <summary>Gets the size along Z.</summary>
        public Length Height => Length.FromMillimeters(Max.Z - Min.Z);

        /// <summary>Gets the centre of the box.</summary>
        public Point3 Center => new Point3(
            (Min.X + Max.X) / 2d,
            (Min.Y + Max.Y) / 2d,
            (Min.Z + Max.Z) / 2d);

        /// <summary>Gets the volume of the box.</summary>
        public double Volume => (Max.X - Min.X) * (Max.Y - Min.Y) * (Max.Z - Min.Z);

        /// <summary>Returns a box grown by the given amount on every side.</summary>
        /// <param name="amount">The growth; must not be negative.</param>
        public BoundingBox Inflate(Length amount)
        {
            if (amount.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "The inflation amount cannot be negative.");
            }

            double value = amount.Millimeters;

            return new BoundingBox(
                new Point3(Min.X - value, Min.Y - value, Min.Z - value),
                new Point3(Max.X + value, Max.Y + value, Max.Z + value),
                extremesAreOrdered: true);
        }

        /// <summary>
        /// Determines whether this box and another one overlap, or are closer than
        /// the given tolerance on every axis.
        /// </summary>
        /// <param name="other">The box to test against.</param>
        /// <param name="tolerance">Extra distance allowed on each axis; must not be negative.</param>
        public bool Intersects(BoundingBox other, Length tolerance = default)
        {
            if (tolerance.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance cannot be negative.");
            }

            double value = tolerance.Millimeters;

            return Min.X - value <= other.Max.X && other.Min.X - value <= Max.X
                && Min.Y - value <= other.Max.Y && other.Min.Y - value <= Max.Y
                && Min.Z - value <= other.Max.Z && other.Min.Z - value <= Max.Z;
        }

        /// <summary>Determines whether the box contains a point.</summary>
        public bool Contains(Point3 point)
        {
            return point.X >= Min.X && point.X <= Max.X
                && point.Y >= Min.Y && point.Y <= Max.Y
                && point.Z >= Min.Z && point.Z <= Max.Z;
        }

        /// <summary>
        /// Returns the part two boxes share, or <c>null</c> when they do not overlap.
        /// </summary>
        /// <param name="other">The box to intersect with.</param>
        /// <returns>The shared box, or <c>null</c> when the boxes are apart.</returns>
        public BoundingBox? Intersect(BoundingBox other)
        {
            double minX = Math.Max(Min.X, other.Min.X);
            double minY = Math.Max(Min.Y, other.Min.Y);
            double minZ = Math.Max(Min.Z, other.Min.Z);
            double maxX = Math.Min(Max.X, other.Max.X);
            double maxY = Math.Min(Max.Y, other.Max.Y);
            double maxZ = Math.Min(Max.Z, other.Max.Z);

            if (minX >= maxX || minY >= maxY || minZ >= maxZ)
            {
                return null;
            }

            return new BoundingBox(
                new Point3(minX, minY, minZ),
                new Point3(maxX, maxY, maxZ),
                extremesAreOrdered: true);
        }

        /// <inheritdoc />
        public bool Equals(BoundingBox other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is BoundingBox other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Min + " .. " + Max;
        }

        /// <summary>Determines whether two boxes are the same.</summary>
        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two boxes are different.</summary>
        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(right);
        }
    }
}
