using System;
using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// The state of the tool window: the scope, the file and the run.
    /// <para>
    /// The scope and the last file are read from the user's settings folder when the window
    /// opens and written back whenever they change. The file is chosen in the save dialog
    /// of the system.
    /// </para>
    /// </summary>
    public sealed class ConcreteExportViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "concrete-export";

        /// <summary>The file name offered when none was chosen yet.</summary>
        internal const string DefaultFileName = "concrete-elements.csv";

        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly ExportSettings _settings;
        private bool _onlySelection;
        private string _filePath;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project to read.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public ConcreteExportViewModel(UIDocument uiDocument, Document document, ConcreteExportText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new ExportSettings());
            _onlySelection = _settings.OnlySelection;
            _filePath = _settings.FilePath;
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public ConcreteExportText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the switch that narrows the scope to the selection.</summary>
        public string OnlySelectionText => Text.OnlySelection;

        /// <summary>Gets the caption of the button that chooses the file.</summary>
        public string ChooseFileText => Text.ChooseFile;

        /// <summary>Gets the hint under the settings.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ExportText => Text.Export;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that shows the chosen file.</summary>
        public string FileLine => Text.FileLabel + ": " +
            (string.IsNullOrWhiteSpace(_filePath) ? Text.NotChosen : _filePath);

        /// <summary>Gets or sets a value indicating whether only the selection is exported.</summary>
        public bool OnlySelection
        {
            get => _onlySelection;

            set
            {
                if (SetProperty(ref _onlySelection, value))
                {
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets or sets the chosen file.</summary>
        public string FilePath
        {
            get => _filePath;

            set
            {
                if (SetProperty(ref _filePath, value ?? string.Empty))
                {
                    RaisePropertyChanged(nameof(FileLine));
                    SaveSettings();
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the file with the save dialog of the system.</summary>
        internal void ChooseFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = DefaultFileName,
                DefaultExt = ".csv",
                Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                try
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(_filePath);
                    dialog.FileName = System.IO.Path.GetFileName(_filePath);
                }
                catch (Exception)
                {
                    // A path the system cannot split is no reason to fail the dialog.
                }
            }

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

        /// <summary>Exports the concrete elements and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            List<Element> selection = CurrentSelection();

            ConcreteExportOutcome outcome = ConcreteExportEngine.Run(
                _document,
                selection,
                _onlySelection,
                _filePath,
                Text,
                _log);

            string message = outcome.Stopped ?? BuildReport(outcome);
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

        private List<Element> CurrentSelection()
        {
            var selection = new List<Element>();

            try
            {
                foreach (ElementId id in _uiDocument.Selection.GetElementIds())
                {
                    Element? element = _document.GetElement(id);

                    if (element != null)
                    {
                        selection.Add(element);
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warn("ConcreteExport: the selection could not be read: " + exception.Message, nameof(ConcreteExportViewModel));
            }

            return selection;
        }

        private void SaveSettings()
        {
            _settings.OnlySelection = _onlySelection;
            _settings.FilePath = _filePath;

            ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
        }

        private string BuildReport(ConcreteExportOutcome outcome)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Text.Result(
                outcome.Scanned,
                outcome.Exported,
                outcome.NotConcrete,
                outcome.Failed,
                outcome.FilePath));

            int shown = 0;

            foreach (string failure in outcome.Failures)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(failure);
                shown++;
            }

            if (outcome.Failures.Count > shown)
            {
                builder.AppendLine(Text.MoreLine(outcome.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
