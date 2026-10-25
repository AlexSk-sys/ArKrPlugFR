using System;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// One view of a source project, read into plain data.
    /// <para>
    /// The Revit adapter fills this record; the rules that decide whether the
    /// view may be transferred never touch the Revit API, so they can be tested
    /// without a running Revit.
    /// </para>
    /// </summary>
    public sealed class ViewCandidate
    {
        /// <summary>Creates a view candidate.</summary>
        /// <param name="id">Element id of the view in the source project.</param>
        /// <param name="name">Name of the view.</param>
        /// <param name="viewTypeName">Name of the view's type, for example <c>FloorPlan</c>.</param>
        /// <param name="isTemplate">Whether the view is a view template.</param>
        /// <param name="isValid">Whether the element was still valid when it was read.</param>
        /// <param name="isSheet">Whether the view is a sheet.</param>
        /// <param name="isSystemSchedule">
        /// Whether the view is a system-managed schedule, such as the revision
        /// schedule of a title block.
        /// </param>
        public ViewCandidate(
            long id,
            string name,
            string viewTypeName,
            bool isTemplate,
            bool isValid,
            bool isSheet,
            bool isSystemSchedule)
        {
            if (string.IsNullOrWhiteSpace(viewTypeName))
            {
                throw new ArgumentException("The view type name is required.", nameof(viewTypeName));
            }

            Id = id;
            Name = name ?? string.Empty;
            ViewTypeName = viewTypeName.Trim();
            IsTemplate = isTemplate;
            IsValid = isValid;
            IsSheet = isSheet;
            IsSystemSchedule = isSystemSchedule;
        }

        /// <summary>Gets the element id of the view in the source project.</summary>
        public long Id { get; }

        /// <summary>Gets the name of the view.</summary>
        public string Name { get; }

        /// <summary>Gets the name of the view's type, for example <c>FloorPlan</c>.</summary>
        public string ViewTypeName { get; }

        /// <summary>Gets a value indicating whether the view is a view template.</summary>
        public bool IsTemplate { get; }

        /// <summary>Gets a value indicating whether the element was still valid when it was read.</summary>
        public bool IsValid { get; }

        /// <summary>Gets a value indicating whether the view is a sheet.</summary>
        public bool IsSheet { get; }

        /// <summary>Gets a value indicating whether the view is a system-managed schedule.</summary>
        public bool IsSystemSchedule { get; }
    }
}
