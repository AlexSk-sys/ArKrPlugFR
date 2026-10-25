using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Compare;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// What one copy did: the counts the report shows and the refusals.
    /// </summary>
    internal sealed class CompareOutcome
    {
        /// <summary>Gets or sets how many rows the comparison holds.</summary>
        internal int Compared { get; set; }

        /// <summary>Gets or sets how many rows differ.</summary>
        internal int Different { get; set; }

        /// <summary>Gets or sets how many values were copied.</summary>
        internal int Copied { get; set; }

        /// <summary>Gets or sets how many rows were skipped because the source value is empty.</summary>
        internal int Skipped { get; set; }

        /// <summary>Gets or sets how many values were refused.</summary>
        internal int Refused { get; set; }

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Copies the values of the source element into the target element.
    /// <para>
    /// The comparison is built in <see cref="ParameterComparison"/>; this class reads the
    /// values out of the model, writes the differing ones — the instance parameter first,
    /// the type parameter second — in one undoable step, and never clears a value.
    /// </para>
    /// </summary>
    internal static class CompareEngine
    {
        /// <summary>How many refusal lines are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Runs the copy.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="source">The element that carries the values worth keeping.</param>
        /// <param name="target">The element to fix.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static CompareOutcome Copy(
            Document document,
            Element source,
            Element target,
            CompareText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                ParameterSupport.ReadValues(source),
                ParameterSupport.ReadValues(target));

            var outcome = new CompareOutcome { Compared = rows.Count };

            foreach (ParameterRow row in rows)
            {
                if (row.Differs)
                {
                    outcome.Different++;
                }
            }

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                bool changed = false;

                foreach (ParameterRow row in rows)
                {
                    if (row.Source.Trim().Length == 0)
                    {
                        outcome.Skipped++;

                        continue;
                    }

                    if (!row.Differs)
                    {
                        continue;
                    }

                    try
                    {
                        if (ParameterSupport.TryWrite(target, row.Name, row.Source.Trim()))
                        {
                            outcome.Copied++;
                            changed = true;
                        }
                        else
                        {
                            outcome.Refused++;
                            Add(outcome.Failures, text.NoTarget(row.Name));
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Refused++;
                        Add(outcome.Failures, text.WriteFailed(row.Name, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "CompareParameters: compared " + outcome.Compared + ", different " + outcome.Different +
                ", copied " + outcome.Copied + ", skipped " + outcome.Skipped + ", refused " + outcome.Refused + ".",
                nameof(CompareEngine));

            return outcome;
        }

        private static void Add(List<string> lines, string line)
        {
            if (lines.Count < LineLimit)
            {
                lines.Add(line);
            }
        }
    }
}
