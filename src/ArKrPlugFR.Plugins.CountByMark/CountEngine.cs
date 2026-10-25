using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Marks;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>What one run did.</summary>
    internal sealed class CountOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        internal CountOutcome(
            int structures,
            int marks,
            int bars,
            int updated,
            int noHostMark,
            int noCount,
            int noParameter,
            int failed,
            IReadOnlyList<string> failures,
            string? stopped)
        {
            Structures = structures;
            Marks = marks;
            Bars = bars;
            Updated = updated;
            NoHostMark = noHostMark;
            NoCount = noCount;
            NoParameter = noParameter;
            Failed = failed;
            Failures = failures ?? Array.Empty<string>();
            Stopped = stopped;
        }

        internal int Structures { get; }

        internal int Marks { get; }

        internal int Bars { get; }

        internal int Updated { get; }

        internal int NoHostMark { get; }

        internal int NoCount { get; }

        internal int NoParameter { get; }

        internal int Failed { get; }

        internal IReadOnlyList<string> Failures { get; }

        internal string? Stopped { get; }
    }

    /// <summary>
    /// Counts the structures per mark and writes the numbers into the bars.
    /// <para>
    /// Structures are read through their mark parameter, bars through their structure
    /// mark and the number parameter; the parameter names come from the tool's settings,
    /// so nothing about a project's template is assumed. The writes run in one
    /// transaction and only ever touch the number parameter of bars.
    /// </para>
    /// </summary>
    internal static class CountEngine
    {
        /// <summary>Runs the count for one kind of structures.</summary>
        /// <param name="document">Project to count in.</param>
        /// <param name="categories">Categories of the chosen kind.</param>
        /// <param name="settings">The parameter names and the kind.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static CountOutcome Run(
            Document document,
            IReadOnlyCollection<BuiltInCategory> categories,
            CountSettings settings,
            CountText text,
            Log log)
        {
            var failures = new List<string>();

            IReadOnlyList<Element> structures = DocumentInfo.GetElements(document, categories);
            var marks = new List<string?>();

            foreach (Element structure in structures)
            {
                marks.Add(ReadMark(structure, settings.StructureMarkParameter));
            }

            IReadOnlyDictionary<string, int> counts = MarkTally.Count(marks);

            if (counts.Count == 0)
            {
                return new CountOutcome(0, 0, 0, 0, 0, 0, 0, 0, failures, text.NoStructures);
            }

            var bars = new List<Element>();

            foreach (Element element in new FilteredElementCollector(document)
                .OfClass(typeof(Rebar))
                .WhereElementIsNotElementType())
            {
                bars.Add(element);
            }

            if (bars.Count == 0)
            {
                return new CountOutcome(0, counts.Count, 0, 0, 0, 0, 0, 0, failures, text.NoBars);
            }

            var planned = new List<(Element Bar, Parameter Target, int Value)>();
            int noHostMark = 0;
            int noCount = 0;
            int noParameter = 0;

            foreach (Element bar in bars)
            {
                string? hostMark = ReadMark(bar, settings.HostMarkParameter);

                if (!MarkTally.TryKey(hostMark, out _))
                {
                    noHostMark++;
                    continue;
                }

                int count = MarkTally.Lookup(counts, hostMark);

                if (count == 0)
                {
                    noCount++;
                    continue;
                }

                Parameter? target = bar.LookupParameter(settings.CountParameter);

                if (target == null || target.IsReadOnly)
                {
                    noParameter++;
                    continue;
                }

                planned.Add((bar, target, count));
            }

            int updated = 0;
            int failed = failures.Count;

            if (planned.Count > 0)
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    foreach ((Element bar, Parameter target, int value) in planned)
                    {
                        try
                        {
                            Write(target, value);
                            updated++;

                            log.Info(
                                "CountByMark: wrote " + value + " into " + ElementDescription.Describe(bar) + ".",
                                nameof(CountEngine));
                        }
                        catch (Exception exception)
                        {
                            failed++;

                            failures.Add(text.WriteFailed(ElementDescription.Describe(bar), exception.Message));

                            log.Warn(
                                "CountByMark: " + ElementDescription.Describe(bar) +
                                " refused the write: " + exception.Message,
                                nameof(CountEngine),
                                exception);
                        }
                    }

                    return updated > 0;
                });
            }

            log.Info(
                "CountByMark: " + counts.Count + " mark(s) over " + structures.Count + " structure(s), " +
                bars.Count + " bar(s), " + updated + " updated, " + noHostMark + " without a structure mark, " +
                noCount + " without a count, " + noParameter + " without the parameter, " + failed + " refused.",
                nameof(CountEngine));

            return new CountOutcome(
                structures.Count, counts.Count, bars.Count, updated, noHostMark, noCount, noParameter, failed,
                failures, updated > 0 ? null : text.NothingWritten);
        }

        private static string? ReadMark(Element element, string parameterName)
        {
            Parameter? parameter = element.LookupParameter(parameterName);

            if (parameter == null)
            {
                return null;
            }

            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString();

                case StorageType.Integer:
                    return parameter.AsInteger().ToString(CultureInfo.InvariantCulture);

                default:
                    return null;
            }
        }

        private static void Write(Parameter parameter, int value)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    parameter.Set(value);
                    return;

                case StorageType.String:
                    parameter.Set(value.ToString(CultureInfo.InvariantCulture));
                    return;

                case StorageType.Double:
                    parameter.Set((double)value);
                    return;

                default:
                    throw new InvalidOperationException(
                        "The number parameter of the bar cannot hold a whole number.");
            }
        }
    }
}
