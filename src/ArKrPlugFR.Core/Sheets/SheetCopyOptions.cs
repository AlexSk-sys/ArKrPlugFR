namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// The switches that say which parts of a sheet travel with its copies.
    /// <para>
    /// Every switch is remembered between sessions by the tool window; the
    /// defaults here are the values a first run starts with. The object is
    /// built once when a run starts, so the run itself reads a fixed set of
    /// decisions.
    /// </para>
    /// </summary>
    public sealed record SheetCopyOptions
    {
        /// <summary>Creates the switches with their first-run defaults.</summary>
        /// <param name="copyViews">Duplicate the placed views and place the copies on the new sheet.</param>
        /// <param name="copyLegends">Place the source sheet's legend views on the new sheet.</param>
        /// <param name="copyText">Copy the text notes owned by the source sheet.</param>
        /// <param name="copyImages">Copy the raster images owned by the source sheet.</param>
        /// <param name="copySchedules">Place the schedule views of the source sheet on the copy again.</param>
        /// <param name="copyAnnotations">Copy the annotations, lines and detailing owned by the source sheet, and the guide grid.</param>
        /// <param name="copyParameters">Copy the sheet parameters and the stamp parameters.</param>
        public SheetCopyOptions(
            bool copyViews = true,
            bool copyLegends = true,
            bool copyText = true,
            bool copyImages = true,
            bool copySchedules = true,
            bool copyAnnotations = true,
            bool copyParameters = false)
        {
            CopyViews = copyViews;
            CopyLegends = copyLegends;
            CopyText = copyText;
            CopyImages = copyImages;
            CopySchedules = copySchedules;
            CopyAnnotations = copyAnnotations;
            CopyParameters = copyParameters;
        }

        /// <summary>Gets a value indicating whether placed views are duplicated.</summary>
        public bool CopyViews { get; }

        /// <summary>Gets a value indicating whether legend views are placed again.</summary>
        public bool CopyLegends { get; }

        /// <summary>Gets a value indicating whether text notes are copied.</summary>
        public bool CopyText { get; }

        /// <summary>Gets a value indicating whether raster images are copied.</summary>
        public bool CopyImages { get; }

        /// <summary>Gets a value indicating whether schedule views are placed on the copy again.</summary>
        public bool CopySchedules { get; }

        /// <summary>Gets a value indicating whether annotations, lines and the guide grid are carried over.</summary>
        public bool CopyAnnotations { get; }

        /// <summary>Gets a value indicating whether sheet and stamp parameters are copied.</summary>
        public bool CopyParameters { get; }
    }
}
