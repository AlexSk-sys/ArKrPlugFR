using System;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.SheetTransfer;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// One sheet of this project that the user can tick.
    /// <para>
    /// Every row can be ticked: the list already left out the sheets that are not
    /// transferable, so the window never has to refuse a tick. The count field next
    /// to the tick says how many copies of that sheet every target receives.
    /// </para>
    /// </summary>
    public sealed class SheetRowViewModel : ObservableObject
    {
        /// <summary>Copies a row asks for when the user does not say otherwise.</summary>
        public const int DefaultCopies = 1;

        /// <summary>Most copies one run makes of a single sheet.</summary>
        public const int MaximumCopies = 99;

        private readonly SheetCandidate _sheet;
        private bool _isSelected;
        private string _copyCountText = Format(DefaultCopies);

        /// <summary>Creates a row.</summary>
        /// <param name="sheet">The sheet to show.</param>
        public SheetRowViewModel(SheetCandidate sheet)
        {
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        }

        /// <summary>Gets the sheet the row shows.</summary>
        public SheetCandidate Item => _sheet;

        /// <summary>Gets the text of the row: the sheet number and name.</summary>
        public string Display => _sheet.Number + " - " + _sheet.Name;

        /// <summary>Gets or sets a value indicating whether the sheet is ticked for copying.</summary>
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

    /// <summary>
    /// One open project that can receive the sheets of this project.
    /// <para>
    /// A project that cannot take sheets — a family, a linked model, a read-only
    /// file — is still listed, with the reason beside it, so the user is told why it
    /// is out rather than left wondering where the project went.
    /// </para>
    /// </summary>
    public sealed class TargetDocumentViewModel : ObservableObject
    {
        private bool _isSelected;

        /// <summary>Creates a target row.</summary>
        /// <param name="document">The open project the row stands for.</param>
        /// <param name="description">How the project is named in the list.</param>
        /// <param name="unavailableReason">Why it cannot take sheets, or <c>null</c> when it can.</param>
        public TargetDocumentViewModel(Document document, string description, string? unavailableReason)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Description = description ?? string.Empty;
            UnavailableReason = unavailableReason;
        }

        /// <summary>Gets the project the row stands for.</summary>
        public Document Document { get; }

        /// <summary>Gets the name of the project: its title, its file, or a note that it is unsaved.</summary>
        public string Description { get; }

        /// <summary>Gets why the project cannot take sheets, or <c>null</c> when it can.</summary>
        public string? UnavailableReason { get; }

        /// <summary>Gets a value indicating whether the project can take sheets.</summary>
        public bool CanReceive => UnavailableReason == null;

        /// <summary>Gets the text of the row: the project and, when it is out, the reason.</summary>
        public string Display => CanReceive
            ? Description
            : Description + " — " + UnavailableReason;

        /// <summary>Gets a value indicating whether sheets are copied into this project.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
