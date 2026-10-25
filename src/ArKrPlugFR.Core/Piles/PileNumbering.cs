using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Piles
{
    /// <summary>
    /// The order piles are numbered in: from the largest Y down or from the smallest Y up,
    /// and left to right by X where two piles share a row.
    /// </summary>
    public static class PileNumbering
    {
        /// <summary>Returns the order of the piles as indexes into the given list.</summary>
        /// <param name="centres">Location of every pile, in millimetres.</param>
        /// <param name="topDown">Whether the numbering runs from the largest Y down.</param>
        public static IReadOnlyList<int> Order(IReadOnlyList<Point3> centres, bool topDown)
        {
            if (centres == null)
            {
                throw new ArgumentNullException(nameof(centres));
            }

            var indices = new List<int>(centres.Count);

            for (int index = 0; index < centres.Count; index++)
            {
                indices.Add(index);
            }

            indices.Sort((left, right) =>
            {
                Point3 first = centres[left];
                Point3 second = centres[right];

                int byY = first.Y.CompareTo(second.Y);

                if (byY != 0)
                {
                    return topDown ? -byY : byY;
                }

                int byX = first.X.CompareTo(second.X);

                return byX != 0 ? byX : left.CompareTo(right);
            });

            return indices;
        }
    }
}
