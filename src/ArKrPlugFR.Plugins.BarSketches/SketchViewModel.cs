using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>
    /// The state of the tool window: the bars to sketch and the run that makes their sheets.
    /// <para>
    /// The window only shows the state and starts the run; the views, the sheets and the notes
    /// live in the engine, and the run is one undoable step.
    /// </para>
    /// </summary>
    public sealed class SketchViewModel : ObservableObject
    {
        /// <summary>How many notes are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Rebar> _bars = new List<Rebar>();
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">Document the picks happen in.</param>
        /// <param name="document">Project to edit.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public SketchViewModel(
            UIDocument uiDocument,
            Document document,
            SketchText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SketchText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the bars.</summary>
        public string BarsLabel => Text.BarsLabel;

        /// <summary>Gets the caption of the bar pick button.</summary>
        public string PickBarsText => Text.PickBars;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the state of the picked bars.</summary>
        public string BarsState => _bars.Count == 0
            ? Text.BarsNone
            : _bars.Count.ToString(CultureInfo.InvariantCulture) + " ×";

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the bars; the window has to be hidden while it runs.</summary>
        internal void PickBars()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new BarFilter(),
                    Text.PickBarsPrompt);

                _bars.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Rebar bar)
                    {
                        _bars.Add(bar);
                    }
                }

                RaisePropertyChanged(nameof(BarsState));
            }
            catch (OperationCanceledException)
            {
                _log.Info("BarSketches: the pick of bars was cancelled.", nameof(SketchViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Makes the sheets and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_bars.Count == 0)
            {
                SetReport(Text.NoBars);

                return Text.NoBars;
            }

            SketchOutcome outcome = BarSketchesEngine.Run(_document, _bars, Text, _log);

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

        private string BuildReport(SketchOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.ResultLine(outcome.Made, outcome.Bars, outcome.Skipped, outcome.Refused));

                if (outcome.Made == 0)
                {
                    builder.AppendLine(Text.NothingMade);
                }

                foreach (string line in outcome.SheetLines)
                {
                    builder.Append("  ");
                    builder.AppendLine(line);
                }

                if (outcome.Remaining > 0)
                {
                    builder.AppendLine(Text.RemainingLine(outcome.Remaining));
                }
            }

            int shown = 0;

            foreach (string note in outcome.Notes)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append(" - ");
                builder.AppendLine(note);
                shown++;
            }

            return builder.ToString().TrimEnd();
        }

        private sealed class BarFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => element is Rebar;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
