using System;

namespace ArKrPlugFR.Core.Geometry
{
    /// <summary>
    /// A point in model space, in millimetres.
    /// <para>
    /// Kept in the domain layer so that geometric rules can be tested without
    /// Revit: the Revit API works in feet and owns its own point type.
    /// </para>
    /// </summary>
    public readonly struct Point3 : IEquatable<Point3>
    {
        /// <summary>Creates a point from its millimetre coordinates.</summary>
        public Point3(double x, double y, double z)
        {
            EnsureFinite(x, nameof(x));
            EnsureFinite(y, nameof(y));
            EnsureFinite(z, nameof(z));

            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Gets the coordinate along the X axis.</summary>
        public double X { get; }

        /// <summary>Gets the coordinate along the Y axis.</summary>
        public double Y { get; }

        /// <summary>Gets the coordinate along the Z axis.</summary>
        public double Z { get; }

        /// <summary>Gets the origin.</summary>
        public static Point3 Origin => new Point3(0d, 0d, 0d);

        /// <summary>Returns the component-wise minimum of two points.</summary>
        public static Point3 Min(Point3 left, Point3 right)
        {
            return new Point3(
                Math.Min(left.X, right.X),
                Math.Min(left.Y, right.Y),
                Math.Min(left.Z, right.Z));
        }

        /// <summary>Returns the component-wise maximum of two points.</summary>
        public static Point3 Max(Point3 left, Point3 right)
        {
            return new Point3(
                Math.Max(left.X, right.X),
                Math.Max(left.Y, right.Y),
                Math.Max(left.Z, right.Z));
        }

        /// <summary>Returns the straight-line distance to another point.</summary>
        public double DistanceTo(Point3 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;

            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        /// <inheritdoc />
        public bool Equals(Point3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Point3 other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Z.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + X.ToString("0.###") + ", " + Y.ToString("0.###") + ", " + Z.ToString("0.###") + ")";
        }

        /// <summary>Determines whether two points are equal.</summary>
        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two points are different.</summary>
        public static bool operator !=(Point3 left, Point3 right)
        {
            return !left.Equals(right);
        }

        private static void EnsureFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "The coordinate must be a finite number.");
            }
        }
    }
}
