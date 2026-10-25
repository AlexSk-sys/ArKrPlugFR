using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Joins;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>How an unjoin run ended.</summary>
    internal enum UnjoinStatus
    {
        /// <summary>Nothing was selected.</summary>
        NoSelection,

        /// <summary>The selection holds no wall, or none of the walls has geometry.</summary>
        NoWalls,

        /// <summary>No cut had to be removed.</summary>
        NothingToDo,

        /// <summary>At least one cut was removed.</summary>
        Completed,
    }

    /// <summary>
    /// The result of an unjoin run: what was found, what was removed and what
    /// refused to change.
    /// </summary>
    internal sealed class UnjoinOutcome
    {
        private UnjoinOutcome(
            UnjoinStatus status,
            int wallCount,
            IReadOnlyList<JoinPair> pairs,
            int removedCount,
            IReadOnlyList<JoinPair> failed)
        {
            Status = status;
            WallCount = wallCount;
            Pairs = pairs;
            RemovedCount = removedCount;
            Failed = failed;
        }

        /// <summary>Gets how the run ended.</summary>
        public UnjoinStatus Status { get; }

        /// <summary>Gets the number of walls the run worked on.</summary>
        public int WallCount { get; }

        /// <summary>Gets the cuts that were offered for removal.</summary>
        public IReadOnlyList<JoinPair> Pairs { get; }

        /// <summary>Gets the number of cuts that were removed.</summary>
        public int RemovedCount { get; }

        /// <summary>Gets the cuts that the modelling software refused to change.</summary>
        public IReadOnlyList<JoinPair> Failed { get; }

        /// <summary>Creates the result of a run that had nothing selected.</summary>
        public static UnjoinOutcome NoSelection()
        {
            return new UnjoinOutcome(UnjoinStatus.NoSelection, 0, Array.Empty<JoinPair>(), 0, Array.Empty<JoinPair>());
        }

        /// <summary>Creates the result of a run whose selection holds no wall.</summary>
        public static UnjoinOutcome NoWalls()
        {
            return new UnjoinOutcome(UnjoinStatus.NoWalls, 0, Array.Empty<JoinPair>(), 0, Array.Empty<JoinPair>());
        }

        /// <summary>Creates the result of a run that removed nothing.</summary>
        public static UnjoinOutcome NothingToDo(int wallCount)
        {
            return new UnjoinOutcome(UnjoinStatus.NothingToDo, wallCount, Array.Empty<JoinPair>(), 0, Array.Empty<JoinPair>());
        }

        /// <summary>Creates the result of a completed run.</summary>
        public static UnjoinOutcome Completed(
            int wallCount,
            IReadOnlyList<JoinPair> pairs,
            int removedCount,
            IReadOnlyList<JoinPair> failed)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            if (failed == null)
            {
                throw new ArgumentNullException(nameof(failed));
            }

            if (removedCount <= 0)
            {
                return new UnjoinOutcome(UnjoinStatus.NothingToDo, wallCount, pairs, 0, failed);
            }

            return new UnjoinOutcome(UnjoinStatus.Completed, wallCount, pairs, removedCount, failed);
        }
    }
}
