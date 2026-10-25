using System;
using System.Globalization;

namespace ArKrPlugFR.Core.Localization
{
    /// <summary>
    /// A short user-facing string in the languages the add-in supports.
    /// <para>
    /// Tool strings are written as typed values rather than looked up by key, so a
    /// missing or misspelled string is a compile error instead of an empty label
    /// in the interface. English is the fallback for any other culture.
    /// </para>
    /// </summary>
    public readonly struct LocalizedText : IEquatable<LocalizedText>
    {
        private readonly string? _english;
        private readonly string? _russian;

        /// <summary>Creates a string with an English and a Russian variant.</summary>
        /// <param name="english">English text; required.</param>
        /// <param name="russian">Russian text; falls back to English when empty.</param>
        public LocalizedText(string english, string russian)
        {
            if (string.IsNullOrWhiteSpace(english))
            {
                throw new ArgumentException("The English text must not be empty.", nameof(english));
            }

            _english = english;
            _russian = russian;
        }

        /// <summary>Creates a string used in every language.</summary>
        public static LocalizedText Invariant(string text)
        {
            return new LocalizedText(text, text);
        }

        /// <summary>Resolves the text for a two-letter culture code, for example <c>ru</c>.</summary>
        public string Resolve(string? culture)
        {
            if (IsRussian(culture) && !string.IsNullOrWhiteSpace(_russian))
            {
                return _russian!;
            }

            return _english ?? string.Empty;
        }

        /// <summary>Resolves the text and formats it with the current culture's separators.</summary>
        public string Resolve(string? culture, params object?[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return Resolve(culture);
            }

            return string.Format(CultureInfo.InvariantCulture, Resolve(culture), arguments);
        }

        /// <summary>Determines whether the given code selects the Russian variant.</summary>
        public static bool IsRussian(string? culture)
        {
            return string.Equals(culture, "ru", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool Equals(LocalizedText other)
        {
            return string.Equals(_english, other._english, StringComparison.Ordinal)
                && string.Equals(_russian, other._russian, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is LocalizedText other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = _english?.GetHashCode() ?? 0;
                return (hash * 397) ^ (_russian?.GetHashCode() ?? 0);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _english ?? string.Empty;
        }
    }
}
