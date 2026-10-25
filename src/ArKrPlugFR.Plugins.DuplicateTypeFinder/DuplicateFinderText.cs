using System.Globalization;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// Rows and the window take this object instead of a culture code, so a label
    /// is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class DuplicateFinderText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public DuplicateFinderText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => DuplicateFinderStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the caption of the types tab.</summary>
        public string TypesTab => DuplicateFinderStrings.TypesTab.Resolve(_culture);

        /// <summary>Gets the caption of the families tab.</summary>
        public string FamiliesTab => DuplicateFinderStrings.FamiliesTab.Resolve(_culture);

        /// <summary>Gets the caption of the close-matches tab.</summary>
        public string NearTab => DuplicateFinderStrings.NearTab.Resolve(_culture);

        /// <summary>Gets the header of the family column.</summary>
        public string ColumnFamily => DuplicateFinderStrings.ColumnFamily.Resolve(_culture);

        /// <summary>Gets the header of the category column.</summary>
        public string ColumnCategory => DuplicateFinderStrings.ColumnCategory.Resolve(_culture);

        /// <summary>Gets the header of the type-to-remove column.</summary>
        public string ColumnType => DuplicateFinderStrings.ColumnType.Resolve(_culture);

        /// <summary>Gets the header of the type-to-keep column.</summary>
        public string ColumnKeptType => DuplicateFinderStrings.ColumnKeptType.Resolve(_culture);

        /// <summary>Gets the header of the family-to-keep column.</summary>
        public string ColumnKeptFamily => DuplicateFinderStrings.ColumnKeptFamily.Resolve(_culture);

        /// <summary>Gets the header of the instance-count column.</summary>
        public string ColumnInstances => DuplicateFinderStrings.ColumnInstances.Resolve(_culture);

        /// <summary>Gets the header of the status column.</summary>
        public string ColumnStatus => DuplicateFinderStrings.ColumnStatus.Resolve(_culture);

        /// <summary>Gets the header of the matched-types column.</summary>
        public string ColumnMatchedTypes => DuplicateFinderStrings.ColumnMatchedTypes.Resolve(_culture);

        /// <summary>Gets the header of the difference-count column.</summary>
        public string ColumnDifference => DuplicateFinderStrings.ColumnDifference.Resolve(_culture);

        /// <summary>Gets the header of the first-type column.</summary>
        public string ColumnTypeA => DuplicateFinderStrings.ColumnTypeA.Resolve(_culture);

        /// <summary>Gets the header of the second-type column.</summary>
        public string ColumnTypeB => DuplicateFinderStrings.ColumnTypeB.Resolve(_culture);

        /// <summary>Gets the status of an item that may be deleted.</summary>
        public string StatusRemovable => DuplicateFinderStrings.StatusRemovable.Resolve(_culture);

        /// <summary>Gets the status of an item that is in use and stays.</summary>
        public string StatusUsed => DuplicateFinderStrings.StatusUsed.Resolve(_culture);

        /// <summary>Gets the caption of the select-all button.</summary>
        public string SelectAll => DuplicateFinderStrings.SelectAll.Resolve(_culture);

        /// <summary>Gets the caption of the clear button.</summary>
        public string ClearSelection => DuplicateFinderStrings.ClearSelection.Resolve(_culture);

        /// <summary>Gets the caption of the copy button.</summary>
        public string CopyList => DuplicateFinderStrings.CopyList.Resolve(_culture);

        /// <summary>Gets the caption of the delete button.</summary>
        public string Delete => DuplicateFinderStrings.Delete.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => DuplicateFinderStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the confirmation box.</summary>
        public string ConfirmCaption => DuplicateFinderStrings.ConfirmCaption.Resolve(_culture);

        /// <summary>Gets the hint that tells the user which rows cannot be selected.</summary>
        public string UsedHint => DuplicateFinderStrings.UsedHint.Resolve(_culture);

        /// <summary>Gets the hint about double-clicking a row.</summary>
        public string ZoomHint => DuplicateFinderStrings.ZoomHint.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ResultCaption => DuplicateFinderStrings.ResultCaption.Resolve(_culture);

        /// <summary>Gets the name of the delete step in the undo list.</summary>
        public string TransactionName => DuplicateFinderStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the confirmation question for a number of items.</summary>
        public string ConfirmText(int count) => DuplicateFinderStrings.ConfirmText.Resolve(_culture, count);

        /// <summary>Gets the line that says how many types were compared.</summary>
        public string SummaryChecked(int count) => DuplicateFinderStrings.SummaryChecked.Resolve(_culture, count);

        /// <summary>Gets the summary line for duplicate types.</summary>
        public string SummaryTypes(int found, int deletable) =>
            DuplicateFinderStrings.SummaryTypes.Resolve(_culture, found, deletable);

        /// <summary>Gets the summary line for duplicate families.</summary>
        public string SummaryFamilies(int found, int deletable) =>
            DuplicateFinderStrings.SummaryFamilies.Resolve(_culture, found, deletable);

        /// <summary>Gets the line that introduces the close matches.</summary>
        public string NearIntro => DuplicateFinderStrings.NearIntro.Resolve(_culture);

        /// <summary>Gets the line that reports how many items were deleted.</summary>
        public string DeletedLine(int count) => DuplicateFinderStrings.DeletedLine.Resolve(_culture, count);

        /// <summary>Gets the line that reports how many items could not be deleted.</summary>
        public string NotDeletedLine(int count) => DuplicateFinderStrings.NotDeletedLine.Resolve(_culture, count);

        /// <summary>Gets the line that reports how many items were skipped because they are in use.</summary>
        public string SkippedLine(int count) => DuplicateFinderStrings.SkippedLine.Resolve(_culture, count);

        /// <summary>Gets the line that reports how many items could not be read.</summary>
        public string ReadErrorsLine(int count) => DuplicateFinderStrings.ReadErrorsLine.Resolve(_culture, count);

        /// <summary>Gets the line that says how many more items were left out of the report.</summary>
        public string MoreLine(int count) => DuplicateFinderStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the heading used when the list is copied to the clipboard.</summary>
        public string CopyHeader => DuplicateFinderStrings.CopyHeader.Resolve(_culture);

        /// <summary>Gets the heading of the types section in a copied list.</summary>
        public string CopyTypesHeader => DuplicateFinderStrings.CopyTypesHeader.Resolve(_culture);

        /// <summary>Gets the heading of the families section in a copied list.</summary>
        public string CopyFamiliesHeader => DuplicateFinderStrings.CopyFamiliesHeader.Resolve(_culture);

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
