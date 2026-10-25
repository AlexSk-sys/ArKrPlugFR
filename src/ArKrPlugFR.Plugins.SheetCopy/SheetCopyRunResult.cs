using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// What the run did to one target project: how many copies it created out of the
    /// planned ones, which sheets failed on the way, and whether anything was
    /// committed to that project at all.
    /// </summary>
    public sealed class TargetCopyResult
    {
        /// <summary>Creates the result of one target project.</summary>
        /// <param name="targetName">How the project is named in the report.</param>
        /// <param name="created">Copies that were created.</param>
        /// <param name="planned">Copies that were planned.</param>
        /// <param name="failures">One line per sheet that could not be copied.</param>
        /// <param name="committed">Whether the project was changed at all.</param>
        public TargetCopyResult(
            string targetName,
            int created,
            int planned,
            IReadOnlyList<string> failures,
            bool committed)
        {
            TargetName = targetName ?? string.Empty;
            Created = created;
            Planned = planned;
            Failures = failures ?? Array.Empty<string>();
            Committed = committed;
        }

        /// <summary>Gets how the project is named in the report.</summary>
        public string TargetName { get; }

        /// <summary>Gets the number of copies that were created.</summary>
        public int Created { get; }

        /// <summary>Gets the number of copies that were planned.</summary>
        public int Planned { get; }

        /// <summary>Gets one line per sheet that could not be copied.</summary>
        public IReadOnlyList<string> Failures { get; }

        /// <summary>Gets a value indicating whether the project was changed at all.</summary>
        public bool Committed { get; }

        /// <summary>Gets a value indicating whether every planned copy arrived.</summary>
        public bool IsComplete => Failures.Count == 0 && Created == Planned;
    }

    /// <summary>
    /// What a whole run did: one result per target project, because the same sheets
    /// go into every ticked project and each of them can succeed or fail on its own.
    /// </summary>
    public sealed class SheetCopyRunResult
    {
        /// <summary>Creates the result of a run.</summary>
        /// <param name="targets">One result per project the run went into.</param>
        public SheetCopyRunResult(IReadOnlyList<TargetCopyResult> targets)
        {
            Targets = targets ?? Array.Empty<TargetCopyResult>();
        }

        /// <summary>Gets one result per target project.</summary>
        public IReadOnlyList<TargetCopyResult> Targets { get; }

        /// <summary>Gets the number of copies created over every target together.</summary>
        public int Created
        {
            get
            {
                int total = 0;

                foreach (TargetCopyResult target in Targets)
                {
                    total += target.Created;
                }

                return total;
            }
        }

        /// <summary>Gets the number of copies planned over every target together.</summary>
        public int Planned
        {
            get
            {
                int total = 0;

                foreach (TargetCopyResult target in Targets)
                {
                    total += target.Planned;
                }

                return total;
            }
        }

        /// <summary>Gets a value indicating whether any sheet failed in any project.</summary>
        public bool HasFailures
        {
            get
            {
                foreach (TargetCopyResult target in Targets)
                {
                    if (target.Failures.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
