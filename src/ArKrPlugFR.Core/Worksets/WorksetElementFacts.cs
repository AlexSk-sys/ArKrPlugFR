using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// What the rules may look at on one element: the category, the family, the type and
    /// the values of the parameters the rules name.
    /// <para>
    /// The Revit adapter reads these facts and <see cref="WorksetRules"/> decides on them,
    /// so every matching rule is exercised by unit tests without Revit.
    /// </para>
    /// </summary>
    public sealed class WorksetElementFacts
    {
        private readonly Dictionary<string, string?> _parameters;

        /// <summary>Creates the facts of one element.</summary>
        /// <param name="category">Category name as shown in the interface; may be absent.</param>
        /// <param name="categoryIdentifier">Built-in category name such as <c>OST_Walls</c>; may be absent.</param>
        /// <param name="family">Name of the family of the element; may be absent.</param>
        /// <param name="type">Name of the type of the element; may be absent.</param>
        /// <param name="parameters">Values of the parameters the rules name; may be absent.</param>
        public WorksetElementFacts(
            string? category,
            string? categoryIdentifier,
            string? family,
            string? type,
            IReadOnlyDictionary<string, string?>? parameters = null)
        {
            Category = category;
            CategoryIdentifier = categoryIdentifier;
            Family = family;
            Type = type;

            _parameters = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            if (parameters != null)
            {
                foreach (KeyValuePair<string, string?> pair in parameters)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key))
                    {
                        _parameters[pair.Key] = pair.Value;
                    }
                }
            }
        }

        /// <summary>Gets the category name as shown in the interface, or <c>null</c>.</summary>
        public string? Category { get; }

        /// <summary>Gets the built-in category name such as <c>OST_Walls</c>, or <c>null</c>.</summary>
        public string? CategoryIdentifier { get; }

        /// <summary>Gets the name of the family of the element, or <c>null</c>.</summary>
        public string? Family { get; }

        /// <summary>Gets the name of the type of the element, or <c>null</c>.</summary>
        public string? Type { get; }

        /// <summary>Reads a parameter value; names are matched without case.</summary>
        /// <param name="name">Name of the parameter.</param>
        /// <returns>The value, or <c>null</c> when the parameter is absent or empty.</returns>
        public string? Parameter(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return _parameters.TryGetValue(name!.Trim(), out string? value) ? WorksetRules.Normalize(value) : null;
        }
    }
}
