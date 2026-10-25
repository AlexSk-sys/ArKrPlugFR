using System;

namespace ArKrPlugFR.Core.Loads
{
    /// <summary>A three-dimensional vector of a load, in the internal units of the platform.</summary>
    public readonly struct LoadVector : IEquatable<LoadVector>
    {
        /// <summary>Creates a vector.</summary>
        /// <param name="x">Component along the first axis.</param>
        /// <param name="y">Component along the second axis.</param>
        /// <param name="z">Component along the vertical axis.</param>
        public LoadVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Gets the component along the first axis.</summary>
        public double X { get; }

        /// <summary>Gets the component along the second axis.</summary>
        public double Y { get; }

        /// <summary>Gets the component along the vertical axis.</summary>
        public double Z { get; }

        /// <inheritdoc />
        public bool Equals(LoadVector other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is LoadVector other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        /// <inheritdoc />
        public override string ToString() =>
            "(" + X.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            Y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ", " +
            Z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>
    /// The rules of a line load on a wall: which vectors carry the force and the moment, and
    /// what the far end of the line carries.
    /// <para>
    /// A positive force points down, so the force vector is <c>(0, 0, −Fz)</c>; the moment
    /// keeps its sign on the vertical axis. A uniform load repeats its vectors at both ends
    /// of the line; a load that is not uniform tapers to zero. The arithmetic is plain
    /// numbers and is tested without Revit.
    /// </para>
    /// </summary>
    public static class LineLoadRules
    {
        /// <summary>
        /// Determines whether the platform can accept a load: it refuses one whose force and
        /// moment are both zero.
        /// </summary>
        /// <param name="fz">Force per metre along the vertical axis.</param>
        /// <param name="mz">Moment per metre about the vertical axis.</param>
        /// <returns><c>true</c> when at least one of the two is not zero.</returns>
        public static bool IsUsable(double fz, double mz)
        {
            return fz != 0 || mz != 0;
        }

        /// <summary>Builds the force vector of a load: a positive force points down.</summary>
        /// <param name="fz">Force per metre along the vertical axis.</param>
        /// <returns>The force vector.</returns>
        public static LoadVector Force(double fz)
        {
            return new LoadVector(0, 0, -fz);
        }

        /// <summary>Builds the moment vector of a load.</summary>
        /// <param name="mz">Moment per metre about the vertical axis.</param>
        /// <returns>The moment vector.</returns>
        public static LoadVector Moment(double mz)
        {
            return new LoadVector(0, 0, mz);
        }

        /// <summary>Builds the force vector at the far end of the line.</summary>
        /// <param name="fz">Force per metre along the vertical axis.</param>
        /// <param name="uniform">Whether the load is uniform along the line.</param>
        /// <returns>The force vector at the far end.</returns>
        public static LoadVector EndForce(double fz, bool uniform)
        {
            return uniform ? Force(fz) : new LoadVector(0, 0, 0);
        }

        /// <summary>Builds the moment vector at the far end of the line.</summary>
        /// <param name="mz">Moment per metre about the vertical axis.</param>
        /// <param name="uniform">Whether the load is uniform along the line.</param>
        /// <returns>The moment vector at the far end.</returns>
        public static LoadVector EndMoment(double mz, bool uniform)
        {
            return uniform ? Moment(mz) : new LoadVector(0, 0, 0);
        }
    }
}
