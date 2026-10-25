using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// The set of type signatures of one family, compared without regard to order.
    /// <para>
    /// Two families are copies of each other when they offer the same types,
    /// whatever order the types are read in. One family can also be a contained
    /// copy of another: every type it offers is offered by the other one as well.
    /// </para>
    /// </summary>
    public sealed class SignatureSet : IEquatable<SignatureSet>
    {
        private readonly HashSet<TypeSignature> _signatures;

        /// <summary>Creates a set from the signatures of a family's types.</summary>
        /// <param name="signatures">Signatures of the family types.</param>
        public SignatureSet(IEnumerable<TypeSignature> signatures)
        {
            if (signatures == null)
            {
                throw new ArgumentNullException(nameof(signatures));
            }

            _signatures = new HashSet<TypeSignature>(signatures);
        }

        /// <summary>Gets the number of distinct signatures in the set.</summary>
        public int Count => _signatures.Count;

        /// <summary>Determines whether every signature of this set is also in the other set.</summary>
        public bool IsSubsetOf(SignatureSet other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return _signatures.IsSubsetOf(other._signatures);
        }

        /// <summary>Determines whether every signature of this set is in the other set, which holds more.</summary>
        public bool IsProperSubsetOf(SignatureSet other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return _signatures.IsProperSubsetOf(other._signatures);
        }

        /// <inheritdoc />
        public bool Equals(SignatureSet? other)
        {
            return other is not null && _signatures.SetEquals(other._signatures);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as SignatureSet);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 17;

            foreach (TypeSignature signature in _signatures)
            {
                unchecked
                {
                    hash ^= signature.GetHashCode();
                }
            }

            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "SignatureSet(" + Count + ")";
        }
    }
}
