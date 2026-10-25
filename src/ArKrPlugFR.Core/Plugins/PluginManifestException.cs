using System;

namespace ArKrPlugFR.Core.Plugins
{
    /// <summary>
    /// Thrown when a <c>*.plugin.json</c> file is malformed. The message names the
    /// offending field, because the file is written by hand or by a build script
    /// and the failure has to be obvious from the log.
    /// </summary>
    public sealed class PluginManifestException : Exception
    {
        /// <summary>Creates the exception for a given file and reason.</summary>
        /// <param name="path">Path of the manifest file.</param>
        /// <param name="reason">Explanation of the failure.</param>
        /// <param name="innerException">Optional underlying exception.</param>
        public PluginManifestException(string path, string reason, Exception? innerException = null)
            : base("Invalid plugin manifest '" + path + "': " + reason, innerException)
        {
            Path = path;
            Reason = reason;
        }

        /// <summary>Gets the path of the manifest file.</summary>
        public string Path { get; }

        /// <summary>Gets the explanation of the failure.</summary>
        public string Reason { get; }
    }
}
