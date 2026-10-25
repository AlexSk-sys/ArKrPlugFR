namespace ArKrPlugFR.Core.SheetTransfer
{
    /// <summary>
    /// A view placed on a source sheet, described with plain values.
    /// </summary>
    public sealed class ViewCandidate
    {
        /// <summary>Creates a view candidate.</summary>
        /// <param name="id">Element id of the view in the source project.</param>
        /// <param name="name">View name in the source project.</param>
        /// <param name="isTemplate">Whether the view is a template.</param>
        public ViewCandidate(long id, string name, bool isTemplate)
        {
            Id = id;
            Name = name ?? string.Empty;
            IsTemplate = isTemplate;
        }

        /// <summary>Gets the element id of the view in the source project.</summary>
        public long Id { get; }

        /// <summary>Gets the view name in the source project.</summary>
        public string Name { get; }

        /// <summary>Gets a value indicating whether the view is a template.</summary>
        public bool IsTemplate { get; }
    }
}
