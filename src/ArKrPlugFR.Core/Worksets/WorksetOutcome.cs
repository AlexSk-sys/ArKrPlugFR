using System.Collections.Generic;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// What one run did: the counts the report shows and the failures it lists.
    /// </summary>
    public sealed class WorksetOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        public string? Stopped { get; set; }

        /// <summary>Gets or sets how many elements the run looked at.</summary>
        public int Considered { get; set; }

        /// <summary>Gets or sets how many elements were put into their workset.</summary>
        public int Assigned { get; set; }

        /// <summary>Gets or sets how many elements already sat in the workset the rules ask for.</summary>
        public int AlreadyInTarget { get; set; }

        /// <summary>Gets or sets how many elements matched no rule.</summary>
        public int NoMatch { get; set; }

        /// <summary>Gets or sets how many elements needed a workset that does not exist and may not be created.</summary>
        public int WorksetMissing { get; set; }

        /// <summary>Gets or sets how many worksets the run created.</summary>
        public int CreatedWorksets { get; set; }

        /// <summary>Gets or sets how many elements or worksets were refused.</summary>
        public int Failed { get; set; }

        /// <summary>Gets the first failures, one line each, for the report.</summary>
        public List<string> Failures { get; } = new List<string>();
    }
}
