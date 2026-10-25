using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Plugins;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Host.Ribbon
{
    /// <summary>
    /// Turns plugin manifests into a ribbon tab.
    /// <para>
    /// Panels and tabs are created once and reused, so the add-in survives being
    /// loaded next to another add-in that already created the same tab, and a
    /// second call cannot create a duplicate panel (Revit throws on duplicates).
    /// </para>
    /// </summary>
    public sealed class RibbonBuilder
    {
        private readonly UIControlledApplication _uiApplication;
        private readonly Log _log;

        /// <summary>Creates a builder over the running Revit session.</summary>
        /// <param name="uiApplication">The Revit session.</param>
        /// <param name="log">Logger used to report skipped tools.</param>
        public RibbonBuilder(UIControlledApplication uiApplication, Log log)
        {
            _uiApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Adds one large button per manifest and returns the number of buttons
        /// that were created.
        /// </summary>
        /// <param name="manifests">Tools to place on the ribbon.</param>
        /// <param name="assemblyDirectory">Directory that contains the tool assemblies.</param>
        /// <param name="culture">Two-letter culture code used for captions.</param>
        public int Build(IReadOnlyList<PluginManifest> manifests, string assemblyDirectory, string culture)
        {
            if (manifests == null)
            {
                throw new ArgumentNullException(nameof(manifests));
            }

            if (assemblyDirectory == null)
            {
                throw new ArgumentNullException(nameof(assemblyDirectory));
            }

            int created = 0;

            foreach (IGrouping<string, PluginManifest> tabGroup in manifests
                .GroupBy(manifest => manifest.Ribbon.Tab)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                EnsureTab(tabGroup.Key);

                foreach (IGrouping<string, PluginManifest> panelGroup in tabGroup
                    .GroupBy(manifest => manifest.Ribbon.Panel)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
                {
                    RibbonPanel? panel = EnsurePanel(tabGroup.Key, panelGroup.Key);

                    if (panel == null)
                    {
                        _log.Warn("Ribbon panel '" + panelGroup.Key + "' on tab '" + tabGroup.Key + "' is not available.", nameof(RibbonBuilder));
                        continue;
                    }

                    foreach (PluginManifest manifest in panelGroup
                        .OrderBy(item => item.Ribbon.Order)
                        .ThenBy(item => item.GetTitle(culture), StringComparer.OrdinalIgnoreCase))
                    {
                        if (AddButton(panel, manifest, assemblyDirectory, culture))
                        {
                            created++;
                        }
                    }
                }
            }

            return created;
        }

        /// <summary>
        /// Adds a single large button, for example one that belongs to the host
        /// rather than to a plugin folder.
        /// </summary>
        /// <param name="tab">Ribbon tab.</param>
        /// <param name="panel">Panel inside the tab.</param>
        /// <param name="name">Internal name, must be unique across the ribbon.</param>
        /// <param name="text">Caption shown on the button.</param>
        /// <param name="tooltip">Tooltip text.</param>
        /// <param name="assemblyPath">Assembly that contains the command.</param>
        /// <param name="className">Full name of the command type.</param>
        public bool AddHostButton(
            string tab,
            string panel,
            string name,
            string text,
            string tooltip,
            string assemblyPath,
            string className)
        {
            EnsureTab(tab);
            RibbonPanel? ribbonPanel = EnsurePanel(tab, panel);

            if (ribbonPanel == null)
            {
                return false;
            }

            try
            {
                var data = new PushButtonData(name, text, assemblyPath, className)
                {
                    ToolTip = tooltip,
                };

                ribbonPanel.AddItem(data);
                return true;
            }
            catch (Exception exception)
            {
                _log.Error("Failed to add button '" + name + "': " + exception.Message, nameof(RibbonBuilder), exception);
                return false;
            }
        }

        private bool AddButton(RibbonPanel panel, PluginManifest manifest, string assemblyDirectory, string culture)
        {
            string assemblyPath = Path.Combine(assemblyDirectory, manifest.Assembly);

            if (!string.Equals(manifest.Ribbon.ButtonSize, "large", StringComparison.OrdinalIgnoreCase))
            {
                _log.Warn(
                    "Plugin '" + manifest.Id + "' requests a '" + manifest.Ribbon.ButtonSize +
                    "' button; only large buttons are supported at the moment, a large button is used instead.",
                    nameof(RibbonBuilder));
            }

            try
            {
                var data = new PushButtonData(manifest.Id, manifest.GetTitle(culture), assemblyPath, manifest.EntryType)
                {
                    ToolTip = manifest.GetDescription(culture),
                };

                panel.AddItem(data);
                return true;
            }
            catch (Exception exception)
            {
                _log.Error("Failed to add plugin '" + manifest.Id + "': " + exception.Message, nameof(RibbonBuilder), exception);
                return false;
            }
        }

        private void EnsureTab(string tab)
        {
            try
            {
                if (_uiApplication.GetRibbonPanels(tab).Count == 0)
                {
                    _uiApplication.CreateRibbonTab(tab);
                }
            }
            catch (Exception exception)
            {
                // The tab can already exist because another add-in created it, or
                // because Revit does not allow it; both are not fatal here.
                _log.Warn("Ribbon tab '" + tab + "' was not created: " + exception.Message, nameof(RibbonBuilder));
            }
        }

        private RibbonPanel? EnsurePanel(string tab, string panel)
        {
            try
            {
                RibbonPanel? existing = _uiApplication
                    .GetRibbonPanels(tab)
                    .FirstOrDefault(item => string.Equals(item.Name, panel, StringComparison.OrdinalIgnoreCase));

                return existing ?? _uiApplication.CreateRibbonPanel(tab, panel);
            }
            catch (Exception exception)
            {
                _log.Error("Ribbon panel '" + panel + "' is not available: " + exception.Message, nameof(RibbonBuilder), exception);
                return null;
            }
        }
    }
}
