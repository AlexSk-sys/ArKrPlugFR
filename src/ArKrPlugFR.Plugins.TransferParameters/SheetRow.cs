using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// One line of the sheet list: the sheet and whether it receives the value.
    /// </summary>
    public sealed class SheetRow : ObservableObject
    {
        private bool _isSelected;

        /// <summary>Creates a row for one sheet.</summary>
        /// <param name="sheet">The sheet to show.</param>
        public SheetRow(ViewSheet sheet)
        {
            Sheet = sheet;
        }

        /// <summary>Gets the sheet the row shows.</summary>
        public ViewSheet Sheet { get; }

        /// <summary>Gets the number of the sheet.</summary>
        public string Number => Sheet.SheetNumber;

        /// <summary>Gets the name of the sheet.</summary>
        public string Name => Sheet.Name;

        /// <summary>Gets or sets a value indicating whether the sheet receives the value.</summary>
        public bool IsSelected
        {
            get => _isSelected;

            set => SetProperty(ref _isSelected, value);
        }
    }
}
