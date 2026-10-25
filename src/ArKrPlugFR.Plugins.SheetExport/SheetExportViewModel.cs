using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>One sheet in the list, with its tick.</summary>
    public sealed class SheetRow : ObservableObject
    {
        private bool _isTicked;

        /// <summary>Creates a row.</summary>
        /// <param name="sheet">The sheet the row stands for.</param>
        internal SheetRow(ViewSheet sheet)
        {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        }

        /// <summary>Gets the sheet.</summary>
        internal ViewSheet Sheet { get; }

        /// <summary>Gets the number of the sheet.</summary>
        public string Number => Sheet.SheetNumber;

        /// <summary>Gets the name of the sheet.</summary>
        public string Name => Sheet.Name;

        /// <summary>Gets or sets a value indicating whether the sheet is exported.</summary>
        public bool IsTicked
        {
            get => _isTicked;
            set => SetProperty(ref _isTicked, value);
        }
    }

    /// <summary>
    /// The state of the tool window: the sheets of the project with their ticks and the folder
    /// the files go into.
    /// <para>
    /// The window only shows the state; the export itself lives in the engine and writes
    /// files outside the project.
    /// </para>
    /// </summary>
    public sealed class SheetExportViewModel : ObservableObject
    {
        private readonly Document _document;
        private readonly Log _log;
        private string _folder;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project the sheets live in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public SheetExportViewModel(Document document, SheetExportText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            foreach (ViewSheet sheet in SheetExportEngine.Sheets(_document))
            {
                Sheets.Add(new SheetRow(sheet));
            }

            _folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SheetExportText Text { get; }

        /// <summary>Gets the rows of the sheet list.</summary>
        public ObservableCollection<SheetRow> Sheets { get; } = new ObservableCollection<SheetRow>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the sheet list.</summary>
        public string SheetsLabel => Text.SheetsLabel;

        /// <summary>Gets the caption of the button that ticks every sheet.</summary>
        public string TickAllText => Text.TickAll;

        /// <summary>Gets the caption of the button that untickes every sheet.</summary>
        public string UntickAllText => Text.UntickAll;

        /// <summary>Gets the caption of the folder field.</summary>
        public string FolderLabel => Text.FolderLabel;

        /// <summary>Gets the caption of the browse button.</summary>
        public string BrowseText => Text.Browse;

        /// <summary>Gets the caption of the export button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets the folder the files are written into.</summary>
        public string Folder
        {
            get => _folder;
            set => SetProperty(ref _folder, value ?? string.Empty);
        }

        /// <summary>Gets the result of the last export, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Ticks every sheet.</summary>
        public void TickAll()
        {
            foreach (SheetRow row in Sheets)
            {
                row.IsTicked = true;
            }
        }

        /// <summary>Untickes every sheet.</summary>
        public void UntickAll()
        {
            foreach (SheetRow row in Sheets)
            {
                row.IsTicked = false;
            }
        }

        /// <summary>Exports the ticked sheets and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (Sheets.Count == 0)
            {
                SetReport(Text.NoSheets);

                return Text.NoSheets;
            }

            var ticked = new List<ViewSheet>();

            foreach (SheetRow row in Sheets)
            {
                if (row.IsTicked)
                {
                    ticked.Add(row.Sheet);
                }
            }

            if (ticked.Count == 0)
            {
                SetReport(Text.NoTicked);

                return Text.NoTicked;
            }

            ExportOutcome outcome = SheetExportEngine.Run(_document, ticked, _folder, Text, _log);

            string message = BuildReport(outcome, ticked);
            SetReport(message);

            return message;
        }

        /// <summary>Shows the given text in the report area of the window.</summary>
        /// <param name="report">Text to show.</param>
        public void SetReport(string report)
        {
            _report = report ?? string.Empty;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private string BuildReport(ExportOutcome outcome, IReadOnlyList<ViewSheet> ticked)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);

                return builder.ToString().TrimEnd();
            }

            if (outcome.Failure != null)
            {
                builder.AppendLine(Text.Failed(outcome.Failure));

                return builder.ToString().TrimEnd();
            }

            builder.AppendLine(Text.ResultLine(outcome.Sheets, outcome.Folder));

            for (int index = 0; index < ticked.Count && index < outcome.FileNames.Count; index++)
            {
                string sheet = ticked[index].SheetNumber + " " + ticked[index].Name;

                builder.Append("  ");
                builder.AppendLine(Text.FileLine(sheet.Trim(), outcome.FileNames[index]));
            }

            foreach (string duplicate in outcome.Duplicates)
            {
                builder.AppendLine(Text.Duplicate(duplicate));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
