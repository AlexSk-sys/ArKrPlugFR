using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Annotations
{
    /// <summary>
    /// One tag as the planning rules see it: its own id, the ids of the elements it
    /// tags, and whether the element it was tagging is gone.
    /// </summary>
    public sealed class TagRecord
    {
        /// <summary>Creates a record of one tag.</summary>
        /// <param name="tagId">Id of the tag.</param>
        /// <param name="hostIds">Ids of the elements the tag tags; empty for an orphan.</param>
        /// <param name="orphaned">Whether the tagged element is gone.</param>
        public TagRecord(long tagId, IEnumerable<long>? hostIds, bool orphaned)
        {
            Id = tagId;
            HostIds = hostIds == null ? Array.Empty<long>() : hostIds.ToArray();
            Orphaned = orphaned;
        }

        /// <summary>Gets the id of the tag.</summary>
        public long Id { get; }

        /// <summary>Gets the ids of the elements the tag tags.</summary>
        public IReadOnlyList<long> HostIds { get; }

        /// <summary>Gets a value indicating whether the tagged element is gone.</summary>
        public bool Orphaned { get; }
    }
}
