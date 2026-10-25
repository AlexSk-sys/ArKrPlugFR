using System;

namespace ArKrPlugFR.Core.Types
{
    /// <summary>
    /// One family type as the type tree sees it: where it sits in the project and which
    /// element it is.
    /// </summary>
    public sealed class TypeFact
    {
        /// <summary>Creates a fact.</summary>
        /// <param name="id">Element id of the type.</param>
        /// <param name="category">Name of the category, or <c>null</c> for a type without one.</param>
        /// <param name="family">Name of the family.</param>
        /// <param name="type">Name of the type.</param>
        public TypeFact(long id, string? category, string family, string type)
        {
            Id = id;
            Category = category;
            Family = family ?? throw new ArgumentNullException(nameof(family));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>Gets the element id of the type.</summary>
        public long Id { get; }

        /// <summary>Gets the name of the category, or <c>null</c>.</summary>
        public string? Category { get; }

        /// <summary>Gets the name of the family.</summary>
        public string Family { get; }

        /// <summary>Gets the name of the type.</summary>
        public string Type { get; }
    }
}
