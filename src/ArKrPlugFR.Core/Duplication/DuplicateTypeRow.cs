using System;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// A type that repeats the content of another type in the same family.
    /// </summary>
    public sealed class DuplicateTypeRow
    {
        /// <summary>Creates a row.</summary>
        /// <param name="familyName">Name of the family both types belong to.</param>
        /// <param name="categoryName">Name of the category of the family.</param>
        /// <param name="duplicate">The type proposed for removal.</param>
        /// <param name="kept">The type that is kept.</param>
        public DuplicateTypeRow(string familyName, string categoryName, TypeRecord duplicate, TypeRecord kept)
        {
            FamilyName = familyName ?? string.Empty;
            CategoryName = categoryName ?? string.Empty;
            Duplicate = duplicate ?? throw new ArgumentNullException(nameof(duplicate));
            Kept = kept ?? throw new ArgumentNullException(nameof(kept));
        }

        /// <summary>Gets the name of the family.</summary>
        public string FamilyName { get; }

        /// <summary>Gets the name of the category.</summary>
        public string CategoryName { get; }

        /// <summary>Gets the type proposed for removal.</summary>
        public TypeRecord Duplicate { get; }

        /// <summary>Gets the type that is kept.</summary>
        public TypeRecord Kept { get; }

        /// <summary>Gets a value indicating whether the duplicate can be deleted.</summary>
        public bool IsRemovable => Duplicate.IsRemovable;
    }
}
