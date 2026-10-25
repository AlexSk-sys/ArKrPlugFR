using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// What one export did: the counts the report shows and the refusals.
    /// </summary>
    internal sealed class ConcreteExportOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many elements the run looked at.</summary>
        internal int Scanned { get; set; }

        /// <summary>Gets or sets how many rows were written.</summary>
        internal int Exported { get; set; }

        /// <summary>Gets or sets how many elements were left out as not concrete.</summary>
        internal int NotConcrete { get; set; }

        /// <summary>Gets or sets how many elements could not be read.</summary>
        internal int Failed { get; set; }

        /// <summary>Gets or sets the path the file was written to.</summary>
        internal string FilePath { get; set; } = string.Empty;

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Writes the concrete structures of the scope into a comma-separated file.
    /// <para>
    /// The rows are built in <see cref="ConcreteExportSupport"/> and formatted by
    /// <see cref="CsvRules"/>; this class decides the scope, walks the elements and writes
    /// the file as UTF-8 with a byte-order mark so a spreadsheet opens it correctly. The
    /// model is only read.
    /// </para>
    /// </summary>
    internal static class ConcreteExportEngine
    {
        /// <summary>How many refusal lines are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Runs the export.</summary>
        /// <param name="document">Project to read.</param>
        /// <param name="selection">Elements of the current selection.</param>
        /// <param name="onlySelection">Whether only the selection is exported.</param>
        /// <param name="filePath">File to write.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static ConcreteExportOutcome Run(
            Document document,
            IReadOnlyList<Element> selection,
            bool onlySelection,
            string filePath,
            ConcreteExportText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new ConcreteExportOutcome { FilePath = filePath ?? string.Empty };
            string path = outcome.FilePath;

            if (string.IsNullOrWhiteSpace(path))
            {
                outcome.Stopped = text.FileRequired;

                return outcome;
            }

            IReadOnlyList<Element> elements = onlySelection ? selection : CollectStructures(document);
            var builder = new StringBuilder();

            builder.AppendLine(CsvRules.Row(ConcreteExportSupport.Header));

            foreach (Element element in elements)
            {
                outcome.Scanned++;

                string label = ElementDescription.Describe(element);

                try
                {
                    if (!ConcreteExportSupport.IsConcrete(document, element))
                    {
                        outcome.NotConcrete++;

                        continue;
                    }

                    builder.AppendLine(CsvRules.Row(ConcreteExportSupport.Row(document, element)));
                    outcome.Exported++;
                }
                catch (Exception exception)
                {
                    outcome.Failed++;
                    Add(outcome.Failures, text.RowFailed(label, exception.Message));
                }
            }

            if (outcome.Exported == 0)
            {
                outcome.Stopped = text.NothingToExport;

                return outcome;
            }

            try
            {
                File.WriteAllText(path, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }
            catch (Exception exception)
            {
                outcome.Stopped = text.FileFailed(path, exception.Message);

                return outcome;
            }

            log.Info(
                "ConcreteExport: scanned " + outcome.Scanned + ", exported " + outcome.Exported +
                ", not concrete " + outcome.NotConcrete + ", refused " + outcome.Failed + ".",
                nameof(ConcreteExportEngine));

            return outcome;
        }

        private static IReadOnlyList<Element> CollectStructures(Document document)
        {
            var categories = new List<BuiltInCategory>(StructureFactsReader.SupportedCategories);
            var filter = new ElementMulticategoryFilter(categories);
            var elements = new List<Element>();

            foreach (Element element in new FilteredElementCollector(document).WhereElementIsNotElementType().WherePasses(filter))
            {
                elements.Add(element);
            }

            return elements;
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
