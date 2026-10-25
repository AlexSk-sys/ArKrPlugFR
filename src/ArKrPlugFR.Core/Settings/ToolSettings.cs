using System;
using System.IO;
using Newtonsoft.Json;

namespace ArKrPlugFR.Core.Settings
{
    /// <summary>
    /// Remembers a tool's switches between sessions as one JSON file per tool.
    /// <para>
    /// A tool has to work whether or not it may write anything down, so a missing,
    /// unreadable or corrupt file falls back to the defaults the caller hands in, and a
    /// write that fails is swallowed rather than shown to the user.
    /// </para>
    /// </summary>
    public static class ToolSettings
    {
        /// <summary>Reads a tool's settings, falling back to the given defaults.</summary>
        /// <typeparam name="T">Type of the settings object.</typeparam>
        /// <param name="directory">Directory that holds the settings files.</param>
        /// <param name="toolName">Name of the tool; it names the file.</param>
        /// <param name="fallback">Settings used when nothing readable is stored.</param>
        /// <returns>The stored settings, or the fallback.</returns>
        public static T Load<T>(string directory, string toolName, T fallback)
            where T : class
        {
            if (fallback == null)
            {
                throw new ArgumentNullException(nameof(fallback));
            }

            try
            {
                string path = PathOf(directory, toolName);

                if (!File.Exists(path))
                {
                    return fallback;
                }

                T? stored = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

                return stored ?? fallback;
            }
            catch (Exception)
            {
                // A file that cannot be read is the same as no file at all.
                return fallback;
            }
        }

        /// <summary>Writes a tool's settings.</summary>
        /// <typeparam name="T">Type of the settings object.</typeparam>
        /// <param name="directory">Directory that holds the settings files.</param>
        /// <param name="toolName">Name of the tool; it names the file.</param>
        /// <param name="settings">Settings to write down.</param>
        /// <returns><c>true</c> when the settings were written.</returns>
        public static bool Save<T>(string directory, string toolName, T settings)
            where T : class
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            try
            {
                string path = PathOf(directory, toolName);
                string? folder = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Builds the path of a tool's settings file.</summary>
        /// <param name="directory">Directory that holds the settings files.</param>
        /// <param name="toolName">Name of the tool; it names the file.</param>
        /// <returns>The full path of the file.</returns>
        public static string PathOf(string directory, string toolName)
        {
            if (directory == null || directory.Trim().Length == 0)
            {
                throw new ArgumentException("The settings directory must be given.", nameof(directory));
            }

            if (!IsSimpleName(toolName))
            {
                throw new ArgumentException(
                    "The tool name must be a plain file name without folders or extensions.",
                    nameof(toolName));
            }

            return Path.Combine(directory, toolName + ".json");
        }

        private static bool IsSimpleName(string? name)
        {
            if (name == null || name.Trim().Length == 0)
            {
                return false;
            }

            foreach (char character in name)
            {
                if (!char.IsLetterOrDigit(character) && character != '-' && character != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
