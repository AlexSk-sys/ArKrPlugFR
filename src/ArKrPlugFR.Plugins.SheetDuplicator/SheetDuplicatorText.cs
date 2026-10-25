using System.Globalization;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window and the rows take this object instead of a culture code, so a
    /// label is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class SheetDuplicatorText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SheetDuplicatorText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SheetDuplicatorStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the English line under the caption.</summary>
        public string Subtitle => SheetDuplicatorStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the sheet list.</summary>
        public string SheetsCaption => SheetDuplicatorStrings.SheetsCaption.Resolve(_culture);

        /// <summary>Gets the placeholder of the search field.</summary>
        public string SearchPlaceholder => SheetDuplicatorStrings.SearchPlaceholder.Resolve(_culture);

        /// <summary>Gets the caption of the settings card.</summary>
        public string SettingsCaption => SheetDuplicatorStrings.SettingsCaption.Resolve(_culture);

        /// <summary>Gets the caption above the content switches.</summary>
        public string OptionGroupCaption => SheetDuplicatorStrings.OptionGroupCaption.Resolve(_culture);

        /// <summary>Gets the label of the views switch.</summary>
        public string OptionViews => SheetDuplicatorStrings.OptionViews.Resolve(_culture);

        /// <summary>Gets the label of the legends switch.</summary>
        public string OptionLegends => SheetDuplicatorStrings.OptionLegends.Resolve(_culture);

        /// <summary>Gets the label of the text-notes switch.</summary>
        public string OptionText => SheetDuplicatorStrings.OptionText.Resolve(_culture);

        /// <summary>Gets the label of the raster-images switch.</summary>
        public string OptionImages => SheetDuplicatorStrings.OptionImages.Resolve(_culture);

        /// <summary>Gets the label of the annotations switch.</summary>
        public string OptionSchedules => SheetDuplicatorStrings.OptionSchedules.Resolve(_culture);

        public string OptionAnnotations => SheetDuplicatorStrings.OptionAnnotations.Resolve(_culture);

        /// <summary>Gets the label of the parameters switch.</summary>
        public string OptionParameters => SheetDuplicatorStrings.OptionParameters.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SheetDuplicatorStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the result box.</summary>
        public string ResultCaption => SheetDuplicatorStrings.ResultCaption.Resolve(_culture);

        /// <summary>Gets the message shown when nothing is ticked.</summary>
        public string NothingSelected => SheetDuplicatorStrings.NothingSelected.Resolve(_culture);

        /// <summary>Gets the message shown after a run without failures.</summary>
        public string Success => SheetDuplicatorStrings.Success.Resolve(_culture);

        /// <summary>Gets the message that reports a failing sheet.</summary>
        /// <param name="sheetNumber">Number of the sheet that failed.</param>
        /// <param name="reason">Why the sheet failed.</param>
        public string FailedSheet(string sheetNumber, string reason) =>
            SheetDuplicatorStrings.FailedSheet.Resolve(_culture, sheetNumber, reason);

        /// <summary>Gets the line that reports a problem on one sheet of a run that continued.</summary>
        /// <param name="sheetNumber">Number of the sheet the problem belongs to.</param>
        /// <param name="reason">What went wrong.</param>
        public string SheetIssue(string sheetNumber, string reason) =>
            SheetDuplicatorStrings.SheetIssue.Resolve(_culture, sheetNumber, reason);

        /// <summary>Gets the message that reports a failure of the whole run.</summary>
        /// <param name="reason">Why the run failed.</param>
        public string GeneralFailure(string reason) =>
            SheetDuplicatorStrings.GeneralFailure.Resolve(_culture, reason);

        /// <summary>Gets the message for a view that refuses to sit on a sheet.</summary>
        /// <param name="viewName">Name of the view.</param>
        public string UnplaceableView(string viewName) =>
            SheetDuplicatorStrings.UnplaceableView.Resolve(_culture, viewName);

        /// <summary>Gets the message for a drafting view that refuses to sit on a sheet.</summary>
        /// <param name="viewName">Name of the view.</param>
        public string DraftingUnplaceableView(string viewName) =>
            SheetDuplicatorStrings.DraftingUnplaceableView.Resolve(_culture, viewName);

        /// <summary>Gets the message for a view that refuses to be duplicated.</summary>
        /// <param name="viewName">Name of the view.</param>
        public string CannotDuplicateView(string viewName) =>
            SheetDuplicatorStrings.CannotDuplicateView.Resolve(_culture, viewName);

        /// <summary>Gets the line that says how many report lines were left out.</summary>
        /// <param name="count">Number of left-out lines.</param>
        public string MoreLine(int count) =>
            SheetDuplicatorStrings.MoreLine.Resolve(_culture, count);

        /// <summary>Gets the message shown when no project document is available.</summary>
        public string NoDocument => SheetDuplicatorStrings.NoDocument.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionNameFor(string number, int copies) =>
            SheetDuplicatorStrings.TransactionNameFor.Resolve(_culture, number, copies);


        /// <summary>Gets the reason used when a sheet vanished mid-run.</summary>
        public string ReasonSheetMissing => SheetDuplicatorStrings.ReasonSheetMissing.Resolve(_culture);

        /// <summary>Gets the reason used when no free sheet number could be assigned.</summary>
        public string ReasonNoFreeNumber => SheetDuplicatorStrings.ReasonNoFreeNumber.Resolve(_culture);

        /// <summary>Gets the caption of the run button with the number of ticked sheets.</summary>
        /// <param name="sheetCount">Number of ticked sheets.</param>
        public string DuplicateButtonText(int sheetCount) =>
            SheetDuplicatorStrings.DuplicateButtonText.Resolve(_culture, sheetCount);

        /// <summary>Formats a number the same way in every language.</summary>
        /// <param name="value">Number to format.</param>
        public static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
