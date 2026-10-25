using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Bridges the differences in <c>IndependentTag</c> between Revit releases.
    /// <para>
    /// Revit 2020 knows a tag of a single element; from 2022 a tag can carry several
    /// references and the plural accessor replaces the single one (the singular property
    /// is gone in 2024). Every read of a tag's hosts goes through here, so the tools
    /// never see the split.
    /// </para>
    /// </summary>
    public static class TagCompat
    {
        /// <summary>Returns the ids of the elements a tag tags, in the project.</summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The host ids; empty for an orphaned tag.</returns>
        public static IReadOnlyCollection<long> GetHostIds(IndependentTag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

#if REVIT_2020
            ElementId hostId = tag.TaggedLocalElementId;

            if (hostId == null || hostId == ElementId.InvalidElementId)
            {
                return Array.Empty<long>();
            }

            return new[] { ElementIdCompat.GetValue(hostId) };
#else
            ICollection<ElementId> hostIds = tag.GetTaggedLocalElementIds();

            if (hostIds == null || hostIds.Count == 0)
            {
                return Array.Empty<long>();
            }

            return hostIds
                .Where(id => id != null && id != ElementId.InvalidElementId)
                .Select(ElementIdCompat.GetValue)
                .ToArray();
#endif
        }
    }
}
