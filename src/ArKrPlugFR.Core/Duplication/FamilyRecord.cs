using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// A family loaded in the project, with all of its types.
    /// </summary>
    public sealed class FamilyRecord
    {
        /// <summary>Creates a family record.</summary>
        /// <param name="name">Name of the family.</param>
        /// <param name="categoryName">Name of the category the family belongs to.</param>
        /// <param name="types">Types of the family.</param>
        /// <param name="id">Element id of the family; zero when it is not known.</param>
        public FamilyRecord(string name, string categoryName, IReadOnlyList<TypeRecord> types, long id = 0L)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The family must have a name.", nameof(name));
            }

            Name = name;
            CategoryName = categoryName ?? string.Empty;
            Types = types ?? throw new ArgumentNullException(nameof(types));
            Id = id;
        }

        /// <summary>Gets the element id of the family, or zero when it is not known.</summary>
        public long Id { get; }

        /// <summary>Gets the name of the family.</summary>
        public string Name { get; }

        /// <summary>Gets the name of the category the family belongs to.</summary>
        public string CategoryName { get; }

        /// <summary>Gets the types of the family.</summary>
        public IReadOnlyList<TypeRecord> Types { get; }

        /// <summary>Gets the total number of placed instances of every type of the family.</summary>
        public int InstanceCount => Types.Sum(type => type.InstanceCount);
    }
}
