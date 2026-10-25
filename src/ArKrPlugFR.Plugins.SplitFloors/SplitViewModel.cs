using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.SplitFloors
{
    /// <summary>
    /// The state of the tool window: the picked floors and the run they start.
    /// <para>
    /// The window owns only interaction; the picking, the split and the report live here.
    /// The floors are picked in the model while the window is hidden, and the list is
    /// dropped once a run has replaced the floors it held.
    /// </para>
    /// </summary>
    public sealed class SplitViewModel : ObservableObject
    {
        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Floor> _floors = new List<Floor>();
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the floors belong to.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public SplitViewModel(UIDocument uiDocument, Document document, SplitText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SplitText Text { get; }

        /// <summary>Gets the picked floors, described for the list.</summary>
        public ObservableCollection<string> PickedFloors { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the picked floor list.</summary>
        public string FloorsCaption => Text.FloorsCaption;

        /// <summary>Gets the caption of the button that picks floors.</summary>
        public string PickFloorsText => Text.PickFloors;

        /// <summary>Gets the caption of the button that clears the list.</summary>
        public string ClearText => Text.Clear;

        /// <summary>Gets the hint under the list.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the picked floors.</summary>
        public string FloorsLine => Text.FloorsLine(_floors.Count);

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the floors; the window has to be hidden while it runs.</summary>
        internal void PickFloors()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new FloorSelectionFilter(),
                    Text.PickPrompt);

                _floors.Clear();
                PickedFloors.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Floor floor)
                    {
                        _floors.Add(floor);
                        PickedFloors.Add(ElementDescription.Describe(floor));
                    }
                }

                RaisePropertyChanged(nameof(FloorsLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("SplitFloors: the pick of floors was cancelled.", nameof(SplitViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Drops the picked floors.</summary>
        public void Clear()
        {
            _floors.Clear();
            PickedFloors.Clear();

            RaisePropertyChanged(nameof(FloorsLine));
        }

        /// <summary>Splits the picked floors and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_floors.Count == 0)
            {
                SetReport(Text.NoFloors);

                return Text.NoFloors;
            }

            FloorSplitOutcome outcome = FloorSplitEngine.Split(_document, _floors, Text, _log);

            // The run has replaced the floors it held, so the list is dropped rather than
            // kept pointing at elements that no longer exist.
            Clear();

            string message = BuildReport(outcome);
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

        private string BuildReport(FloorSplitOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.Result(
                    outcome.Considered,
                    outcome.Split,
                    outcome.Created,
                    outcome.Skipped,
                    outcome.Failed));
            }

            int shown = 0;

            foreach (string note in outcome.Notes)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(note);
                shown++;
            }

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

            int total = outcome.Notes.Count + outcome.Failures.Count;

            if (total > shown)
            {
                builder.AppendLine(Text.MoreLine(total - shown));
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>Lets the user pick floors and nothing else.</summary>
        private sealed class FloorSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Floor;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
