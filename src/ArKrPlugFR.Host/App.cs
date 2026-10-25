using System;
using System.Globalization;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Plugins;
using ArKrPlugFR.Host.Ribbon;
using ArKrPlugFR.Host.Commands;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Host
{
    /// <summary>
    /// Revit entry point declared in <c>ArKrPlugFR.addin</c>.
    /// <para>
    /// The host owns the ribbon only. Each tool is a separate command declared in
    /// its own manifest, which keeps the host free of tool logic and lets tools
    /// be built and versioned independently.
    /// </para>
    /// </summary>
    public sealed class App : IExternalApplication
    {
        /// <summary>Ribbon tab that hosts every tool.</summary>
        public const string RibbonTabName = "ArKrPlugFR";

        private readonly Log _log = CreateLog();

        /// <inheritdoc />
        public Result OnStartup(UIControlledApplication application)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            try
            {
                string revitVersion = application.ControlledApplication.VersionNumber;
                string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                _log.Info("Add-in starting on Revit " + revitVersion + ".", nameof(App));

                PluginCatalog catalog = PluginCatalog.CreateForHostAssembly();
                PluginManifestLoadResult discovered = catalog.Discover(revitVersion);

                foreach (string error in discovered.Errors)
                {
                    _log.Warn(error, nameof(App));
                }

                var builder = new RibbonBuilder(application, _log);
                int buttons = builder.Build(discovered.Manifests, catalog.Directory, culture);

                buttons += AddHostButtons(builder, culture) ? 1 : 0;

                _log.Info("Add-in started with " + buttons + " button(s).", nameof(App));
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                // Faulting here would make Revit disable the add-in silently on the
                // next start, so the failure is logged and reported as-is.
                _log.Error("Add-in failed to start: " + exception.Message, nameof(App), exception);
                return Result.Failed;
            }
        }

        /// <inheritdoc />
        public Result OnShutdown(UIControlledApplication application)
        {
            _log.Info("Add-in stopped.", nameof(App));
            return Result.Succeeded;
        }

        private bool AddHostButtons(RibbonBuilder builder, string culture)
        {
            bool isRussian = string.Equals(culture, "ru", StringComparison.OrdinalIgnoreCase);
            string assemblyPath = typeof(App).Assembly.Location;

            return builder.AddHostButton(
                RibbonTabName,
                isRussian ? "Справка" : "Help",
                "ArKrPlugFR.About",
                isRussian ? "О надстройке" : "About",
                isRussian
                    ? "Версия, установленные инструменты и расположение журналов."
                    : "Version, installed tools and log locations.",
                assemblyPath,
                typeof(AboutCommand).FullName!);
        }

        private static Log CreateLog()
        {
            var log = new Log();

            try
            {
                log.Add(new FileLogSink(AppPaths.LogDirectory));
            }
            catch (Exception)
            {
                // The add-in still has to load when the log directory is unwritable.
            }

            return log;
        }
    }
}
