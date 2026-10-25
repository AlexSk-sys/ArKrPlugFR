using System;
using System.IO;
using ArKrPlugFR.Core.Plugins;

namespace ArKrPlugFR.Host
{
    /// <summary>
    /// Finds the tools that are deployed next to the host.
    /// <para>
    /// The deployment is a flat folder: the host assembly, the tool assemblies
    /// and one <c>*.plugin.json</c> per tool. There is no registry entry and no
    /// absolute path, so the same folder works on any machine.
    /// </para>
    /// </summary>
    public sealed class PluginCatalog
    {
        /// <summary>Creates a catalog over the given directory.</summary>
        /// <param name="directory">Directory that holds the tool assemblies and manifests.</param>
        public PluginCatalog(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("The directory must not be empty.", nameof(directory));
            }

            Directory = directory;
        }

        /// <summary>Gets the directory that holds the tools.</summary>
        public string Directory { get; }

        /// <summary>Creates a catalog for the folder of the host assembly.</summary>
        public static PluginCatalog CreateForHostAssembly()
        {
            string location = typeof(PluginCatalog).Assembly.Location;
            string directory = Path.GetDirectoryName(location);

            if (string.IsNullOrEmpty(directory))
            {
                directory = AppDomain.CurrentDomain.BaseDirectory;
            }

            return new PluginCatalog(directory);
        }

        /// <summary>
        /// Returns the manifests whose tool assembly is present and whose
        /// supported Revit releases include <paramref name="revitVersion"/>.
        /// Missing assemblies are reported as errors rather than dropped silently.
        /// </summary>
        /// <param name="revitVersion">Revit release of the running session, for example <c>2022</c>.</param>
        public PluginManifestLoadResult Discover(string revitVersion)
        {
            PluginManifestLoadResult loaded = PluginManifestReader.LoadDirectory(Directory);
            var manifests = new System.Collections.Generic.List<PluginManifest>();
            var errors = new System.Collections.Generic.List<string>(loaded.Errors);

            foreach (PluginManifest manifest in loaded.Manifests)
            {
                if (!manifest.SupportsRevitVersion(revitVersion))
                {
                    continue;
                }

                string assemblyPath = Path.Combine(Directory, manifest.Assembly);

                if (!File.Exists(assemblyPath))
                {
                    errors.Add(
                        "Plugin '" + manifest.Id + "': assembly '" + manifest.Assembly +
                        "' was not found in '" + Directory + "', the button is skipped.");
                    continue;
                }

                manifests.Add(manifest);
            }

            return new PluginManifestLoadResult(manifests, errors);
        }
    }
}
