using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// The groups of views the transfer would copy, with planned unique names.
    /// </summary>
    public sealed class TransferPlan
    {
        /// <summary>Creates a plan.</summary>
        /// <param name="groups">Groups ordered the way the planner produced them.</param>
        public TransferPlan(IReadOnlyList<TransferGroup> groups)
        {
            Groups = groups ?? throw new ArgumentNullException(nameof(groups));
            TotalCount = groups.Sum(group => group.ItemCount);
        }

        /// <summary>Gets the groups of the plan.</summary>
        public IReadOnlyList<TransferGroup> Groups { get; }

        /// <summary>Gets the total number of views the plan would copy.</summary>
        public int TotalCount { get; }

        /// <summary>Gets a value indicating whether the plan copies nothing.</summary>
        public bool IsEmpty => TotalCount == 0;
    }
}
