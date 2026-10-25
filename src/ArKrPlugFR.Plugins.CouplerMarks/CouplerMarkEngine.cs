using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Couplers;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.CouplerMarks
{
    /// <summary>
    /// What one run did: the counts the report shows, the lines about what was left out and
    /// the refusals.
    /// </summary>
    internal sealed class CouplerMarkOutcome
    {
        /// <summary>Gets or sets the reason the run did nothing, or <c>null</c> when it ran.</summary>
        internal string? Stopped { get; set; }

        /// <summary>Gets or sets how many couplers the run looked at.</summary>
        internal int Considered { get; set; }

        /// <summary>Gets or sets how many couplers were given both values.</summary>
        internal int Updated { get; set; }

        /// <summary>Gets or sets how many couplers had no bar inside the tolerance.</summary>
        internal int NoBarNearby { get; set; }

        /// <summary>Gets or sets how many couplers whose bar host carries no mark or no category.</summary>
        internal int NoHostData { get; set; }

        /// <summary>Gets or sets how many couplers were refused.</summary>
        internal int Failed { get; set; }

        /// <summary>Gets the first lines about what was left out, one line each.</summary>
        internal List<string> Notes { get; } = new List<string>();

        /// <summary>Gets the first refusals, one line each.</summary>
        internal List<string> Failures { get; } = new List<string>();
    }

    /// <summary>
    /// Marks every picked coupler with the identity of the bar nearest to it.
    /// <para>
    /// The bars of the project are read once: the centre of every bar's box and its host.
    /// For every coupler the nearest bar inside the tolerance is looked up (in
    /// <see cref="CouplerRules"/>), and the mark and the category of the host are written
    /// into the two named parameters of the coupler — both or neither, so a coupler never
    /// ends up half marked.
    /// </para>
    /// </summary>
    internal static class CouplerMarkEngine
    {
        /// <summary>How many lines about one kind are kept for the report.</summary>
        private const int LineLimit = 10;

        /// <summary>Runs the marking.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="couplers">Picked couplers.</param>
        /// <param name="settings">Tolerance and parameter names.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static CouplerMarkOutcome Run(
            Document document,
            IReadOnlyList<Element> couplers,
            CouplerSettings settings,
            CouplerText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (couplers == null)
            {
                throw new ArgumentNullException(nameof(couplers));
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

            var outcome = new CouplerMarkOutcome { Considered = couplers.Count };

            if (couplers.Count == 0)
            {
                outcome.Stopped = text.NoCouplers;

                return outcome;
            }

            if (!ParameterNameRules.IsUsable(settings.MarkParameter)
                || !ParameterNameRules.IsUsable(settings.CategoryParameter))
            {
                outcome.Stopped = text.ParametersRequired;

                return outcome;
            }

            if (!CouplerRules.IsUsableTolerance(settings.ToleranceMillimeters))
            {
                outcome.Stopped = text.ToleranceRequired;

                return outcome;
            }

            var bars = new List<BarFact>();
            var centres = new List<Point3>();

            foreach (Element element in new FilteredElementCollector(document).OfClass(typeof(Rebar)))
            {
                if (!(element is Rebar rebar))
                {
                    continue;
                }

                Point3? centre = CouplerSupport.CentreOf(rebar);

                if (centre == null)
                {
                    continue;
                }

                var fact = new BarFact(centre.Value, document.GetElement(rebar.GetHostId()));

                bars.Add(fact);
                centres.Add(fact.Centre);
            }

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                bool changed = false;

                foreach (Element coupler in couplers)
                {
                    string label = ElementDescription.Describe(coupler);

                    try
                    {
                        Segment3? axis = CouplerSupport.AxisOf(coupler);

                        if (axis == null)
                        {
                            outcome.Failed++;
                            Add(outcome.Failures, text.NoGeometry(label));

                            continue;
                        }

                        int nearest = CouplerRules.FindNearest(axis.Value, centres, settings.ToleranceMillimeters);

                        if (nearest < 0)
                        {
                            outcome.NoBarNearby++;
                            Add(outcome.Notes, text.NoBarNearby(label));

                            continue;
                        }

                        Element? host = bars[nearest].Host;
                        string? mark = ReadMark(host);
                        string? category = host?.Category?.Name;

                        if (mark == null || category == null)
                        {
                            outcome.NoHostData++;
                            Add(outcome.Notes, text.NoHostData(label));

                            continue;
                        }

                        if (!ParameterSupport.CanWrite(coupler, settings.MarkParameter)
                            || !ParameterSupport.CanWrite(coupler, settings.CategoryParameter))
                        {
                            outcome.Failed++;
                            Add(outcome.Failures, text.NoTargetParameter(label));

                            continue;
                        }

                        bool markWritten = ParameterSupport.TryWrite(coupler, settings.MarkParameter, mark);
                        bool categoryWritten = ParameterSupport.TryWrite(coupler, settings.CategoryParameter, category);

                        if (markWritten && categoryWritten)
                        {
                            outcome.Updated++;
                            changed = true;
                        }
                        else
                        {
                            outcome.Failed++;
                            Add(outcome.Failures, text.HalfPair(label));
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
                "CouplerMarks: read " + outcome.Considered + ", updated " + outcome.Updated +
                ", no bar in reach " + outcome.NoBarNearby + ", host without data " + outcome.NoHostData +
                ", refused " + outcome.Failed + ".",
                nameof(CouplerMarkEngine));

            return outcome;
        }

        private static string? ReadMark(Element? host)
        {
            if (host == null)
            {
                return null;
            }

            Parameter? mark = host.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

            if (mark == null || !mark.HasValue)
            {
                return null;
            }

            string? value = mark.AsString();

            if (value == null)
            {
                value = mark.AsValueString();
            }

            return TransferValue.Normalize(value);
        }

        private static void Add(List<string> lines, string line)
        {
            if (lines.Count < LineLimit)
            {
                lines.Add(line);
            }
        }

        private sealed class BarFact
        {
            internal BarFact(Point3 centre, Element? host)
            {
                Centre = centre;
                Host = host;
            }

            internal Point3 Centre { get; }

            internal Element? Host { get; }
        }
    }
}
