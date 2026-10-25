using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;

namespace ArKrPlugFR.Core.Floors
{
    /// <summary>
    /// The planar rules that turn the contours of one flat floor into the parts a split
    /// creates.
    /// <para>
    /// Only the horizontal coordinates take part: a contour is a polygon, an opening is a
    /// polygon inside its part, and a polygon inside an opening cannot be a profile of a
    /// floor. The rules are pure geometry, so they are exercised without Revit.
    /// </para>
    /// </summary>
    public static class FloorLoopRules
    {
        /// <summary>Builds the plan of one floor from its contours.</summary>
        /// <param name="contours">Contours of the floor, in the order they were read.</param>
        /// <returns>The parts to create and the contours that cannot take part.</returns>
        public static FloorLoopPlan Classify(IReadOnlyList<FloorContour> contours)
        {
            var parts = new List<FloorPart>();
            var degenerate = new List<int>();
            var nested = new List<int>();
            var duplicates = new List<int>();

            if (contours == null || contours.Count == 0)
            {
                return new FloorLoopPlan(parts, degenerate, nested, duplicates);
            }

            int count = contours.Count;
            var excluded = new bool[count];
            var container = new int[count];

            for (int index = 0; index < count; index++)
            {
                container[index] = -1;

                if (contours[index] == null || contours[index].IsDegenerate)
                {
                    excluded[index] = true;
                    degenerate.Add(index);
                }
            }

            while (true)
            {
                AssignContainers(contours, excluded, container);

                int duplicate = -1;

                for (int index = 0; index < count && duplicate == -1; index++)
                {
                    if (excluded[index])
                    {
                        continue;
                    }

                    int parent = container[index];

                    if (parent != -1 && container[parent] == index)
                    {
                        duplicate = Math.Max(index, parent);
                    }
                }

                if (duplicate == -1)
                {
                    break;
                }

                // Two contours that each lie inside the other are one and the same; the
                // later one is dropped. The containers are worked out again, because
                // dropping a contour can change what contains what.
                excluded[duplicate] = true;
                duplicates.Add(duplicate);
            }

            var outers = new List<int>();
            var holesByOuter = new Dictionary<int, List<int>>();

            for (int index = 0; index < count; index++)
            {
                if (excluded[index])
                {
                    continue;
                }

                int parent = container[index];

                if (parent == -1)
                {
                    outers.Add(index);
                }
                else if (container[parent] == -1)
                {
                    if (!holesByOuter.TryGetValue(parent, out List<int>? holes))
                    {
                        holes = new List<int>();
                        holesByOuter[parent] = holes;
                    }

                    holes.Add(index);
                }
                else
                {
                    nested.Add(index);
                }
            }

            foreach (int outer in outers)
            {
                IReadOnlyList<int> holes = holesByOuter.TryGetValue(outer, out List<int>? found)
                    ? found
                    : new List<int>();

                parts.Add(new FloorPart(outer, holes));
            }

            return new FloorLoopPlan(parts, degenerate, nested, duplicates);
        }

        /// <summary>
        /// Works out, for every contour that takes part, the smallest contour that contains
        /// it. Only the first point of a contour is tested; valid floor sketches do not
        /// overlap, so a contour is either inside another one or outside it.
        /// </summary>
        private static void AssignContainers(
            IReadOnlyList<FloorContour> contours,
            bool[] excluded,
            int[] container)
        {
            for (int index = 0; index < contours.Count; index++)
            {
                container[index] = -1;

                if (excluded[index])
                {
                    continue;
                }

                Point3 first = contours[index].Points[0];

                for (int other = 0; other < contours.Count; other++)
                {
                    if (other == index || excluded[other] || !Contains(contours[other], first))
                    {
                        continue;
                    }

                    if (container[index] == -1 || contours[other].Area < contours[container[index]].Area)
                    {
                        container[index] = other;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether a point lies inside a contour, in the horizontal plane.
        /// A point exactly on the outline counts as inside for some edges and outside for
        /// others; sketches do not share outlines, so the ambiguity never decides a run.
        /// </summary>
        /// <param name="contour">Contour to test against.</param>
        /// <param name="point">Point to test.</param>
        public static bool Contains(FloorContour contour, Point3 point)
        {
            if (contour == null)
            {
                throw new ArgumentNullException(nameof(contour));
            }

            IReadOnlyList<Point3> points = contour.Points;
            bool inside = false;

            for (int index = 0, previous = points.Count - 1; index < points.Count; previous = index++)
            {
                double xi = points[index].X;
                double yi = points[index].Y;
                double xj = points[previous].X;
                double yj = points[previous].Y;

                bool crosses = ((yi > point.Y) != (yj > point.Y))
                    && (point.X < (((xj - xi) * (point.Y - yi) / (yj - yi)) + xi));

                if (crosses)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
