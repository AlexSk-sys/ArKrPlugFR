using System;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Runs an edit as one undoable step.
    /// <para>
    /// A tool that writes must open a transaction, and it should not leave an
    /// empty entry in the user's undo list when it turns out that nothing needed
    /// changing. The body reports whether it changed anything, and the transaction
    /// is rolled back when it did not.
    /// </para>
    /// </summary>
    public static class DocumentEdit
    {
        /// <summary>Runs an edit and commits it only when the body reports a change.</summary>
        /// <param name="document">The document to edit.</param>
        /// <param name="transactionName">Name shown in the undo list.</param>
        /// <param name="body">
        /// The edit; returns <c>true</c> when the model changed and <c>false</c> when
        /// it did not.
        /// </param>
        /// <returns><c>true</c> when the edit was committed.</returns>
        public static bool Run(Document document, string transactionName, Func<bool> body)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (string.IsNullOrWhiteSpace(transactionName))
            {
                throw new ArgumentException("The transaction needs a name for the undo list.", nameof(transactionName));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            using (var transaction = new Transaction(document, transactionName))
            {
                transaction.Start();

                // The specs require that a bulk run never blocks on a platform
                // dialog part-way through: warnings are dropped, resolvable errors
                // are resolved and nothing is left to prompt for.
                FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new SilentFailures());
                options.SetClearAfterRollback(true);
                transaction.SetFailureHandlingOptions(options);

                bool changed;

                try
                {
                    changed = body();
                }
                catch (Exception)
                {
                    transaction.RollBack();
                    throw;
                }

                if (changed)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.RollBack();
                }

                return changed;
            }
        }
    }
}
