using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// The settings of a duplicate scan.
    /// <para>
    /// Which parameters identify a type rather than describe it differs between
    /// companies, so the list of ignored parameters is configuration, not a
    /// constant buried in the comparison.
    /// </para>
    /// </summary>
    public sealed class DuplicateScanOptions
    {
        /// <summary>
        /// Parameters that never take part in the comparison: the type identity and
        /// the free-text annotations, which are usually empty on the copy.
        /// </summary>
        public static readonly IReadOnlyList<string> DefaultExcludedParameters = new[]
        {
            "Type Name",
            "Type",
            "ID",
            "Type Mark",
            "Mark",
            "Comments",
            "Description",
            "Type Image",
            "Cost",
        };

        private readonly HashSet<string> _excluded;

        /// <summary>Creates the options.</summary>
        /// <param name="excludedParameters">Parameter names ignored by the comparison; defaults to <see cref="DefaultExcludedParameters"/>.</param>
        /// <param name="significantDigits">Significant digits used to compare numbers.</param>
        /// <param name="nearDuplicateMinimumDifferences">Smallest number of differing parameters worth reporting as a near duplicate.</param>
        /// <param name="nearDuplicateMaximumDifferences">Largest number of differing parameters worth reporting as a near duplicate.</param>
        /// <param name="nearDuplicateReportLimit">Maximum number of near-duplicate pairs reported.</param>
        /// <param name="nearDuplicatePairLimitPerFamily">Maximum number of type pairs examined per family.</param>
        public DuplicateScanOptions(
            IEnumerable<string>? excludedParameters = null,
            int significantDigits = ParameterValueNormalizer.DefaultSignificantDigits,
            int nearDuplicateMinimumDifferences = 1,
            int nearDuplicateMaximumDifferences = 3,
            int nearDuplicateReportLimit = 20,
            int nearDuplicatePairLimitPerFamily = 400)
        {
            if (significantDigits < 1 || significantDigits > 17)
            {
                throw new ArgumentOutOfRangeException(nameof(significantDigits), significantDigits, "Must be between 1 and 17.");
            }

            if (nearDuplicateMinimumDifferences < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nearDuplicateMinimumDifferences),
                    nearDuplicateMinimumDifferences,
                    "Must be at least 1.");
            }

            if (nearDuplicateMaximumDifferences < nearDuplicateMinimumDifferences)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nearDuplicateMaximumDifferences),
                    nearDuplicateMaximumDifferences,
                    "Must not be smaller than the minimum.");
            }

            if (nearDuplicateReportLimit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(nearDuplicateReportLimit), nearDuplicateReportLimit, "Must be at least 1.");
            }

            if (nearDuplicatePairLimitPerFamily < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nearDuplicatePairLimitPerFamily),
                    nearDuplicatePairLimitPerFamily,
                    "Must be at least 1.");
            }

            _excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in excludedParameters ?? DefaultExcludedParameters)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _excluded.Add(name.Trim());
                }
            }

            SignificantDigits = significantDigits;
            NearDuplicateMinimumDifferences = nearDuplicateMinimumDifferences;
            NearDuplicateMaximumDifferences = nearDuplicateMaximumDifferences;
            NearDuplicateReportLimit = nearDuplicateReportLimit;
            NearDuplicatePairLimitPerFamily = nearDuplicatePairLimitPerFamily;
        }

        /// <summary>Gets the parameter names ignored by the comparison.</summary>
        public IReadOnlyCollection<string> ExcludedParameters => _excluded;

        /// <summary>Gets the significant digits used to compare numbers.</summary>
        public int SignificantDigits { get; }

        /// <summary>Gets the smallest number of differing parameters reported as a near duplicate.</summary>
        public int NearDuplicateMinimumDifferences { get; }

        /// <summary>Gets the largest number of differing parameters reported as a near duplicate.</summary>
        public int NearDuplicateMaximumDifferences { get; }

        /// <summary>Gets the maximum number of near-duplicate pairs reported.</summary>
        public int NearDuplicateReportLimit { get; }

        /// <summary>Gets the maximum number of type pairs examined per family.</summary>
        public int NearDuplicatePairLimitPerFamily { get; }

        /// <summary>Determines whether a parameter is ignored by the comparison.</summary>
        public bool IsExcluded(string parameterName)
        {
            return !string.IsNullOrWhiteSpace(parameterName) && _excluded.Contains(parameterName.Trim());
        }
    }
}
