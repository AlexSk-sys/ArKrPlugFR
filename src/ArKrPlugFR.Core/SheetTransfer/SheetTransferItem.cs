namespace ArKrPlugFR.Core.SheetTransfer
{
    /// <summary>
    /// One planned copy of a source sheet: the number the copy receives and the
    /// marked name it carries.
    /// </summary>
    public sealed class SheetTransferItem
    {
        /// <summary>Creates a planned copy.</summary>
        /// <param name="sourceSheetId">Element id of the sheet in the source project.</param>
        /// <param name="sourceNumber">Sheet number in the source project.</param>
        /// <param name="sourceName">Sheet name in the source project.</param>
        /// <param name="number">Number the copy receives in the target project.</param>
        /// <param name="name">Name the copy receives in the target project.</param>
        /// <param name="viewCount">Number of transferable views placed on the sheet.</param>
        public SheetTransferItem(
            long sourceSheetId,
            string sourceNumber,
            string sourceName,
            string number,
            string name,
            int viewCount)
        {
            SourceSheetId = sourceSheetId;
            SourceNumber = sourceNumber;
            SourceName = sourceName;
            Number = number;
            Name = name;
            ViewCount = viewCount;
        }

        /// <summary>Gets the element id of the sheet in the source project.</summary>
        public long SourceSheetId { get; }

        /// <summary>Gets the sheet number in the source project.</summary>
        public string SourceNumber { get; }

        /// <summary>Gets the sheet name in the source project.</summary>
        public string SourceName { get; }

        /// <summary>Gets the number the copy receives in the target project.</summary>
        public string Number { get; }

        /// <summary>Gets the name the copy receives in the target project.</summary>
        public string Name { get; }

        /// <summary>Gets the number of transferable views placed on the sheet.</summary>
        public int ViewCount { get; }
    }
}
