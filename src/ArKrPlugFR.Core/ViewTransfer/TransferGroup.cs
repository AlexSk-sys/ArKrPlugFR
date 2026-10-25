using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// One row of the transfer tree: a group of views that share a display category.
    /// <para>
    /// A group is identified either by a catalog key or — for a view type the
    /// catalog does not know — by the raw view-type name, which then names the
    /// group directly.
    /// </para>
    /// </summary>
    public sealed class TransferGroup
    {
        /// <summary>Creates a group.</summary>
        /// <param name="key">Catalog key of the group, or <c>null</c> for a raw group.</param>
        /// <param name="fallbackViewTypeName">Raw view-type name for a group the catalog does not know.</param>
        /// <param name="items">Views of the group.</param>
        public TransferGroup(string? key, string? fallbackViewTypeName, IReadOnlyList<TransferItem> items)
        {
            if (key == null && string.IsNullOrWhiteSpace(fallbackViewTypeName))
            {
                throw new ArgumentException("A group needs either a key or a raw view-type name.");
            }

            if (key != null && !string.IsNullOrWhiteSpace(fallbackViewTypeName))
            {
                throw new ArgumentException("A group has either a key or a raw view-type name, not both.");
            }

            Key = key;
            FallbackViewTypeName = fallbackViewTypeName?.Trim() ?? string.Empty;
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>Gets the catalog key of the group, or <c>null</c> for a raw group.</summary>
        public string? Key { get; }

        /// <summary>Gets the raw view-type name of a raw group, or an empty string for a catalog group.</summary>
        public string FallbackViewTypeName { get; }

        /// <summary>Gets the views of the group.</summary>
        public IReadOnlyList<TransferItem> Items { get; }

        /// <summary>Gets the number of views in the group.</summary>
        public int ItemCount => Items.Count;

        /// <summary>Gets the name the group is ordered by before it is localized.</summary>
        public string SortKey => Key ?? FallbackViewTypeName;
    }
}
