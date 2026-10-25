using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Annotations
{
    /// <summary>Which tags of a view a refresh run will nudge.</summary>
    public sealed class RefreshPlan
    {
        /// <summary>Creates a plan.</summary>
        /// <param name="tags">Ids of the tags to nudge, ordered and without repetitions.</param>
        /// <param name="orphaned">How many of the considered tags are orphaned.</param>
        public RefreshPlan(IEnumerable<long> tags, int orphaned)
        {
            Tags = tags == null ? Array.Empty<long>() : tags.ToArray();
            Orphaned = orphaned;
        }

        /// <summary>Gets the ids of the tags to nudge.</summary>
        public IReadOnlyList<long> Tags { get; }

        /// <summary>Gets how many of the considered tags are orphaned.</summary>
        public int Orphaned { get; }
    }

    /// <summary>
    /// The rule that reads a view's tags and the user's selection and decides what the
    /// refresh touches.
    /// <para>
    /// An empty selection means the whole view: the user who just wants the annotations
    /// on screen to catch up does not have to select anything. A selection narrows the
    /// run to the tags of the selected elements, so a stale tag can be refreshed without
    /// touching the rest of the drawing.
    /// </para>
    /// </summary>
    public static class RefreshPlanner
    {
        /// <summary>
        /// Builds the plan of a refresh run.
        /// </summary>
        /// <param name="tags">The tags of the view.</param>
        /// <param name="selected">
        /// Ids of the selected elements; empty or <c>null</c> selects the whole view.
        /// </param>
        /// <returns>The tags to nudge and the number of orphaned ones.</returns>
        public static RefreshPlan Build(IEnumerable<TagRecord> tags, IReadOnlyCollection<long>? selected)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            bool wholeView = selected == null || selected.Count == 0;

            var planned = new List<long>();
            var seen = new HashSet<long>();
            int orphaned = 0;

            foreach (TagRecord tag in tags)
            {
                if (!InScope(tag, selected, wholeView))
                {
                    continue;
                }

                if (tag.Orphaned)
                {
                    orphaned++;
                    continue;
                }

                if (seen.Add(tag.Id))
                {
                    planned.Add(tag.Id);
                }
            }

            planned.Sort();

            return new RefreshPlan(planned, orphaned);
        }

        private static bool InScope(TagRecord tag, IReadOnlyCollection<long>? selected, bool wholeView)
        {
            if (wholeView)
            {
                return true;
            }

            if (selected!.Contains(tag.Id))
            {
                return true;
            }

            foreach (long host in tag.HostIds)
            {
                if (selected.Contains(host))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
