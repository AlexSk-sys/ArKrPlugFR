using System;

namespace ArKrPlugFR.Core.Units
{
    /// <summary>
    /// A length as a value object. The Revit API stores lengths in feet, while
    /// project settings and user input are always in millimetres, so conversion
    /// lives in one place instead of being spread over the code base.
    /// </summary>
    public readonly struct Length : IEquatable<Length>, IComparable<Length>
    {
        /// <summary>Number of millimetres in one international foot (0.3048 m).</summary>
        public const double MillimetersPerFoot = 304.8;

        private Length(double millimeters)
        {
            Millimeters = millimeters;
        }

        /// <summary>Gets the length in millimetres.</summary>
        public double Millimeters { get; }

        /// <summary>Gets the length in metres.</summary>
        public double Meters => Millimeters / 1000d;

        /// <summary>Gets the length in international feet (Revit internal units).</summary>
        public double Feet => Millimeters / MillimetersPerFoot;

        /// <summary>Gets the length in inches.</summary>
        public double Inches => Millimeters / 25.4d;

        /// <summary>Gets the zero length.</summary>
        public static Length Zero => new Length(0d);

        /// <summary>Creates a length from millimetres.</summary>
        public static Length FromMillimeters(double millimeters)
        {
            EnsureFinite(millimeters, nameof(millimeters));
            return new Length(millimeters);
        }

        /// <summary>Creates a length from metres.</summary>
        public static Length FromMeters(double meters)
        {
            EnsureFinite(meters, nameof(meters));
            return new Length(meters * 1000d);
        }

        /// <summary>Creates a length from international feet, the unit used by the Revit API.</summary>
        public static Length FromFeet(double feet)
        {
            EnsureFinite(feet, nameof(feet));
            return new Length(feet * MillimetersPerFoot);
        }

        /// <summary>Creates a length from inches.</summary>
        public static Length FromInches(double inches)
        {
            EnsureFinite(inches, nameof(inches));
            return new Length(inches * 25.4d);
        }

        /// <inheritdoc />
        public bool Equals(Length other)
        {
            return Millimeters.Equals(other.Millimeters);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Length other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Millimeters.GetHashCode();
        }

        /// <inheritdoc />
        public int CompareTo(Length other)
        {
            return Millimeters.CompareTo(other.Millimeters);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Millimeters.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + " mm";
        }

        /// <summary>Adds two lengths.</summary>
        public static Length operator +(Length left, Length right)
        {
            return new Length(left.Millimeters + right.Millimeters);
        }

        /// <summary>Subtracts one length from another.</summary>
        public static Length operator -(Length left, Length right)
        {
            return new Length(left.Millimeters - right.Millimeters);
        }

        /// <summary>Scales a length by a factor.</summary>
        public static Length operator *(Length length, double factor)
        {
            EnsureFinite(factor, nameof(factor));
            return new Length(length.Millimeters * factor);
        }

        /// <summary>Returns the absolute value of the length.</summary>
        public static Length Abs(Length length)
        {
            return new Length(Math.Abs(length.Millimeters));
        }

        /// <summary>Determines whether two lengths are equal.</summary>
        public static bool operator ==(Length left, Length right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two lengths are different.</summary>
        public static bool operator !=(Length left, Length right)
        {
            return !left.Equals(right);
        }

        /// <summary>Determines whether the left length is smaller than the right one.</summary>
        public static bool operator <(Length left, Length right)
        {
            return left.Millimeters < right.Millimeters;
        }

        /// <summary>Determines whether the left length is greater than the right one.</summary>
        public static bool operator >(Length left, Length right)
        {
            return left.Millimeters > right.Millimeters;
        }

        /// <summary>Determines whether the left length is smaller than or equal to the right one.</summary>
        public static bool operator <=(Length left, Length right)
        {
            return left.Millimeters <= right.Millimeters;
        }

        /// <summary>Determines whether the left length is greater than or equal to the right one.</summary>
        public static bool operator >=(Length left, Length right)
        {
            return left.Millimeters >= right.Millimeters;
        }

        private static void EnsureFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "The value must be a finite number.");
            }
        }
    }
}
