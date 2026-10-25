using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Types
{
    /// <summary>One family of the tree: its types, in order.</summary>
    public sealed class FamilyGroup
    {
        /// <summary>Creates a family group.</summary>
        /// <param name="name">Name of the family.</param>
        /// <param name="types">Types of the family.</param>
        public FamilyGroup(string name, IReadOnlyList<TypeFact> types)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        /// <summary>Gets the name of the family.</summary>
        public string Name { get; }

        /// <summary>Gets the types of the family.</summary>
        public IReadOnlyList<TypeFact> Types { get; }

        /// <summary>Gets how many types the family holds.</summary>
        public int TypeCount => Types.Count;
    }

    /// <summary>One category of the tree: its families, in order.</summary>
    public sealed class CategoryGroup
    {
        /// <summary>Creates a category group.</summary>
        /// <param name="name">Name of the category.</param>
        /// <param name="families">Families of the category.</param>
        public CategoryGroup(string name, IReadOnlyList<FamilyGroup> families)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Families = families ?? throw new ArgumentNullException(nameof(families));
        }

        /// <summary>Gets the name of the category.</summary>
        public string Name { get; }

        /// <summary>Gets the families of the category.</summary>
        public IReadOnlyList<FamilyGroup> Families { get; }

        /// <summary>Gets how many types the category holds.</summary>
        public int TypeCount
        {
            get
            {
                int total = 0;

                foreach (FamilyGroup family in Families)
                {
                    total += family.TypeCount;
                }

                return total;
            }
        }
    }
}
