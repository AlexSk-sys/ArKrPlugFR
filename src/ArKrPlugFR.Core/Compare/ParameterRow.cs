using System;

namespace ArKrPlugFR.Core.Compare
{
    /// <summary>
    /// One row of the comparison: a parameter name, the value of the source element and the
    /// value of the target element.
    /// </summary>
    public sealed class ParameterRow
    {
        /// <summary>Creates a row.</summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="source">Value of the source element; empty when it has none.</param>
        /// <param name="target">Value of the target element; empty when it has none.</param>
        public ParameterRow(string name, string? source, string? target)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? string.Empty;
            Target = target ?? string.Empty;
        }

        /// <summary>Gets the name of the parameter.</summary>
        public string Name { get; }

        /// <summary>Gets the value of the source element.</summary>
        public string Source { get; }

        /// <summary>Gets the value of the target element.</summary>
        public string Target { get; }

        /// <summary>Gets a value indicating whether the two values differ.</summary>
        public bool Differs => !string.Equals(Source.Trim(), Target.Trim(), StringComparison.Ordinal);
    }
}
