using System;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Joins
{
    /// <summary>
    /// One candidate join: a neighbouring element that might be cutting the wall
    /// the user selected.
    /// <para>
    /// The box is recorded here rather than looked up later so that the rule that
    /// decides which joins to break stays independent of the modelling software.
    /// </para>
    /// </summary>
    public readonly struct UnjoinCandidate : IEquatable<UnjoinCandidate>
    {
        /// <summary>Creates a candidate join.</summary>
        /// <param name="wallId">Id of the selected wall.</param>
        /// <param name="otherId">Id of the neighbouring element.</param>
        /// <param name="box">Model box of the neighbouring element.</param>
        /// <param name="isJoined">Whether the modelling software reports the two elements as joined.</param>
        public UnjoinCandidate(long wallId, long otherId, BoundingBox box, bool isJoined)
        {
            if (wallId == otherId)
            {
                throw new ArgumentException("A candidate join cannot reference the same element twice.", nameof(otherId));
            }

            WallId = wallId;
            OtherId = otherId;
            Box = box;
            IsJoined = isJoined;
        }

        /// <summary>Gets the id of the selected wall.</summary>
        public long WallId { get; }

        /// <summary>Gets the id of the neighbouring element.</summary>
        public long OtherId { get; }

        /// <summary>Gets the model box of the neighbouring element.</summary>
        public BoundingBox Box { get; }

        /// <summary>Gets a value indicating whether the elements are joined.</summary>
        public bool IsJoined { get; }

        /// <inheritdoc />
        public bool Equals(UnjoinCandidate other)
        {
            return WallId == other.WallId
                && OtherId == other.OtherId
                && Box.Equals(other.Box)
                && IsJoined == other.IsJoined;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is UnjoinCandidate other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = WallId.GetHashCode();
                hash = (hash * 397) ^ OtherId.GetHashCode();
                hash = (hash * 397) ^ Box.GetHashCode();
                hash = (hash * 397) ^ IsJoined.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return WallId + " x " + OtherId + " joined=" + IsJoined;
        }
    }
}
