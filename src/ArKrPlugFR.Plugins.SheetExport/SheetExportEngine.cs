using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>What one export did.</summary>
    internal sealed class ExportOutcome
    {
        internal ExportOutcome(
            string? stopped,
            string folder,
            int sheets,
            IReadOnlyList<string> fileNames,
            IReadOnlyList<string> duplicates,
            string? failure)
        {
            Stopped = stopped;
            Folder = folder;
            Sheets = sheets;
            FileNames = fileNames ?? Array.Empty<string>();
            Duplicates = duplicates ?? Array.Empty<string>();
            Failure = failure;
        }

        internal string? Stopped { get; }

        internal string Folder { get; }

        internal int Sheets { get; }

        internal IReadOnlyList<string> FileNames { get; }

        internal IReadOnlyList<string> Duplicates { get; }

        internal string? Failure { get; }
    }

    /// <summary>
    /// Exports the ticked sheets as one PDF file per sheet.
    /// <para>
    /// The file names follow <see cref="SheetFileNames"/>, which is tested without Revit and
    /// is also the pattern the platform is asked to use, so the report names the files the
    /// export really produces. Two sheets that would land on one file are reported before the
    /// export runs. Nothing in the document changes: the files are written outside the
    /// project.
    /// </para>
    /// </summary>
    internal static class SheetExportEngine
    {
        /// <summary>Lists the sheets of the project, ordered by number.</summary>
        /// <param name="document">The project to read.</param>
        /// <returns>The sheets that can be exported.</returns>
        internal static IReadOnlyList<ViewSheet> Sheets(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(sheet => !sheet.IsPlaceholder)
                .OrderBy(sheet => sheet.SheetNumber, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>Exports the given sheets into the folder.</summary>
        /// <param name="document">The project to export from.</param>
        /// <param name="sheets">The sheets to export.</param>
        /// <param name="folder">Folder the files are written into.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the export did.</returns>
        internal static ExportOutcome Run(
            Document document,
            IReadOnlyList<ViewSheet> sheets,
            string folder,
            SheetExportText text,
            Log log)
        {
            if (!SheetExportCompat.IsSupported)
            {
                return new ExportOutcome(text.NotSupported, folder, 0, Array.Empty<string>(), Array.Empty<string>(), null);
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                return new ExportOutcome(text.NoFolder, folder, 0, Array.Empty<string>(), Array.Empty<string>(), null);
            }

            if (!Directory.Exists(folder))
            {
                return new ExportOutcome(text.FolderMissing(folder), folder, 0, Array.Empty<string>(), Array.Empty<string>(), null);
            }

            var names = sheets
                .Select(sheet => SheetFileNames.ForSheet(sheet.SheetNumber, sheet.Name))
                .ToList();

            IReadOnlyList<string> duplicates = SheetFileNames.Duplicates(names);

            try
            {
                SheetExportCompat.ExportPdf(document, folder, sheets.Select(sheet => sheet.Id).ToList());

                log.Info(
                    "SheetExport: exported " + sheets.Count + " sheet(s) into «" + folder + "».",
                    nameof(SheetExportEngine));

                return new ExportOutcome(null, folder, sheets.Count, names, duplicates, null);
            }
            catch (Exception exception)
            {
                log.Error(
                    "SheetExport: the export failed: " + exception.Message,
                    nameof(SheetExportEngine),
                    exception);

                return new ExportOutcome(null, folder, sheets.Count, names, duplicates, exception.Message);
            }
        }
    }
}
