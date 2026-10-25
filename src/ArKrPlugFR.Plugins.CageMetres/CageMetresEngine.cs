using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArKrPlugFR.Core.Cages;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>What one run measured.</summary>
    internal sealed class CageOutcome
    {
        /// <summary>Creates the result of a run.</summary>
        internal CageOutcome(
            string family,
            int lineCount,
            double lineMetres,
            int longitudinalBars,
            double inclinedBarMetres,
            int inclinedPositions,
            int transversePositions,
            IReadOnlyList<double> transverseBarMetres,
            double longitudinalMetres,
            double inclinedMetres,
            double transverseMetres,
            double totalMetres,
            string? writeNote,
            IReadOnlyList<string> warnings)
        {
            Family = family;
            LineCount = lineCount;
            LineMetres = lineMetres;
            LongitudinalBars = longitudinalBars;
            InclinedBarMetres = inclinedBarMetres;
            InclinedPositions = inclinedPositions;
            TransversePositions = transversePositions;
            TransverseBarMetres = transverseBarMetres ?? Array.Empty<double>();
            LongitudinalMetres = longitudinalMetres;
            InclinedMetres = inclinedMetres;
            TransverseMetres = transverseMetres;
            TotalMetres = totalMetres;
            WriteNote = writeNote;
            Warnings = warnings ?? Array.Empty<string>();
        }

        internal string Family { get; }

        internal int LineCount { get; }

        internal double LineMetres { get; }

        internal int LongitudinalBars { get; }

        internal double InclinedBarMetres { get; }

        internal int InclinedPositions { get; }

        internal int TransversePositions { get; }

        internal IReadOnlyList<double> TransverseBarMetres { get; }

        internal double LongitudinalMetres { get; }

        internal double InclinedMetres { get; }

        internal double TransverseMetres { get; }

        internal double TotalMetres { get; }

        internal string? WriteNote { get; }

        internal IReadOnlyList<string> Warnings { get; }
    }

    /// <summary>
    /// Measures a cage along its picked layout lines and writes the running metres into it.
    /// <para>
    /// The lengths come from the cage itself: the number of longitudinal bars and the two
    /// steps from its parameters, the bar lengths from its sub-components, sorted by the
    /// group flags. The parameter names come from the tool's settings, so nothing about a
    /// project's template is assumed, and the arithmetic lives in
    /// <see cref="CageRules"/> where it is tested without Revit.
    /// </para>
    /// </summary>
    internal static class CageMetresEngine
    {
        /// <summary>Runs the measurement for one cage.</summary>
        /// <param name="document">Project the cage lives in.</param>
        /// <param name="lines">The picked layout lines.</param>
        /// <param name="cage">The picked cage.</param>
        /// <param name="settings">The parameter names and the write switch.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run measured.</returns>
        internal static CageOutcome Run(
            Document document,
            IReadOnlyList<Curve> lines,
            FamilyInstance cage,
            CageMetresSettings settings,
            CageMetresText text,
            Log log)
        {
            var warnings = new List<string>();

            double lineFeet = 0;

            foreach (Curve line in lines)
            {
                lineFeet += line.Length;
            }

            double lineMetres = Length.FromFeet(lineFeet).Meters;

            int longitudinalBars = ReadCount(cage, settings.LongitudinalCountParameter);
            double inclinedStep = ReadMetres(cage, settings.InclinedStepParameter);
            double transverseStep = ReadMetres(cage, settings.TransverseStepParameter);

            if (longitudinalBars < 0)
            {
                warnings.Add(text.MissingCount(settings.LongitudinalCountParameter));
            }

            if (inclinedStep <= 0)
            {
                warnings.Add(text.MissingStep(settings.InclinedStepParameter));
            }

            if (transverseStep <= 0)
            {
                warnings.Add(text.MissingStep(settings.TransverseStepParameter));
            }

            var inclinedLengths = new List<double>();
            var transverseLengths = new List<double>();
            int unclassified = 0;
            int subComponents = 0;

            foreach (ElementId id in cage.GetSubComponentIds())
            {
                if (!(document.GetElement(id) is FamilyInstance bar))
                {
                    continue;
                }

                subComponents++;

                if (HasFlag(bar, settings.LongitudinalFlagParameter))
                {
                    continue;
                }

                if (HasFlag(bar, settings.InclinedFlagParameter))
                {
                    AddIfUsable(inclinedLengths, ReadMetres(bar, settings.BarLengthParameter));
                    continue;
                }

                if (HasFlag(bar, settings.TransverseFlagParameter))
                {
                    AddIfUsable(transverseLengths, ReadMetres(bar, settings.BarLengthParameter));
                    continue;
                }

                unclassified++;
            }

            if (subComponents == 0)
            {
                warnings.Add(text.NoSubComponents);
            }

            if (unclassified > 0)
            {
                warnings.Add(text.Unclassified(unclassified));
            }

            double inclinedBarMetres = inclinedLengths.Count > 0 ? inclinedLengths.Max() : 0;

            if (inclinedBarMetres <= 0)
            {
                warnings.Add(text.MissingInclinedLength);
            }

            IReadOnlyList<double> distinctTransverse = Distinct(transverseLengths);

            if (distinctTransverse.Count == 0)
            {
                warnings.Add(text.MissingTransverseLength);
            }

            int inclinedPositions = CageRules.Positions(lineMetres, inclinedStep);
            int transversePositions = CageRules.Positions(lineMetres, transverseStep);

            double longitudinalMetres = CageRules.Round(
                CageRules.LongitudinalMetres(lineMetres, longitudinalBars));
            double inclinedMetres = CageRules.Round(
                CageRules.InclinedMetres(inclinedPositions, inclinedBarMetres));
            double transverseMetres = CageRules.Round(
                CageRules.TransverseMetres(transversePositions, distinctTransverse));
            double totalMetres = CageRules.Round(longitudinalMetres + inclinedMetres + transverseMetres);

            string? writeNote = Write(document, cage, settings, text, totalMetres, log, warnings);

            log.Info(
                "CageMetres: " + text.LinesLine(lines.Count, lineMetres) + ", " +
                longitudinalBars + " longitudinal, " + inclinedPositions + " inclined positions, " +
                transversePositions + " transverse positions, total " +
                totalMetres.ToString("0.00", CultureInfo.InvariantCulture) + " m.",
                nameof(CageMetresEngine));

            return new CageOutcome(
                cage.Symbol?.Family?.Name ?? cage.Name ?? string.Empty,
                lines.Count,
                lineMetres,
                Math.Max(longitudinalBars, 0),
                inclinedBarMetres,
                inclinedPositions,
                transversePositions,
                distinctTransverse,
                longitudinalMetres,
                inclinedMetres,
                transverseMetres,
                totalMetres,
                writeNote,
                warnings);
        }

        private static string? Write(
            Document document,
            FamilyInstance cage,
            CageMetresSettings settings,
            CageMetresText text,
            double totalMetres,
            Log log,
            List<string> warnings)
        {
            if (!settings.WriteResult)
            {
                return text.WriteOff;
            }

            Parameter? target = cage.LookupParameter(settings.ResultParameter)
                ?? cage.Symbol?.LookupParameter(settings.ResultParameter);

            if (target == null)
            {
                return text.NoParameter(settings.ResultParameter);
            }

            if (target.IsReadOnly)
            {
                return text.ReadOnly(settings.ResultParameter);
            }

            if (target.StorageType != StorageType.Double && target.StorageType != StorageType.String)
            {
                return text.NotANumber(settings.ResultParameter);
            }

            try
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    if (target.StorageType == StorageType.Double)
                    {
                        target.Set(totalMetres);
                    }
                    else
                    {
                        target.Set(CageMetresText.Metres(totalMetres));
                    }

                    return true;
                });

                log.Info(
                    "CageMetres: wrote " + CageMetresText.Metres(totalMetres) + " into «" +
                    settings.ResultParameter + "».",
                    nameof(CageMetresEngine));

                return text.Written(settings.ResultParameter);
            }
            catch (Exception exception)
            {
                log.Warn(
                    "CageMetres: the write was refused: " + exception.Message,
                    nameof(CageMetresEngine),
                    exception);

                warnings.Add(text.WriteFailed(exception.Message));

                return null;
            }
        }

        private static IReadOnlyList<double> Distinct(IEnumerable<double> lengths)
        {
            var seen = new List<double>();

            foreach (double length in lengths)
            {
                // Bars that differ by less than a tenth of a millimetre are one length.
                double key = Math.Round(length, 4, MidpointRounding.AwayFromZero);

                if (key > 0 && !seen.Contains(key))
                {
                    seen.Add(key);
                }
            }

            seen.Sort();

            return seen;
        }

        private static void AddIfUsable(List<double> lengths, double value)
        {
            if (value > 0)
            {
                lengths.Add(value);
            }
        }

        private static int ReadCount(Element element, string parameterName)
        {
            Parameter? parameter = element.LookupParameter(parameterName);

            if (parameter == null)
            {
                return -1;
            }

            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    return parameter.AsInteger();

                case StorageType.Double:
                    return (int)Math.Round(parameter.AsDouble());

                default:
                    return -1;
            }
        }

        private static double ReadMetres(Element element, string parameterName)
        {
            Parameter? parameter = element.LookupParameter(parameterName)
                ?? (element as FamilyInstance)?.Symbol?.LookupParameter(parameterName);

            if (parameter == null)
            {
                return 0;
            }

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return Length.FromFeet(parameter.AsDouble()).Meters;

                case StorageType.Integer:
                    // An integer parameter carries no unit; a length stored that way is read
                    // as millimetres, which is how a design writes such a parameter.
                    return Length.FromMillimeters(parameter.AsInteger()).Meters;

                default:
                    return 0;
            }
        }

        private static bool HasFlag(Element element, string parameterName)
        {
            Parameter? parameter = element.LookupParameter(parameterName)
                ?? (element as FamilyInstance)?.Symbol?.LookupParameter(parameterName);

            if (parameter == null)
            {
                return false;
            }

            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    return parameter.AsInteger() != 0;

                case StorageType.Double:
                    return parameter.AsDouble() > 0.5;

                default:
                    return false;
            }
        }
    }
}
