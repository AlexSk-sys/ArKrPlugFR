using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// What a duplicate scan found.
    /// </summary>
    public sealed class DuplicateScanResult
    {
        /// <summary>Creates a result.</summary>
        /// <param name="checkedTypeCount">How many types were compared.</param>
        /// <param name="typeDuplicates">Types that repeat another type of the same family.</param>
        /// <param name="familyDuplicates">Families that repeat another family.</param>
        /// <param name="nearDuplicates">Types that differ in only a few parameters.</param>
        public DuplicateScanResult(
            int checkedTypeCount,
            IReadOnlyList<DuplicateTypeRow> typeDuplicates,
            IReadOnlyList<DuplicateFamilyRow> familyDuplicates,
            IReadOnlyList<NearDuplicatePair> nearDuplicates)
        {
            if (checkedTypeCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(checkedTypeCount), checkedTypeCount, "A count cannot be negative.");
            }

            CheckedTypeCount = checkedTypeCount;
            TypeDuplicates = typeDuplicates ?? throw new ArgumentNullException(nameof(typeDuplicates));
            FamilyDuplicates = familyDuplicates ?? throw new ArgumentNullException(nameof(familyDuplicates));
            NearDuplicates = nearDuplicates ?? throw new ArgumentNullException(nameof(nearDuplicates));
        }

        /// <summary>Gets how many types were compared.</summary>
        public int CheckedTypeCount { get; }

        /// <summary>Gets the types that repeat another type of the same family.</summary>
        public IReadOnlyList<DuplicateTypeRow> TypeDuplicates { get; }

        /// <summary>Gets the families that repeat another family.</summary>
        public IReadOnlyList<DuplicateFamilyRow> FamilyDuplicates { get; }

        /// <summary>Gets the types that differ in only a few parameters.</summary>
        public IReadOnlyList<NearDuplicatePair> NearDuplicates { get; }

        /// <summary>Gets the number of duplicate types that have no placed instances and can be deleted.</summary>
        public int RemovableTypeCount => TypeDuplicates.Count(row => row.IsRemovable);

        /// <summary>Gets the number of duplicate families that have no placed instances and can be deleted.</summary>
        public int RemovableFamilyCount => FamilyDuplicates.Count(row => row.IsRemovable);

        /// <summary>Gets a value indicating whether anything at all was found.</summary>
        public bool IsEmpty => TypeDuplicates.Count == 0 && FamilyDuplicates.Count == 0;
    }
}
