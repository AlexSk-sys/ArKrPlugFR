using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Plugins;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Host.Commands
{
    /// <summary>
    /// Shows what is installed: product version, Revit release, every discovered
    /// tool and the folders that hold settings and logs.
    /// <para>
    /// The first question after an update is always "which version am I running",
    /// so the answer is one click away and can be copied into a message.
    /// </para>
    /// </summary>
    public sealed class AboutCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            string revitVersion = context.Document.Application.VersionNumber;
            var culture = new CultureInfo(context.Culture);
            PluginCatalog catalog = PluginCatalog.CreateForHostAssembly();
            PluginManifestLoadResult discovered = catalog.Discover(revitVersion);

            var report = new StringBuilder();
            report.AppendLine("ArKrPlugFR " + GetProductVersion());
            report.AppendLine("Revit " + revitVersion + " (" + context.Document.Application.VersionName + ")");
            report.AppendLine("Culture: " + CultureInfo.CurrentUICulture.Name);
            report.AppendLine("Document: " + context.Document.Title);
            report.AppendLine();
            report.AppendLine("Tools (" + discovered.Manifests.Count + "):");

            foreach (PluginManifest manifest in discovered.Manifests.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
            {
                report.AppendLine("  " + manifest.Id + " — " + manifest.GetTitle(culture.TwoLetterISOLanguageName) + " [" + manifest.Assembly + "]");
            }

            if (discovered.Manifests.Count == 0)
            {
                report.AppendLine("  none");
            }

            if (discovered.Errors.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("Problems (" + discovered.Errors.Count + "):");

                foreach (string error in discovered.Errors)
                {
                    report.AppendLine("  " + error);
                }
            }

            report.AppendLine();
            report.AppendLine("Logs:     " + AppPaths.LogDirectory);
            report.AppendLine("Settings: " + AppPaths.SettingsDirectory);

            Log.Info("About window opened.", nameof(AboutCommand));

            bool isRussian = string.Equals(context.Culture, "ru", StringComparison.OrdinalIgnoreCase);
            var window = new ToolWindow
            {
                Title = isRussian ? "О надстройке — ArKrPlugFR" : "About — ArKrPlugFR",
                Subtitle = isRussian
                    ? "Состав установки и расположение журналов"
                    : "Installed tools and log locations",
                ReportCaption = isRussian ? "Сведения" : "Details",
                Report = report.ToString(),
            };

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }

        private static string GetProductVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string? informational = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informational))
            {
                return informational!;
            }

            Version? version = assembly.GetName().Version;
            return version?.ToString() ?? "unknown";
        }
    }
}
