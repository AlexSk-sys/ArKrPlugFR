using System;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Sheets;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// One line of the sheet list: the sheet, whether it is ticked, and how many
    /// copies it needs.
    /// <para>
    /// The count field accepts digits only, keeps a minimum of one copy and a
    /// cap of ninety-nine; anything the user types outside that range is
    /// brought back into range instead of being refused halfway through.
    /// </para>
    /// </summary>
    public sealed class SheetRowViewModel : ObservableObject
    {
        /// <summary>How many copies a row starts with.</summary>
        public const int DefaultCopies = 1;

        /// <summary>The cap of copies one sheet may ask for.</summary>
        public const int MaximumCopies = 99;

        private readonly SheetFacts _sheet;
        private bool _isSelected;
        private string _copyCountText = DefaultCopies.ToString(CultureInfo.InvariantCulture);

        /// <summary>Creates a row for one sheet.</summary>
        /// <param name="sheet">The sheet to show.</param>
        public SheetRowViewModel(SheetFacts sheet)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        }

        /// <summary>Gets the sheet the row shows.</summary>
        public SheetFacts Sheet => _sheet;

        /// <summary>Gets the element id of the sheet.</summary>
        public long SheetId => _sheet.Id;

        /// <summary>Gets the number of the sheet.</summary>
        public string Number => _sheet.Number;

        /// <summary>Gets the name of the sheet.</summary>
        public string Name => _sheet.Name;

        /// <summary>Gets or sets a value indicating whether the sheet is ticked for duplication.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>Gets the number of copies the row asks for, never below one.</summary>
        public int CopyCount
        {
            get
            {
                if (int.TryParse(_copyCountText, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                    && value >= DefaultCopies)
                {
                    return value > MaximumCopies ? MaximumCopies : value;
                }

                return DefaultCopies;
            }
        }

        /// <summary>Gets or sets the text of the count field, normalised as the user types.</summary>
        public string CopyCountText
        {
            get => _copyCountText;

            set
            {
                string normalized = Normalize(value);

                if (string.Equals(_copyCountText, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _copyCountText = normalized;
                RaisePropertyChanged(nameof(CopyCountText));
                RaisePropertyChanged(nameof(CopyCount));
            }
        }

        /// <summary>Adds one copy, up to the cap.</summary>
        public void Increase()
        {
            int count = CopyCount;
            CopyCountText = Format(count >= MaximumCopies ? MaximumCopies : count + 1);
        }

        /// <summary>Removes one copy, down to one copy.</summary>
        public void Decrease()
        {
            int count = CopyCount;
            CopyCountText = Format(count <= DefaultCopies ? DefaultCopies : count - 1);
        }

        private static string Format(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var digits = new StringBuilder(value!.Length);

            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    digits.Append(character);
                }
            }

            string trimmed = digits.ToString().TrimStart('0');

            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            if (!int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed))
            {
                // A number too large to hold is still too large for the run.
                return Format(MaximumCopies);
            }

            return parsed > MaximumCopies ? Format(MaximumCopies) : trimmed;
        }
    }
}
