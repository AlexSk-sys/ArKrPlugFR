using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Plugins
{
    /// <summary>
    /// Declarative description of a single tool, read from a
    /// <c>*.plugin.json</c> file next to the tool assembly.
    /// <para>
    /// The host builds the ribbon from these files, so adding a tool does not
    /// require recompiling the host, and every tool carries its own bilingual
    /// captions and its own version range.
    /// </para>
    /// </summary>
    public sealed class PluginManifest
    {
        /// <summary>Creates a manifest.</summary>
        /// <param name="id">Stable identifier, for example <c>unjoin-elements</c>.</param>
        /// <param name="assembly">File name of the assembly, for example <c>ArKrPlugFR.Plugins.UnjoinElements.dll</c>.</param>
        /// <param name="entryType">Full name of the command type.</param>
        /// <param name="titles">Button captions keyed by culture, for example <c>en</c> and <c>ru</c>.</param>
        /// <param name="descriptions">Tooltip texts keyed by culture.</param>
        /// <param name="ribbon">Placement of the buttons on the ribbon.</param>
        /// <param name="supportedRevitVersions">Revit releases the tool supports; empty means all supported releases.</param>
        public PluginManifest(
            string id,
            string assembly,
            string entryType,
            IReadOnlyDictionary<string, string> titles,
            IReadOnlyDictionary<string, string> descriptions,
            RibbonPlacement ribbon,
            IReadOnlyCollection<string>? supportedRevitVersions = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The plugin id must not be empty.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(assembly))
            {
                throw new ArgumentException("The assembly name must not be empty.", nameof(assembly));
            }

            if (string.IsNullOrWhiteSpace(entryType))
            {
                throw new ArgumentException("The entry type must not be empty.", nameof(entryType));
            }

            if (titles == null || titles.Count == 0)
            {
                throw new ArgumentException("At least one title is required.", nameof(titles));
            }

            Id = id;
            Assembly = assembly;
            EntryType = entryType;
            Titles = titles;
            Descriptions = descriptions ?? new Dictionary<string, string>();
            Ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
            SupportedRevitVersions = supportedRevitVersions ?? Array.Empty<string>();
        }

        /// <summary>Gets the stable identifier of the tool.</summary>
        public string Id { get; }

        /// <summary>Gets the assembly file name that contains the command.</summary>
        public string Assembly { get; }

        /// <summary>Gets the full name of the command type.</summary>
        public string EntryType { get; }

        /// <summary>Gets the button captions keyed by culture.</summary>
        public IReadOnlyDictionary<string, string> Titles { get; }

        /// <summary>Gets the tooltip texts keyed by culture.</summary>
        public IReadOnlyDictionary<string, string> Descriptions { get; }

        /// <summary>Gets the ribbon placement.</summary>
        public RibbonPlacement Ribbon { get; }

        /// <summary>Gets the supported Revit releases; empty means all supported releases.</summary>
        public IReadOnlyCollection<string> SupportedRevitVersions { get; }

        /// <summary>
        /// Returns the caption for the given culture, falling back to English and
        /// then to any available value, so a missing translation degrades to a
        /// readable label instead of an empty button.
        /// </summary>
        /// <param name="culture">Two-letter culture code, for example <c>ru</c>.</param>
        public string GetTitle(string? culture)
        {
            return Resolve(Titles, culture, Id);
        }

        /// <summary>Returns the tooltip for the given culture, with the same fallback as <see cref="GetTitle"/>.</summary>
        public string GetDescription(string? culture)
        {
            return Resolve(Descriptions, culture, string.Empty);
        }

        /// <summary>Determines whether the tool declares support for the given Revit release.</summary>
        public bool SupportsRevitVersion(string revitVersion)
        {
            if (SupportedRevitVersions.Count == 0)
            {
                return true;
            }

            foreach (string supported in SupportedRevitVersions)
            {
                if (string.Equals(supported, revitVersion, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Resolve(IReadOnlyDictionary<string, string> values, string? culture, string fallback)
        {
            if (values == null || values.Count == 0)
            {
                return fallback;
            }

            if (!string.IsNullOrWhiteSpace(culture) && values.TryGetValue(culture!, out string? exact) && !string.IsNullOrWhiteSpace(exact))
            {
                return exact;
            }

            if (values.TryGetValue("en", out string? english) && !string.IsNullOrWhiteSpace(english))
            {
                return english;
            }

            foreach (KeyValuePair<string, string> pair in values)
            {
                if (!string.IsNullOrWhiteSpace(pair.Value))
                {
                    return pair.Value;
                }
            }

            return fallback;
        }
    }
}
