using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Writes the layer structure of the picked walls and floors into a parameter, as
    /// text with one layer per line.
    /// <para>
    /// The layers are read from the compound structure of the type; an element without a
    /// compound structure — a basic wall without layers, a curtain wall — is reported and
    /// skipped. The write goes to the instance parameter first and to the type parameter
    /// second, so a wall type can be described once for all of its walls.
    /// </para>
    /// </summary>
    internal static class LayerTransferEngine
    {
        /// <summary>Runs the write.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="elements">Picked walls and floors.</param>
        /// <param name="parameterName">Name of the parameter to write.</param>
        /// <param name="toType">Whether the write goes to the type instead of the instance.</param>
        /// <param name="culture">Culture the layer text is written in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static TransferOutcome Run(
            Document document,
            IReadOnlyList<Element> elements,
            string parameterName,
            bool toType,
            string culture,
            TransferText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new TransferOutcome { Considered = elements.Count };

            if (!ParameterNameRules.IsUsable(parameterName))
            {
                outcome.Stopped = text.TargetRequired;

                return outcome;
            }

            DocumentEdit.Run(document, text.LayerTransactionName, () =>
            {
                bool changed = false;

                foreach (Element element in elements)
                {
                    string label = ElementDescription.Describe(element);

                    try
                    {
                        IReadOnlyList<LayerFact>? layers = LayerSupport.ReadLayers(document, element);

                        if (layers == null)
                        {
                            outcome.Skipped++;
                            TransferNotes.Add(outcome.Notes, text.NoLayers(label));

                            continue;
                        }

                        if (layers.Count == 0)
                        {
                            outcome.Skipped++;
                            TransferNotes.Add(outcome.Notes, text.EmptyLayers(label));

                            continue;
                        }

                        string value = LayerTextRules.Format(layers, culture);
                        Element? target = toType ? ParameterSupport.TypeOf(element) : element;

                        if (target == null || !ParameterSupport.TryWrite(target, parameterName, value))
                        {
                            outcome.WithoutParameter++;
                            TransferNotes.Add(outcome.Notes, text.NoTargetParameter(label));

                            continue;
                        }

                        outcome.Written++;
                        changed = true;
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
                "TransferParameters/layers: read " + outcome.Considered + ", written " + outcome.Written +
                ", skipped " + outcome.Skipped + ", without the parameter " + outcome.WithoutParameter +
                ", refused " + outcome.Failed + ".",
                nameof(LayerTransferEngine));

            return outcome;
        }
    }
}
