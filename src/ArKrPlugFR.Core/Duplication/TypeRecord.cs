using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// One family type as read from the project: what it is called, how many
    /// instances of it are placed, and its parameter values.
    /// </summary>
    public sealed class TypeRecord
    {
        /// <summary>Creates a type record.</summary>
        /// <param name="id">Element id of the type.</param>
        /// <param name="familyId">Element id of the owning family.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="instanceCount">Number of placed instances of this type.</param>
        /// <param name="values">Raw parameter values keyed by parameter name.</param>
        public TypeRecord(
            long id,
            long familyId,
            string typeName,
            int instanceCount,
            IReadOnlyDictionary<string, string> values)
        {
            if (instanceCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(instanceCount), instanceCount, "A count cannot be negative.");
            }

            Id = id;
            FamilyId = familyId;
            TypeName = typeName ?? string.Empty;
            InstanceCount = instanceCount;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>Gets the element id of the type.</summary>
        public long Id { get; }

        /// <summary>Gets the element id of the owning family.</summary>
        public long FamilyId { get; }

        /// <summary>Gets the name of the type.</summary>
        public string TypeName { get; }

        /// <summary>Gets the number of placed instances of this type.</summary>
        public int InstanceCount { get; }

        /// <summary>Gets the raw parameter values keyed by parameter name.</summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>Gets a value indicating whether the type can be deleted, which needs zero instances.</summary>
        public bool IsRemovable => InstanceCount == 0;
    }
}
