using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Core.RebarMarks;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// What one run did: the counts the report shows, the lines about what was left out and
    /// the refusals.
    /// </summary>
    internal sealed class AreaRebarOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many sources the run looked at.</summary>
        internal int Sources { get; set; }

        /// <summary>Gets or sets how many sources carry no value in the parameter.</summary>
        internal int NoValue { get; set; }

        /// <summary>Gets or sets how many sources have no bars.</summary>
        internal int NoBars { get; set; }

        /// <summary>Gets or sets how many bars were filled.</summary>
        internal int Filled { get; set; }

        /// <summary>Gets or sets how many bars were kept because they already carry a value.</summary>
        internal int Kept { get; set; }

        /// <summary>Gets or sets how many bars were refused.</summary>
        internal int Refused { get; set; }

        /// <summary>Gets the first lines about what was left out, one line each.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Fills the bars of every area reinforcement from the field they belong to.
    /// <para>
    /// The bars of the project are read once and grouped by their host
    /// (<c>Rebar.GetHostId</c>). For every source the chosen parameter is read and written
    /// into the bars of that source — the instance parameter first, the type parameter
    /// second — while <see cref="FillRules"/> decides whether a bar receives the value at
    /// all.
    /// </para>
    /// </summary>
    internal static class AreaRebarEngine
    {
        /// <summary>How many lines about one kind are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Runs the fill.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="sources">Area reinforcement elements of the scope.</param>
        /// <param name="settings">Scope, parameter and the switch that protects existing values.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static AreaRebarOutcome Run(
            Document document,
            IReadOnlyList<Element> sources,
            AreaRebarSettings settings,
            AreaRebarText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
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

            var outcome = new AreaRebarOutcome();

            if (sources.Count == 0)
            {
                outcome.Stopped = text.NoSources;

                return outcome;
            }

            if (!ParameterNameRules.IsUsable(settings.ParameterName))
            {
                outcome.Stopped = text.ParameterRequired;

                return outcome;
            }

            var barsByHost = new Dictionary<long, List<Element>>();

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(Rebar)))
            {
                if (!(element is Rebar rebar))
                {
                    continue;
                }

                ElementId hostId = rebar.GetHostId();

                if (hostId == null || hostId == ElementId.InvalidElementId)
                {
                    continue;
                }

                long key = ElementIdCompat.GetValue(hostId);

                if (!barsByHost.TryGetValue(key, out List<Element>? bars))
                {
                    bars = new List<Element>();
                    barsByHost[key] = bars;
                }

                bars.Add(rebar);
            }

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                bool changed = false;

                foreach (Element source in sources)
                {
                    outcome.Sources++;

                    string sourceLabel = ElementDescription.Describe(source);

                    try
                    {
                        string? value = ParameterSupport.Read(source, settings.ParameterName);

                        if (value == null)
                        {
                            outcome.NoValue++;
                            Add(outcome.Notes, text.NoSourceValue(sourceLabel));

                            continue;
                        }

                        if (!barsByHost.TryGetValue(ElementIdCompat.GetValue(source.Id), out List<Element>? bars) || bars.Count == 0)
                        {
                            outcome.NoBars++;
                            Add(outcome.Notes, text.NoBars(sourceLabel));

                            continue;
                        }

                        foreach (Element bar in bars)
                        {
                            string barLabel = ElementDescription.Describe(bar);

                            try
                            {
                                string? current = ParameterSupport.Read(bar, settings.ParameterName);

                                if (!FillRules.ShouldFill(value, current, settings.OnlyEmpty))
                                {
                                    outcome.Kept++;

                                    continue;
                                }

                                if (ParameterSupport.TryWrite(bar, settings.ParameterName, value))
                                {
                                    outcome.Filled++;
                                    changed = true;
                                }
                                else
                                {
                                    outcome.Refused++;
                                    Add(outcome.Failures, text.NoTargetParameter(barLabel));
                                }
                            }
                            catch (Exception exception)
                            {
                                outcome.Refused++;
                                Add(outcome.Failures, text.WriteFailed(barLabel, exception.Message));
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Refused++;
                        Add(outcome.Failures, text.WriteFailed(sourceLabel, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "AreaRebarMarks: sources " + outcome.Sources + ", filled " + outcome.Filled +
                ", kept " + outcome.Kept + ", without a value " + outcome.NoValue +
                ", without bars " + outcome.NoBars + ", refused " + outcome.Refused + ".",
                nameof(AreaRebarEngine));

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
