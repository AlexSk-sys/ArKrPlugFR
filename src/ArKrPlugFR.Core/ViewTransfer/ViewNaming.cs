using System;
using System.Collections.Generic;
using System.Globalization;

namespace ArKrPlugFR.Core.ViewTransfer
{
    /// <summary>
    /// Naming rules for the copies the transfer creates.
    /// <para>
    /// A project may hold only one view of a name, so a copy that would collide
    /// with an existing view — or with another copy planned in the same run —
    /// gets a numbered suffix. The tool never overwrites; it only renames its
    /// own copy.
    /// </para>
    /// </summary>
    public static class ViewNaming
    {
        /// <summary>Name used when the source view has no usable name.</summary>
        public const string FallbackName = "View";

        /// <summary>Cleans a view name; an empty name becomes the fallback.</summary>
        /// <param name="name">Name as read from the source project; may be null.</param>
        /// <returns>The trimmed name, or <see cref="FallbackName"/>.</returns>
        public static string Normalize(string? name)
        {
            return string.IsNullOrWhiteSpace(name) ? FallbackName : name!.Trim();
        }

        /// <summary>
        /// Returns the first name that does not collide with the taken ones:
        /// the name itself, or the name with a <c> (2)</c>, <c> (3)</c> … suffix.
        /// </summary>
        /// <param name="name">Desired name; normalized first.</param>
        /// <param name="takenNames">Names already used, compared case-insensitively.</param>
        /// <returns>A name that is not in <paramref name="takenNames"/>.</returns>
        public static string MakeUnique(string name, ICollection<string> takenNames)
        {
            if (takenNames == null)
            {
                throw new ArgumentNullException(nameof(takenNames));
            }

            string normalized = Normalize(name);

            if (!IsTaken(normalized, takenNames))
            {
                return normalized;
            }

            for (int suffix = 2; ; suffix++)
            {
                string candidate = normalized + " (" + suffix.ToString(CultureInfo.InvariantCulture) + ")";

                if (!IsTaken(candidate, takenNames))
                {
                    return candidate;
                }
            }
        }

        /// <summary>Determines whether a name is already used, ignoring case.</summary>
        /// <param name="name">Name to look for.</param>
        /// <param name="takenNames">Names already used.</param>
        /// <returns><c>true</c> when the name is taken.</returns>
        public static bool IsTaken(string name, ICollection<string> takenNames)
        {
            if (takenNames == null)
            {
                throw new ArgumentNullException(nameof(takenNames));
            }

            foreach (string taken in takenNames)
            {
                if (string.Equals(taken, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
