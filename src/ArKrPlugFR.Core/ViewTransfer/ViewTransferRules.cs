using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// Decides which views of the source project may be transferred.
    /// <para>
    /// The rule is exclusion-based: view templates, ordinary views and ordinary
    /// schedules may move; sheets, panel schedules, system schedules and views
    /// whose type exists only inside Revit may not.
    /// </para>
    /// </summary>
    public static class ViewTransferRules
    {
        /// <summary>
        /// View-type names that are never transferred, whatever else is known
        /// about the element. Compared case-insensitively.
        /// </summary>
        private static readonly HashSet<string> NeverTransferableTypeNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DrawingSheet",
                "PanelSchedule",
                "ColumnSchedule",
                "Internal",
                "Undefined",
                "FirstRev",
            };

        /// <summary>Determines whether the view may be copied into another project.</summary>
        /// <param name="candidate">The view to judge.</param>
        /// <returns><c>true</c> when the view may be transferred.</returns>
        public static bool IsTransferable(ViewCandidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            if (!candidate.IsValid)
            {
                return false;
            }

            if (candidate.IsSheet || candidate.IsSystemSchedule)
            {
                return false;
            }

            return !NeverTransferableTypeNames.Contains(candidate.ViewTypeName);
        }
    }
}
