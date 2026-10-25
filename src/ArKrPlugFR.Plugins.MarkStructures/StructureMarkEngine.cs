using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Marking;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// What one run did: the counts the report shows, the lines about what was left out and
    /// the refusals.
    /// </summary>
    internal sealed class StructureMarkOutcome
    {
        /// <summary>Gets or sets how many elements the run looked at.</summary>
        internal int Considered { get; set; }

        /// <summary>Gets or sets how many elements were marked.</summary>
        internal int Marked { get; set; }

        /// <summary>Gets or sets how many elements already carried a mark.</summary>
        internal int AlreadyMarked { get; set; }

        /// <summary>Gets or sets how many elements had no kind.</summary>
        internal int NoKind { get; set; }

        /// <summary>Gets or sets how many elements belong to a kind with an empty code.</summary>
        internal int NoMarkCode { get; set; }

        /// <summary>Gets or sets how many elements were refused.</summary>
        internal int Failed { get; set; }

        /// <summary>Gets the first lines about what was left out, one line each.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Walks the structures of the project and writes the mark every one of them asks for.
    /// <para>
    /// The decision is made in <see cref="MarkingRules"/>: the kind of an element from its
    /// category and its shape, the material family from the names of its materials, the
    /// mark from the configured code and suffix. This class only reads the facts out of the
    /// model and writes the mark parameter in one undoable step.
    /// </para>
    /// </summary>
    internal static class StructureMarkEngine
    {
        /// <summary>How many lines about one kind are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Runs the marking.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="settings">Codes, suffixes and the switch that protects existing marks.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static StructureMarkOutcome Run(
            Document document,
            MarkingSettings settings,
            MarkText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new StructureMarkOutcome();

            var categories = new List<BuiltInCategory>(StructureFactsReader.SupportedCategories);
            var filter = new ElementMulticategoryFilter(categories);
            var elements = new List<Element>();

            foreach (Element element in new FilteredElementCollector(document).WhereElementIsNotElementType().WherePasses(filter))
            {
                elements.Add(element);
            }

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                bool changed = false;

                foreach (Element element in elements)
                {
                    outcome.Considered++;

                    string label = ElementDescription.Describe(element);

                    try
                    {
                        StructureFacts facts = StructureFactsReader.Read(document, element);
                        StructureKind? kind = MarkingRules.Detect(facts);

                        if (kind == null)
                        {
                            outcome.NoKind++;

                            continue;
                        }

                        string mark = MarkingRules.Mark(settings, kind.Value, MarkingRules.Material(facts.MaterialNames));

                        if (mark.Length == 0)
                        {
                            outcome.NoMarkCode++;
                            Add(outcome.Notes, text.NoMarkCode(label, kind.Value));

                            continue;
                        }

                        Parameter? parameter = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

                        if (parameter == null || parameter.IsReadOnly || parameter.StorageType != StorageType.String)
                        {
                            outcome.Failed++;
                            Add(outcome.Failures, text.NoTargetParameter(label));

                            continue;
                        }

                        string current = parameter.AsString() ?? string.Empty;

                        if (settings.OnlyEmpty && current.Trim().Length > 0)
                        {
                            outcome.AlreadyMarked++;

                            continue;
                        }

                        if (parameter.Set(mark))
                        {
                            outcome.Marked++;
                            changed = true;
                        }
                        else
                        {
                            outcome.Failed++;
                            Add(outcome.Failures, text.WriteRefused(label));
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Failed++;
                        Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "MarkStructures: read " + outcome.Considered + ", marked " + outcome.Marked +
                ", already marked " + outcome.AlreadyMarked + ", without a kind " + outcome.NoKind +
                ", without a mark code " + outcome.NoMarkCode + ", refused " + outcome.Failed + ".",
                nameof(StructureMarkEngine));

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
