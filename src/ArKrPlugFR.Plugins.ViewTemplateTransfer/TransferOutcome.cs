using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// One view that could not be copied, with the reason reported by Revit.
    /// </summary>
    public sealed class CopyFailure
    {
        /// <summary>Creates a failure record.</summary>
        /// <param name="id">Element id of the view in the source project.</param>
        /// <param name="message">The error message, verbatim.</param>
        public CopyFailure(long id, string message)
        {
            Id = id;
            Message = message ?? string.Empty;
        }

        /// <summary>Gets the element id of the view in the source project.</summary>
        public long Id { get; }

        /// <summary>Gets the error message, verbatim.</summary>
        public string Message { get; }
    }

    /// <summary>
    /// What one transfer run did: how many views arrived and which ones did not.
    /// </summary>
    public sealed class TransferOutcome
    {
        /// <summary>Creates an outcome.</summary>
        /// <param name="copiedCount">Number of views copied into the target.</param>
        /// <param name="failures">Views that could not be copied.</param>
        public TransferOutcome(int copiedCount, IReadOnlyList<CopyFailure> failures)
        {
            if (copiedCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(copiedCount));
            }

            CopiedCount = copiedCount;
            Failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        /// <summary>Gets the number of views copied into the target.</summary>
        public int CopiedCount { get; }

        /// <summary>Gets the views that could not be copied.</summary>
        public IReadOnlyList<CopyFailure> Failures { get; }

        /// <summary>Gets a value indicating whether every requested view was copied.</summary>
        public bool IsCompletelySuccessful => Failures.Count == 0;
    }
}
