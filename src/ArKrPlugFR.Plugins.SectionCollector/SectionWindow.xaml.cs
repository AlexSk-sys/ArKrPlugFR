using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ArKrPlugFR.Core.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SectionCollector
{
    /// <summary>
    /// The report window of the tool.
    /// <para>
    /// The window owns the report text, the selection of the collected elements and the
    /// clipboard; the collection itself ran before the window opened.
    /// </para>
    /// </summary>
    public partial class SectionWindow : Window, INotifyPropertyChanged
    {
        private readonly UIDocument _uiDocument;
        private readonly IReadOnlyList<ElementId> _ids;
        private readonly SectionText _text;
        private readonly Log _log;
        private string _status = string.Empty;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="outcome">What the run collected.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        internal SectionWindow(UIDocument uiDocument, SectionOutcome outcome, SectionText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _ids = outcome.ElementIds;
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            Report = outcome.Stopped ?? outcome.Report;

            InitializeComponent();

            DataContext = this;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => _text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => _text.Subtitle;

        /// <summary>Gets the report text.</summary>
        public string Report { get; }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SectionText Text => _text;

        /// <summary>Gets the status line shown next to the buttons.</summary>
        public string Status => _status;

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _uiDocument.Selection.SetElementIds(new List<ElementId>(_ids));
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "SectionCollector: the elements could not be selected: " + exception.Message,
                    nameof(SectionWindow),
                    exception);

                SetStatus(_text.SelectFailed(exception.Message));
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Report);
                SetStatus(_text.Copied);
            }
            catch (Exception exception)
            {
                _log.Warn(
                    "SectionCollector: the report could not be copied: " + exception.Message,
                    nameof(SectionWindow),
                    exception);

                SetStatus(_text.SelectFailed(exception.Message));
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetStatus(string status)
        {
            _status = status;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }
}
