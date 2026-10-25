using System;
using ArKrPlugFR.Core.Duplication;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// A row the user can tick.
    /// <para>
    /// An item that is still used in the project cannot be ticked at all. Refusing
    /// the tick is better than accepting it and then skipping the item during the
    /// delete, because the user sees why nothing happened.
    /// </para>
    /// </summary>
    public abstract class CheckableRow : ObservableObject
    {
        private bool _isSelected;

        /// <summary>Creates a row.</summary>
        /// <param name="isRemovable">Whether the item may be deleted.</param>
        /// <param name="zoomTypeId">Type to show in the model on a double-click, or <c>null</c>.</param>
        /// <param name="text">Text resolved for the current language.</param>
        protected CheckableRow(bool isRemovable, long? zoomTypeId, DuplicateFinderText text)
        {
            IsRemovable = isRemovable;
            ZoomTypeId = zoomTypeId;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public DuplicateFinderText Text { get; }

        /// <summary>Gets a value indicating whether the item may be deleted.</summary>
        public bool IsRemovable { get; }

        /// <summary>Gets the type to show in the model on a double-click.</summary>
        public long? ZoomTypeId { get; }

        /// <summary>Gets or sets a value indicating whether the item is ticked for deletion.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value && !IsRemovable)
                {
                    return;
                }

                SetProperty(ref _isSelected, value);
            }
        }

        /// <summary>Gets the element id to delete.</summary>
        public abstract long ItemId { get; }

        /// <summary>Gets the human-readable status of the item.</summary>
        public string Status => IsRemovable ? Text.StatusRemovable : Text.StatusUsed;

        /// <summary>Gets one line describing the item, used when the list is copied.</summary>
        public abstract string CopyLine { get; }
    }

    /// <summary>A family type that repeats another type of the same family.</summary>
    public sealed class TypeRowViewModel : CheckableRow
    {
        private readonly DuplicateTypeRow _row;

        /// <summary>Creates a row.</summary>
        /// <param name="row">The finding to show.</param>
        /// <param name="text">Text resolved for the current language.</param>
        public TypeRowViewModel(DuplicateTypeRow row, DuplicateFinderText text)
            : base(
                (row ?? throw new ArgumentNullException(nameof(row))).IsRemovable,
                row.Duplicate.InstanceCount > 0 ? row.Duplicate.Id : (long?)null,
                text)
        {
            _row = row;
        }

        /// <summary>Gets the name of the family.</summary>
        public string Family => _row.FamilyName;

        /// <summary>Gets the name of the category.</summary>
        public string Category => _row.CategoryName;

        /// <summary>Gets the name of the type to remove.</summary>
        public string TypeName => _row.Duplicate.TypeName;

        /// <summary>Gets the name of the type to keep.</summary>
        public string KeptTypeName => _row.Kept.TypeName;

        /// <summary>Gets the number of placed instances of the type to remove.</summary>
        public string Instances => DuplicateFinderText.Number(_row.Duplicate.InstanceCount);

        /// <inheritdoc />
        public override long ItemId => _row.Duplicate.Id;

        /// <inheritdoc />
        public override string CopyLine =>
            string.Join("\t", Family, Category, TypeName, KeptTypeName, Instances, Status);
    }

    /// <summary>A family that repeats another family.</summary>
    public sealed class FamilyRowViewModel : CheckableRow
    {
        private readonly DuplicateFamilyRow _row;

        /// <summary>Creates a row.</summary>
        /// <param name="row">The finding to show.</param>
        /// <param name="text">Text resolved for the current language.</param>
        public FamilyRowViewModel(DuplicateFamilyRow row, DuplicateFinderText text)
            : base(
                (row ?? throw new ArgumentNullException(nameof(row))).IsRemovable,
                FindInstanceTypeId(row.Duplicate),
                text)
        {
            _row = row;
        }

        /// <summary>Gets the name of the family to remove.</summary>
        public string Family => _row.Duplicate.Name;

        /// <summary>Gets the name of the category.</summary>
        public string Category => _row.Duplicate.CategoryName;

        /// <summary>Gets the name of the family to keep.</summary>
        public string KeptFamily => _row.Kept.Name;

        /// <summary>Gets how many types of the kept family the duplicate repeats.</summary>
        public string MatchedTypes =>
            DuplicateFinderText.Number(_row.MatchedTypeCount) + " / " + DuplicateFinderText.Number(_row.KeptTypeCount);

        /// <summary>Gets the number of placed instances of the family to remove.</summary>
        public string Instances => DuplicateFinderText.Number(_row.InstanceCount);

        /// <inheritdoc />
        public override long ItemId => _row.Duplicate.Id;

        /// <inheritdoc />
        public override string CopyLine =>
            string.Join("\t", Family, Category, KeptFamily, MatchedTypes, Instances, Status);

        private static long? FindInstanceTypeId(FamilyRecord family)
        {
            foreach (TypeRecord type in family.Types)
            {
                if (type.InstanceCount > 0)
                {
                    return type.Id;
                }
            }

            return null;
        }
    }

    /// <summary>Two types of one family that differ in only a few parameters.</summary>
    public sealed class NearRowViewModel : ObservableObject
    {
        private readonly NearDuplicatePair _pair;

        /// <summary>Creates a row.</summary>
        /// <param name="pair">The finding to show.</param>
        public NearRowViewModel(NearDuplicatePair pair)
        {
            _pair = pair ?? throw new ArgumentNullException(nameof(pair));
        }

        /// <summary>Gets the name of the family.</summary>
        public string Family => _pair.FamilyName;

        /// <summary>Gets the name of the first type.</summary>
        public string FirstType => _pair.FirstTypeName;

        /// <summary>Gets the name of the second type.</summary>
        public string SecondType => _pair.SecondTypeName;

        /// <summary>Gets how many parameters the two types disagree on.</summary>
        public string Difference => DuplicateFinderText.Number(_pair.DifferenceCount);
    }
}
