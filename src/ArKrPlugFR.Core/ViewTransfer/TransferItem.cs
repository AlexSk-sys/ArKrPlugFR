using System;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// One view the transfer plans to copy.
    /// </summary>
    public sealed class TransferItem
    {
        /// <summary>Creates a planned item.</summary>
        /// <param name="id">Element id of the view in the source project.</param>
        /// <param name="name">Name of the view in the source project.</param>
        /// <param name="plannedName">Name the copy is expected to receive in the target.</param>
        /// <param name="viewTypeName">Name of the view's type.</param>
        /// <param name="isTemplate">Whether the view is a view template.</param>
        public TransferItem(long id, string name, string plannedName, string viewTypeName, bool isTemplate)
        {
            if (string.IsNullOrWhiteSpace(plannedName))
            {
                throw new ArgumentException("The planned name is required.", nameof(plannedName));
            }

            if (string.IsNullOrWhiteSpace(viewTypeName))
            {
                throw new ArgumentException("The view type name is required.", nameof(viewTypeName));
            }

            Id = id;
            Name = name ?? string.Empty;
            PlannedName = plannedName;
            ViewTypeName = viewTypeName;
            IsTemplate = isTemplate;
        }

        /// <summary>Gets the element id of the view in the source project.</summary>
        public long Id { get; }

        /// <summary>Gets the name of the view in the source project.</summary>
        public string Name { get; }

        /// <summary>Gets the name the copy is expected to receive in the target.</summary>
        public string PlannedName { get; }

        /// <summary>Gets the name of the view's type.</summary>
        public string ViewTypeName { get; }

        /// <summary>Gets a value indicating whether the view is a view template.</summary>
        public bool IsTemplate { get; }
    }
}
