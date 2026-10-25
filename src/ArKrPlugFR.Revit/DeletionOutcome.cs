using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// What a delete operation did: the ids that went away and the ids that were
    /// refused, with a reason for each refusal.
    /// </summary>
    public sealed class DeletionOutcome
    {
        /// <summary>Creates an outcome.</summary>
        /// <param name="deleted">Ids that were deleted.</param>
        /// <param name="failures">One line per id that was not deleted.</param>
        public DeletionOutcome(IReadOnlyList<long> deleted, IReadOnlyList<string> failures)
        {
            Deleted = deleted ?? throw new ArgumentNullException(nameof(deleted));
            Failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        /// <summary>Gets the ids that were deleted.</summary>
        public IReadOnlyList<long> Deleted { get; }

        /// <summary>Gets one line per id that was not deleted.</summary>
        public IReadOnlyList<string> Failures { get; }
    }
}
