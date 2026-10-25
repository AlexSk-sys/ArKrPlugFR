using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Analytical
{
    /// <summary>A floor or a beam a member can be extended to, described by its footprint.</summary>
    public readonly struct ExtensionTarget
    {
        /// <summary>Creates a target from the box of the element.</summary>
        /// <param name="bounds">The bounding box of the target element, in millimetres.</param>
        public ExtensionTarget(BoundingBox bounds)
        {
            Bounds = bounds;
        }

        /// <summary>Gets the bounding box of the target element.</summary>
        public BoundingBox Bounds { get; }

        /// <summary>Gets the height the member is extended to: the bottom of the target.</summary>
        public double BottomZ => Bounds.Min.Z;
    }

    /// <summary>
    /// The rules of extending a member up to a target above it.
    /// <para>
    /// A member reaches a target when the target lies above its top and its footprint lies
    /// within the reach in plan; among the targets that qualify, the lowest one wins, and
    /// ties are broken by the nearer footprint. All of it is plain geometry in millimetres
    /// and is tested without Revit.
    /// </para>
    /// </summary>
    public static class ExtensionRules
    {
        /// <summary>The default reach of the search in plan: fifteen feet.</summary>
        public const double DefaultReachMillimeters = 4572d;

        /// <summary>Returns the distance in plan from a point to a box; zero when the point is above the box.</summary>
        /// <param name="point">The point to measure from.</param>
        /// <param name="bounds">The footprint to measure to.</param>
        /// <returns>The distance in plan, in millimetres.</returns>
        public static double PlanarDistance(Point3 point, BoundingBox bounds)
        {
            double dx = DistanceToRange(point.X, bounds.Min.X, bounds.Max.X);
            double dy = DistanceToRange(point.Y, bounds.Min.Y, bounds.Max.Y);

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// Finds the target a member should be extended to: the lowest bottom above the top
        /// of the member, within the reach in plan; ties are broken by the nearer footprint.
        /// </summary>
        /// <param name="sourceTop">The top of the member.</param>
        /// <param name="targets">The targets to choose from, in the order they were read.</param>
        /// <param name="reachMillimeters">How far in plan a target may lie.</param>
        /// <returns>The index of the best target, or -1 when there is none.</returns>
        public static int FindBestTarget(
            Point3 sourceTop,
            IReadOnlyList<ExtensionTarget> targets,
            double reachMillimeters)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (reachMillimeters < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reachMillimeters),
                    reachMillimeters,
                    "The reach cannot be negative.");
            }

            int best = -1;
            double bestBottom = 0;
            double bestDistance = 0;

            for (int index = 0; index < targets.Count; index++)
            {
                ExtensionTarget target = targets[index];

                if (target.BottomZ <= sourceTop.Z)
                {
                    continue;
                }

                double distance = PlanarDistance(sourceTop, target.Bounds);

                if (distance > reachMillimeters)
                {
                    continue;
                }

                if (best < 0
                    || target.BottomZ < bestBottom
                    || (target.BottomZ == bestBottom && distance < bestDistance))
                {
                    best = index;
                    bestBottom = target.BottomZ;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>Returns the new top of a member: its place in plan, raised to a height.</summary>
        /// <param name="top">The current top of the member.</param>
        /// <param name="targetBottomZ">The height to extend to.</param>
        /// <returns>The new top of the member.</returns>
        public static Point3 ExtendedTop(Point3 top, double targetBottomZ)
        {
            return new Point3(top.X, top.Y, targetBottomZ);
        }

        private static double DistanceToRange(double value, double min, double max)
        {
            if (value < min)
            {
                return min - value;
            }

            return value > max ? value - max : 0d;
        }
    }
}
