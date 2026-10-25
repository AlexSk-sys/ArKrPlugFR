using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Joins
{
    /// <summary>
    /// Decides which candidate joins to break.
    /// <para>
    /// Both conditions from the tool's specification are applied here, and here
    /// only: the neighbouring element must be reported as joined to the wall, and
    /// its box, grown by the search margin, must reach the wall box. Keeping the
    /// pair collection in the domain layer means the same rule can be tested
    /// without starting Revit.
    /// </para>
    /// </summary>
    public static class UnjoinCandidates
    {
        /// <summary>Selects the joins to break.</summary>
        /// <param name="wallBoxes">Model box per selected wall id.</param>
        /// <param name="candidates">Neighbours to test, as reported by the modelling software.</param>
        /// <param name="tolerance">Margin added around the neighbour box before testing for overlap.</param>
        /// <returns>
        /// Distinct pairs, ordered by their ids. Walls that are missing from
        /// <paramref name="wallBoxes"/> are ignored.
        /// </returns>
        public static IReadOnlyList<JoinPair> Select(
            IReadOnlyDictionary<long, BoundingBox> wallBoxes,
            IEnumerable<UnjoinCandidate> candidates,
            Length tolerance = default)
        {
            if (wallBoxes == null)
            {
                throw new ArgumentNullException(nameof(wallBoxes));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (tolerance.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance cannot be negative.");
            }

            var pairs = new HashSet<JoinPair>();

            foreach (UnjoinCandidate candidate in candidates)
            {
                if (!candidate.IsJoined)
                {
                    continue;
                }

                if (!wallBoxes.TryGetValue(candidate.WallId, out BoundingBox wallBox))
                {
                    continue;
                }

                if (candidate.WallId == candidate.OtherId)
                {
                    continue;
                }

                if (!candidate.Box.Inflate(tolerance).Intersects(wallBox))
                {
                    continue;
                }

                pairs.Add(new JoinPair(candidate.WallId, candidate.OtherId));
            }

            return pairs
                .OrderBy(pair => pair)
                .ToList();
        }
    }
}
