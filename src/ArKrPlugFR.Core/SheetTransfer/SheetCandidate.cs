using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.SheetTransfer
{
    /// <summary>
    /// A sheet of the source project, described with plain values.
    /// <para>
    /// The transfer rules work on this description instead of a Revit element, so
    /// which sheets may move and how they are numbered can be decided and tested
    /// without a running Revit.
    /// </para>
    /// </summary>
    public sealed class SheetCandidate
    {
        /// <summary>Creates a sheet candidate.</summary>
        /// <param name="id">Element id of the sheet in the source project.</param>
        /// <param name="number">Sheet number in the source project.</param>
        /// <param name="name">Sheet name in the source project.</param>
        /// <param name="isPlaceholder">Whether the sheet is a sheet-list placeholder.</param>
        /// <param name="placedViews">Views placed on the sheet.</param>
        public SheetCandidate(long id, string number, string name, bool isPlaceholder, IReadOnlyList<ViewCandidate> placedViews)
        {
            Id = id;
            Number = number ?? string.Empty;
            Name = name ?? string.Empty;
            IsPlaceholder = isPlaceholder;
            PlacedViews = placedViews ?? Array.Empty<ViewCandidate>();
        }

        /// <summary>Gets the element id of the sheet in the source project.</summary>
        public long Id { get; }

        /// <summary>Gets the sheet number in the source project.</summary>
        public string Number { get; }

        /// <summary>Gets the sheet name in the source project.</summary>
        public string Name { get; }

        /// <summary>Gets a value indicating whether the sheet is a sheet-list placeholder.</summary>
        public bool IsPlaceholder { get; }

        /// <summary>Gets the views placed on the sheet.</summary>
        public IReadOnlyList<ViewCandidate> PlacedViews { get; }
    }
}
