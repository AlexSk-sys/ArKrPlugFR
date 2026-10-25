using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Writes one value into the chosen parameter of the ticked sheets.
    /// <para>
    /// The value is read once, before the transaction, so a run writes the same text
    /// everywhere. An empty value stops the run: the tool transfers a value and never
    /// clears a parameter. A sheet whose parameter is missing, read-only or not a text
    /// parameter is reported, and the run continues with the rest.
    /// </para>
    /// </summary>
    internal static class SheetTransferEngine
    {
        /// <summary>Runs the transfer.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="sheets">Ticked sheets.</param>
        /// <param name="sourceValue">Value read from the source element.</param>
        /// <param name="targetName">Name of the parameter to write.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static TransferOutcome Run(
            Document document,
            IReadOnlyList<ViewSheet> sheets,
            string sourceValue,
            string targetName,
            TransferText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (sheets == null)
            {
                throw new ArgumentNullException(nameof(sheets));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new TransferOutcome { Considered = sheets.Count };

            string? value = TransferValue.Normalize(sourceValue);

            if (value == null)
            {
                outcome.Stopped = text.NoSourceValue;

                return outcome;
            }

            DocumentEdit.Run(document, text.SheetTransactionName, () =>
            {
                bool changed = false;

                foreach (ViewSheet sheet in sheets)
                {
                    string label = ElementDescription.Describe(sheet);

                    try
                    {
                        if (ParameterSupport.TryWrite(sheet, targetName, value))
                        {
                            outcome.Written++;
                            changed = true;
                        }
                        else
                        {
                            outcome.WithoutParameter++;
                            TransferNotes.Add(outcome.Notes, text.NoTargetParameter(label));
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Failed++;
                        TransferNotes.Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "TransferParameters/sheets: read " + outcome.Considered + ", written " + outcome.Written +
                ", without the parameter " + outcome.WithoutParameter + ", refused " + outcome.Failed + ".",
                nameof(SheetTransferEngine));

            return outcome;
        }
    }
}
