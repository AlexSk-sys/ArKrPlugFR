using System.Collections.Generic;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// What one transfer did: the counts the report shows, the lines about what was left
    /// out and the refusals.
    /// </summary>
    internal sealed class TransferOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many elements the run looked at.</summary>
        internal int Considered { get; set; }

        /// <summary>Gets or sets how many values were written.</summary>
        internal int Written { get; set; }

        /// <summary>Gets or sets how many sources held no value.</summary>
        internal int Empty { get; set; }

        /// <summary>Gets or sets how many elements were skipped as nothing to read.</summary>
        internal int Skipped { get; set; }

        /// <summary>Gets or sets how many elements the target parameter is missing from.</summary>
        internal int WithoutParameter { get; set; }

        /// <summary>Gets or sets how many elements were refused by the platform.</summary>
        internal int Failed { get; set; }

        /// <summary>Gets the first lines about what was left out, one line each.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }
}
