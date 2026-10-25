using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Concrete
{
    /// <summary>
    /// The rules of cutting openings through concrete: what counts as concrete, which way
    /// a service goes through, and whether it really does.
    /// <para>
    /// A tool that cuts concrete has to answer three questions before it opens a
    /// transaction: is this element concrete, which direction does the service travel,
    /// and does the crossing cover the whole thickness of the element. All three are
    /// decided here, on plain names and boxes, so the answers are testable without a
    /// running Revit.
    /// </para>
    /// </summary>
    public static class ConcreteRules
    {
        /// <summary>
        /// How much of the thickness may stay uncovered and still count as passing
        /// through, in millimetres.
        /// </summary>
        public const double PassToleranceMillimeters = 1d;

        /// <summary>Words that mark an element, a type or a material as concrete.</summary>
        private static readonly string[] ConcreteWords = { "concrete", "бетон" };

        /// <summary>Gets the tolerance of the through test.</summary>
        public static Length PassTolerance => Length.FromMillimeters(PassToleranceMillimeters);

        /// <summary>
        /// Tells whether a name marks its element as concrete: the word "concrete" or
        /// "бетон" anywhere in the name, compared without case.
        /// </summary>
        /// <param name="name">Name of a family, a type or a material.</param>
        /// <returns><c>true</c> when the name carries the word.</returns>
        public static bool IsConcreteName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            foreach (string word in ConcreteWords)
            {
                if (name!.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tells whether any of the names of an element marks it as concrete.
        /// </summary>
        /// <param name="names">Names of the element's family, type and materials.</param>
        /// <returns><c>true</c> when at least one name carries the word.</returns>
        public static bool AnyConcrete(IEnumerable<string?>? names)
        {
            if (names == null)
            {
                return false;
            }

            foreach (string? name in names)
            {
                if (IsConcreteName(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads the direction a piece of equipment travels: the longest side of its box.
        /// The longest side of a duct, a pipe or a cable tray is the way it runs, and an
        /// opening through a column follows that run.
        /// </summary>
        /// <param name="equipment">Box of the equipment, in millimetres.</param>
        /// <returns>The axis of the run; ties are resolved to X, then Y, then Z.</returns>
        public static CutAxis ServiceAxis(BoundingBox equipment)
        {
            double width = equipment.Width.Millimeters;
            double depth = equipment.Depth.Millimeters;
            double height = equipment.Height.Millimeters;

            if (width >= depth && width >= height)
            {
                return CutAxis.X;
            }

            return depth >= height ? CutAxis.Y : CutAxis.Z;
        }

        /// <summary>
        /// Tells whether a crossing covers the whole thickness of its host: the crossing
        /// has to reach the full extent of the host along the direction of passage, less
        /// the tolerance.
        /// </summary>
        /// <param name="crossingExtent">Extent of the crossing along the direction of passage, in millimetres.</param>
        /// <param name="hostExtent">Extent of the host along the same direction, in millimetres.</param>
        /// <param name="tolerance">How much may stay uncovered; must not be negative.</param>
        /// <returns><c>true</c> when the equipment passes through.</returns>
        public static bool PassesThrough(double crossingExtent, double hostExtent, Length tolerance)
        {
            if (hostExtent <= 0d)
            {
                return false;
            }

            double slack = tolerance.Millimeters;

            if (slack < 0d)
            {
                slack = 0d;
            }

            return crossingExtent + slack >= hostExtent;
        }
    }
}
