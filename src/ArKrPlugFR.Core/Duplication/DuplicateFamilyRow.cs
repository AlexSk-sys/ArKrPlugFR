using System;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// A family whose types repeat another family's types.
    /// </summary>
    public sealed class DuplicateFamilyRow
    {
        /// <summary>Creates a row.</summary>
        /// <param name="duplicate">The family proposed for removal.</param>
        /// <param name="kept">The family that is kept.</param>
        /// <param name="matchedTypeCount">How many of the kept family's types the duplicate family repeats.</param>
        public DuplicateFamilyRow(FamilyRecord duplicate, FamilyRecord kept, int matchedTypeCount)
        {
            Duplicate = duplicate ?? throw new ArgumentNullException(nameof(duplicate));
            Kept = kept ?? throw new ArgumentNullException(nameof(kept));

            if (matchedTypeCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matchedTypeCount), matchedTypeCount, "A count cannot be negative.");
            }

            MatchedTypeCount = matchedTypeCount;
        }

        /// <summary>Gets the family proposed for removal.</summary>
        public FamilyRecord Duplicate { get; }

        /// <summary>Gets the family that is kept.</summary>
        public FamilyRecord Kept { get; }

        /// <summary>Gets how many of the kept family's types the duplicate family repeats.</summary>
        public int MatchedTypeCount { get; }

        /// <summary>Gets the number of types of the duplicate family.</summary>
        public int DuplicateTypeCount => Duplicate.Types.Count;

        /// <summary>Gets the number of types of the kept family.</summary>
        public int KeptTypeCount => Kept.Types.Count;

        /// <summary>Gets the total number of placed instances of the duplicate family.</summary>
        public int InstanceCount => Duplicate.InstanceCount;

        /// <summary>Gets a value indicating whether the duplicate can be deleted.</summary>
        public bool IsRemovable => InstanceCount == 0;
    }
}
