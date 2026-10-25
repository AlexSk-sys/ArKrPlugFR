using System;

namespace ArKrPlugFR.Core.Units
{
    /// <summary>
    /// A volume as a value object. The Revit API stores volumes in cubic feet, while a
    /// report and a data file want cubic metres, so the conversion lives in one place.
    /// </summary>
    public readonly struct Volume : IEquatable<Volume>
    {
        /// <summary>Number of cubic metres in one cubic foot.</summary>
        public const double CubicMetersPerCubicFoot = 0.028316846592d;

        private Volume(double cubicMeters)
        {
            CubicMeters = cubicMeters;
        }

        /// <summary>Gets the volume in cubic metres.</summary>
        public double CubicMeters { get; }

        /// <summary>Gets the volume in cubic feet, the unit used by the Revit API.</summary>
        public double CubicFeet => CubicMeters / CubicMetersPerCubicFoot;

        /// <summary>Creates a volume from cubic metres.</summary>
        /// <param name="cubicMeters">Volume in cubic metres.</param>
        public static Volume FromCubicMeters(double cubicMeters)
        {
            EnsureFinite(cubicMeters, nameof(cubicMeters));

            return new Volume(cubicMeters);
        }

        /// <summary>Creates a volume from cubic feet.</summary>
        /// <param name="cubicFeet">Volume in cubic feet.</param>
        public static Volume FromCubicFeet(double cubicFeet)
        {
            EnsureFinite(cubicFeet, nameof(cubicFeet));

            return new Volume(cubicFeet * CubicMetersPerCubicFoot);
        }

        /// <inheritdoc />
        public bool Equals(Volume other)
        {
            return CubicMeters.Equals(other.CubicMeters);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Volume other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return CubicMeters.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return CubicMeters.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + " m3";
        }

        /// <summary>Determines whether two volumes are the same.</summary>
        public static bool operator ==(Volume left, Volume right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two volumes are different.</summary>
        public static bool operator !=(Volume left, Volume right)
        {
            return !left.Equals(right);
        }

        private static void EnsureFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "The volume must be a finite number.");
            }
        }
    }
}
