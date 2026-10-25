using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArKrPlugFR.Core.Plugins
{
    /// <summary>
    /// Reads <c>*.plugin.json</c> manifests.
    /// <para>
    /// Parsing is strict on purpose: a tool with a broken manifest is reported
    /// with the file name and the offending field instead of appearing as a dead
    /// button on the ribbon.
    /// </para>
    /// </summary>
    public static class PluginManifestReader
    {
        /// <summary>File name pattern the host scans for.</summary>
        public const string FilePattern = "*.plugin.json";

        /// <summary>Parses a manifest from JSON text.</summary>
        /// <param name="json">JSON content of the manifest.</param>
        /// <param name="path">Path used in error messages; may be omitted for in-memory input.</param>
        /// <exception cref="PluginManifestException">The JSON is malformed or a required field is missing.</exception>
        public static PluginManifest Parse(string json, string path = "<memory>")
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new PluginManifestException(path, "the file is empty.");
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (JsonException exception)
            {
                throw new PluginManifestException(path, "the JSON is malformed: " + exception.Message, exception);
            }

            string id = ReadRequiredString(root, "id", path);
            string assembly = ReadRequiredString(root, "assembly", path);
            string entryType = ReadRequiredString(root, "entryType", path);

            IReadOnlyDictionary<string, string> titles = ReadLocalizedMap(root, "titles", path, required: true);
            IReadOnlyDictionary<string, string> descriptions = ReadLocalizedMap(root, "descriptions", path, required: false);
            RibbonPlacement ribbon = ReadRibbon(root, path);
            IReadOnlyCollection<string> versions = ReadVersionList(root, "supportedRevitVersions");

            return new PluginManifest(id, assembly, entryType, titles, descriptions, ribbon, versions);
        }

        /// <summary>Reads every manifest in a directory, without throwing on individual bad files.</summary>
        /// <param name="directory">Directory to scan.</param>
        /// <returns>The valid manifests and the errors found, so the caller can log both.</returns>
        public static PluginManifestLoadResult LoadDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("The directory must not be empty.", nameof(directory));
            }

            if (!Directory.Exists(directory))
            {
                return new PluginManifestLoadResult(Array.Empty<PluginManifest>(), Array.Empty<string>());
            }

            var manifests = new List<PluginManifest>();
            var errors = new List<string>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.EnumerateFiles(directory, FilePattern, SearchOption.TopDirectoryOnly).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    PluginManifest manifest = Parse(File.ReadAllText(file), file);

                    if (!seenIds.Add(manifest.Id))
                    {
                        errors.Add(file + ": duplicate plugin id '" + manifest.Id + "', the file is ignored.");
                        continue;
                    }

                    manifests.Add(manifest);
                }
                catch (PluginManifestException exception)
                {
                    errors.Add(exception.Message);
                }
                catch (IOException exception)
                {
                    errors.Add(file + ": " + exception.Message);
                }
            }

            return new PluginManifestLoadResult(manifests, errors);
        }

        private static string ReadRequiredString(JObject root, string property, string path)
        {
            JToken? token = root[property];
            if (token == null || token.Type != JTokenType.String || string.IsNullOrWhiteSpace(token.Value<string>()))
            {
                throw new PluginManifestException(path, "the required string field '" + property + "' is missing.");
            }

            return token.Value<string>()!.Trim();
        }

        private static IReadOnlyDictionary<string, string> ReadLocalizedMap(
            JObject root,
            string property,
            string path,
            bool required)
        {
            JToken? token = root[property];
            if (token == null)
            {
                if (required)
                {
                    throw new PluginManifestException(path, "the required object field '" + property + "' is missing.");
                }

                return new Dictionary<string, string>();
            }

            if (token.Type != JTokenType.Object)
            {
                throw new PluginManifestException(path, "the field '" + property + "' must be an object keyed by culture, for example { \"en\": \"...\", \"ru\": \"...\" }.");
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (JProperty pair in ((JObject)token).Properties())
            {
                string? value = pair.Value.Type == JTokenType.String ? pair.Value.Value<string>() : null;
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new PluginManifestException(path, "the field '" + property + "." + pair.Name + "' must be a non-empty string.");
                }

                values[pair.Name] = value!.Trim();
            }

            if (required && values.Count == 0)
            {
                throw new PluginManifestException(path, "the field '" + property + "' must contain at least one caption.");
            }

            return values;
        }

        private static RibbonPlacement ReadRibbon(JObject root, string path)
        {
            JToken? token = root["ribbon"];
            if (token == null)
            {
                throw new PluginManifestException(path, "the required object field 'ribbon' is missing.");
            }

            if (token.Type != JTokenType.Object)
            {
                throw new PluginManifestException(path, "the field 'ribbon' must be an object.");
            }

            var ribbon = (JObject)token;

            try
            {
                return new RibbonPlacement(
                    ReadRequiredString(ribbon, "tab", path),
                    ReadRequiredString(ribbon, "panel", path),
                    ribbon["buttonSize"]?.Value<string>() ?? "large",
                    ribbon["order"]?.Value<int?>() ?? 0);
            }
            catch (ArgumentException exception)
            {
                throw new PluginManifestException(path, exception.Message, exception);
            }
        }

        private static IReadOnlyCollection<string> ReadVersionList(JObject root, string property)
        {
            JToken? token = root[property];
            if (token == null || token.Type == JTokenType.Null)
            {
                return Array.Empty<string>();
            }

            if (token.Type == JTokenType.Array)
            {
                return token
                    .Values<string>()
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!.Trim())
                    .ToArray();
            }

            return new[] { token.Value<string>()?.Trim() ?? string.Empty }.Where(v => v.Length > 0).ToArray();
        }
    }
}
