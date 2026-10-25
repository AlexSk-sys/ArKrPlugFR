using System;
using System.Collections.Generic;
using System.Globalization;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.HostChange;
using ArKrPlugFR.Core.Marks;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.ReassignRebar
{
    /// <summary>What one run did.</summary>
    internal sealed class ReassignOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        internal ReassignOutcome(
            int picked,
            int reassigned,
            int ifcUpdated,
            int skipped,
            int refused,
            IReadOnlyList<string> failures,
            string? stopped)
        {
            Picked = picked;
            Reassigned = reassigned;
            IfcUpdated = ifcUpdated;
            Skipped = skipped;
            Refused = refused;
            Failures = failures ?? Array.Empty<string>();
            Stopped = stopped;
        }

        internal int Picked { get; }

        internal int Reassigned { get; }

        internal int IfcUpdated { get; }

        internal int Skipped { get; }

        internal int Refused { get; }

        internal IReadOnlyList<string> Failures { get; }

        internal string? Stopped { get; }
    }

    /// <summary>
    /// Attaches the picked rebar to the picked structure.
    /// <para>
    /// A real bar changes its host through the platform's own setter, which refuses
    /// shapes the new host cannot carry; a bar that arrived from IFC only gets the host's
    /// mark written into it; a bar inside a system and anything else are left alone.
    /// One transaction covers the whole run.
    /// </para>
    /// </summary>
    internal static class ReassignEngine
    {
        /// <summary>Runs the reassignment.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="picked">Elements the user picked as rebar.</param>
        /// <param name="host">Structure the rebar is attached to.</param>
        /// <param name="settings">The parameter names.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static ReassignOutcome Run(
            Document document,
            IReadOnlyList<Element> picked,
            Element host,
            ReassignSettings settings,
            ReassignText text,
            Log log)
        {
            string? mark = ReadMark(host, settings.MarkParameter);

            if (!MarkTally.TryKey(mark, out string hostMark)
                && !MarkTally.TryKey(ReadMark(host, "Mark"), out hostMark))
            {
                log.Warn(
                    "ReassignRebar: host " + ElementDescription.Describe(host) + " has no mark.",
                    nameof(ReassignEngine));

                return new ReassignOutcome(picked.Count, 0, 0, 0, 0, Array.Empty<string>(), text.HostWithoutMark);
            }

            int reassigned = 0;
            int ifcUpdated = 0;
            int skipped = 0;
            int refused = 0;
            var failures = new List<string>();

            DocumentEdit.Run(document, text.TransactionName, () =>
            {
                foreach (Element element in picked)
                {
                    HostChangeAction action = HostChangeRules.Route(Classify(element));

                    if (action == HostChangeAction.ChangeHost)
                    {
                        try
                        {
                            ((Rebar)element).SetHostId(document, host.Id);
                            reassigned++;

                            log.Info(
                                "ReassignRebar: re-hosted " + ElementDescription.Describe(element) +
                                " to " + ElementDescription.Describe(host) + ".",
                                nameof(ReassignEngine));
                        }
                        catch (Exception exception)
                        {
                            refused++;

                            failures.Add(text.Refused(ElementDescription.Describe(element), exception.Message));

                            log.Warn(
                                "ReassignRebar: " + ElementDescription.Describe(element) +
                                " refused the new host: " + exception.Message,
                                nameof(ReassignEngine),
                                exception);
                        }

                        continue;
                    }

                    if (action == HostChangeAction.UpdateIfcMark)
                    {
                        Parameter? target = element.LookupParameter(settings.IfcMarkParameter);

                        if (target == null || target.IsReadOnly || target.StorageType != StorageType.String)
                        {
                            skipped++;
                            continue;
                        }

                        try
                        {
                            target.Set(hostMark);
                            ifcUpdated++;
                        }
                        catch (Exception exception)
                        {
                            refused++;

                            failures.Add(text.Refused(ElementDescription.Describe(element), exception.Message));
                        }

                        continue;
                    }

                    skipped++;
                }

                return reassigned + ifcUpdated > 0;
            });

            log.Info(
                "ReassignRebar: " + picked.Count + " picked, " + reassigned + " reassigned, " +
                ifcUpdated + " IFC marks, " + skipped + " skipped, " + refused + " refused.",
                nameof(ReassignEngine));

            return new ReassignOutcome(
                picked.Count, reassigned, ifcUpdated, skipped, refused, failures, null);
        }

        /// <summary>Classifies a picked element by its own type.</summary>
        /// <param name="element">The element to classify.</param>
        /// <returns>The kind of the element.</returns>
        internal static RebarKind Classify(Element element)
        {
            if (element is Rebar)
            {
                return RebarKind.Bar;
            }

            if (element is RebarInSystem)
            {
                return RebarKind.BarInSystem;
            }

            if (element is FamilyInstance && ElementScope.HasCategory(element, BuiltInCategory.OST_Rebar))
            {
                return RebarKind.IfcBar;
            }

            return RebarKind.Other;
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
    }
}
