using System;

namespace ArKrPlugFR.Core.Geometry
{
    /// <summary>
    /// A straight segment in model space, in millimetres.
    /// <para>
    /// The domain layer works with segments where a tool has to ask "how far is this
    /// element from that line" — for example the central axis of a coupler and the centres
    /// of the bars around it.
    /// </para>
    /// </summary>
    public readonly struct Segment3
    {
        /// <summary>Creates a segment.</summary>
        /// <param name="start">One end.</param>
        /// <param name="end">The other end.</param>
        public Segment3(Point3 start, Point3 end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Gets one end of the segment.</summary>
        public Point3 Start { get; }

        /// <summary>Gets the other end of the segment.</summary>
        public Point3 End { get; }

        /// <summary>Gets the length of the segment in millimetres.</summary>
        public double Length => Start.DistanceTo(End);

        /// <summary>Returns the distance from the segment to a point.</summary>
        /// <param name="point">The point to measure.</param>
        public double DistanceTo(Point3 point)
        {
            double dx = End.X - Start.X;
            double dy = End.Y - Start.Y;
            double dz = End.Z - Start.Z;
            double lengthSquared = (dx * dx) + (dy * dy) + (dz * dz);

            if (lengthSquared <= 0d)
            {
                return Start.DistanceTo(point);
            }

            double along = (((point.X - Start.X) * dx) + ((point.Y - Start.Y) * dy) + ((point.Z - Start.Z) * dz)) / lengthSquared;

            if (along <= 0d)
            {
                return Start.DistanceTo(point);
            }

            if (along >= 1d)
            {
                return End.DistanceTo(point);
            }

            return point.DistanceTo(new Point3(
                Start.X + (along * dx),
                Start.Y + (along * dy),
                Start.Z + (along * dz)));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Start + " .. " + End;
        }
    }
}
