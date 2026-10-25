using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// What one run did: how many copies were created and which of them could not be
    /// made. A copy that fails is skipped and the run carries on, so the failures are
    /// the only record of what was left out.
    /// </summary>
    public sealed class DuplicateRunResult
    {
        /// <summary>Creates the result of one run.</summary>
        /// <param name="created">Copies that were created.</param>
        /// <param name="failures">One line per copy that could not be made.</param>
        public DuplicateRunResult(int created, IReadOnlyList<string> failures)
        {
            Created = created;
            Failures = failures ?? Array.Empty<string>();
        }

        /// <summary>Gets the number of copies that were created.</summary>
        public int Created { get; }

        /// <summary>Gets one line per copy that could not be made.</summary>
        public IReadOnlyList<string> Failures { get; }
    }
}
