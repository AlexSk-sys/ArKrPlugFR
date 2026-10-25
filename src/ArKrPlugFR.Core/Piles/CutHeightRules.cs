using System;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Piles
{
    /// <summary>
    /// The height of the part of a pile that is cut off under the raft: the piece between
    /// the top of the pile and the bottom of the slab it runs into.
    /// </summary>
    public static class CutHeightRules
    {
        /// <summary>
        /// Measures the cut height of one pile against one slab.
        /// </summary>
        /// <param name="pile">Box of the pile, in millimetres.</param>
        /// <param name="slab">Box of the slab, in millimetres.</param>
        /// <returns>
        /// The height in millimetres — never more than the pile itself — or <c>null</c>
        /// when the pile does not overlap the slab in plan, or when the slab bottom lies at
        /// or above the pile top, so there is nothing to cut.
        /// </returns>
        public static double? CutHeight(BoundingBox pile, BoundingBox slab)
        {
            bool overlaps = pile.Min.X < slab.Max.X && slab.Min.X < pile.Max.X
                && pile.Min.Y < slab.Max.Y && slab.Min.Y < pile.Max.Y;

            if (!overlaps)
            {
                return null;
            }

            double cut = pile.Max.Z - slab.Min.Z;

            if (cut <= 0d)
            {
                return null;
            }

            double pileHeight = pile.Height.Millimeters;

            return cut > pileHeight ? pileHeight : cut;
        }
    }
}
