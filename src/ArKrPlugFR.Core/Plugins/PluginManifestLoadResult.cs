using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Plugins
{
    /// <summary>Result of scanning a directory for plugin manifests.</summary>
    public sealed class PluginManifestLoadResult
    {
        /// <summary>Creates a result from the manifests that loaded and the errors that were found.</summary>
        /// <param name="manifests">Manifests that parsed and validated.</param>
        /// <param name="errors">Human-readable problems, one per skipped file.</param>
        public PluginManifestLoadResult(IReadOnlyList<PluginManifest> manifests, IReadOnlyList<string> errors)
        {
            Manifests = manifests ?? throw new ArgumentNullException(nameof(manifests));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        /// <summary>Gets the manifests that parsed and validated.</summary>
        public IReadOnlyList<PluginManifest> Manifests { get; }

        /// <summary>Gets the problems found, one per skipped file.</summary>
        public IReadOnlyList<string> Errors { get; }
    }
}
