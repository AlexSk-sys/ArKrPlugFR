using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Floors
{
    /// <summary>
    /// What the planar rules make of the contours of one floor: the parts to create,
    /// and the contours that cannot take part in a profile, each with its reason.
    /// </summary>
    public sealed class FloorLoopPlan
    {
        /// <summary>Creates a plan.</summary>
        /// <param name="parts">Parts of the floor, one new floor each.</param>
        /// <param name="degenerate">Contours too small or too short to be a profile.</param>
        /// <param name="nested">Contours inside an opening, which cannot be a profile.</param>
        /// <param name="duplicates">Contours that repeat another contour.</param>
        public FloorLoopPlan(
            IReadOnlyList<FloorPart> parts,
            IReadOnlyList<int> degenerate,
            IReadOnlyList<int> nested,
            IReadOnlyList<int> duplicates)
        {
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));
            Degenerate = degenerate ?? throw new ArgumentNullException(nameof(degenerate));
            Nested = nested ?? throw new ArgumentNullException(nameof(nested));
            Duplicates = duplicates ?? throw new ArgumentNullException(nameof(duplicates));
        }

        /// <summary>Gets the parts of the floor, in the order of their outer contours.</summary>
        public IReadOnlyList<FloorPart> Parts { get; }

        /// <summary>Gets the indexes of the degenerate contours.</summary>
        public IReadOnlyList<int> Degenerate { get; }

        /// <summary>Gets the indexes of the contours that lie inside an opening.</summary>
        public IReadOnlyList<int> Nested { get; }

        /// <summary>Gets the indexes of the duplicate contours.</summary>
        public IReadOnlyList<int> Duplicates { get; }

        /// <summary>Gets a value indicating whether any contour of the floor could not be used.</summary>
        public bool HasRejectedContours => Degenerate.Count > 0 || Nested.Count > 0 || Duplicates.Count > 0;
    }
}
