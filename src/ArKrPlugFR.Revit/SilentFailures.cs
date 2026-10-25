using System;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Keeps a bulk edit from stopping on a dialog.
    /// <para>
    /// A tool that writes hundreds of elements raises warnings the platform would
    /// normally put in front of the user, and an unanswered modal prompt blocks the
    /// run. Warnings are therefore deleted, a failure the platform can resolve is
    /// resolved on the spot, and anything else rolls the whole edit back instead of
    /// asking a question in the middle of the operation.
    /// </para>
    /// </summary>
    internal sealed class SilentFailures : IFailuresPreprocessor
    {
        /// <inheritdoc />
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            if (failuresAccessor == null)
            {
                return FailureProcessingResult.Continue;
            }

            foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
            {
                if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                    continue;
                }

                try
                {
                    failuresAccessor.ResolveFailure(failure);
                }
                catch (Exception)
                {
                    // Only a real answer from the user could settle this one, and the
                    // run has no way to ask: rolling the edit back keeps the model
                    // consistent without a prompt.
                    return FailureProcessingResult.ProceedWithRollBack;
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}
