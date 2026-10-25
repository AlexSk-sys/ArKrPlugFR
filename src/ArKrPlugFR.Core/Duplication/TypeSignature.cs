using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// The comparable content of one family type: every parameter that describes
    /// the type, with its value normalised.
    /// <para>
    /// Identity and free-text parameters are left out — a copy normally has those
    /// filled on one side and empty on the other, and comparing them would hide
    /// exactly the duplicate the user is looking for. Parameters without a value
    /// are left out too, so a type that simply does not carry a parameter still
    /// matches one where that parameter is empty.
    /// </para>
    /// </summary>
    public sealed class TypeSignature : IEquatable<TypeSignature>
    {
        private readonly SortedDictionary<string, string> _values;

        /// <summary>Creates a signature from already normalised values.</summary>
        /// <param name="values">Normalised values keyed by parameter name; empty values are ignored.</param>
        public TypeSignature(IEnumerable<KeyValuePair<string, string>> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _values = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, string> pair in values)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }

                _values[pair.Key.Trim()] = pair.Value;
            }
        }

        /// <summary>Gets the normalised values keyed by parameter name.</summary>
        public IReadOnlyDictionary<string, string> Values => _values;

        /// <summary>
        /// Builds the signature of a type, applying the options.
        /// </summary>
        /// <param name="type">The type to describe.</param>
        /// <param name="options">Comparison settings: which parameters to ignore and how to read numbers.</param>
        public static TypeSignature Create(TypeRecord type, DuplicateScanOptions options)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var values = new List<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, string> pair in type.Values)
            {
                if (options.IsExcluded(pair.Key))
                {
                    continue;
                }

                values.Add(new KeyValuePair<string, string>(
                    pair.Key,
                    ParameterValueNormalizer.Normalize(pair.Value, options.SignificantDigits)));
            }

            return new TypeSignature(values);
        }

        /// <summary>
        /// Counts the parameters on which two signatures disagree, including
        /// parameters that only one of them carries.
        /// </summary>
        public int DifferenceCount(TypeSignature other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var names = new HashSet<string>(_values.Keys, StringComparer.Ordinal);
            names.UnionWith(other._values.Keys);

            int differences = 0;

            foreach (string name in names)
            {
                bool mine = _values.TryGetValue(name, out string? myValue);
                bool theirs = other._values.TryGetValue(name, out string? theirValue);

                if (!mine || !theirs || !string.Equals(myValue, theirValue, StringComparison.Ordinal))
                {
                    differences++;
                }
            }

            return differences;
        }

        /// <inheritdoc />
        public bool Equals(TypeSignature? other)
        {
            if (other is null || _values.Count != other._values.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> pair in _values)
            {
                if (!other._values.TryGetValue(pair.Key, out string? value)
                    || !string.Equals(pair.Value, value, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeSignature);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // Order-independent, because two types that carry the same parameters
            // are the same type whatever order the parameters were read in.
            int hash = 17;

            foreach (KeyValuePair<string, string> pair in _values)
            {
                unchecked
                {
                    hash ^= (pair.Key.GetHashCode() * 397) ^ pair.Value.GetHashCode();
                }
            }

            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join("; ", _values.Select(pair => pair.Key + "=" + pair.Value));
        }
    }
}
