using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Floors
{
    /// <summary>
    /// One floor of the split: the outer contour and the openings that belong to it,
    /// as indexes into the contour list the plan was built from.
    /// </summary>
    public sealed class FloorPart
    {
        /// <summary>Creates a part.</summary>
        /// <param name="outer">Index of the outer contour.</param>
        /// <param name="holes">Indexes of the openings inside the outer contour.</param>
        public FloorPart(int outer, IReadOnlyList<int> holes)
        {
            Outer = outer;
            Holes = holes ?? throw new ArgumentNullException(nameof(holes));
        }

        /// <summary>Gets the index of the outer contour.</summary>
        public int Outer { get; }

        /// <summary>Gets the indexes of the openings of the part.</summary>
        public IReadOnlyList<int> Holes { get; }
    }
}
