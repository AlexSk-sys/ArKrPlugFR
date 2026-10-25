using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ArKrPlugFR.Core.Compare;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// The state of the tool window: the two elements, their comparison and the copy.
    /// <para>
    /// The window owns only interaction; the picking, the comparison and the copy live
    /// here. The comparison follows the picks and is refreshed after a copy, so the list
    /// always shows the state the model is in.
    /// </para>
    /// </summary>
    public sealed class CompareViewModel : ObservableObject
    {
        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private Element? _source;
        private Element? _target;
        private int _differences;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the elements belong to.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public CompareViewModel(UIDocument uiDocument, Document document, CompareText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public CompareText Text { get; }

        /// <summary>Gets the lines of the comparison.</summary>
        public ObservableCollection<string> Rows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the button that picks the source element.</summary>
        public string PickSourceText => Text.PickSource;

        /// <summary>Gets the caption of the button that picks the target element.</summary>
        public string PickTargetText => Text.PickTarget;

        /// <summary>Gets the caption of the comparison list.</summary>
        public string ComparisonCaption => Text.ComparisonCaption;

        /// <summary>Gets the caption of the copy button.</summary>
        public string CopyText => Text.Copy;

        /// <summary>Gets the hint under the list.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that describes the source element.</summary>
        public string SourceLine => Text.SourceLabel + ": " +
            (_source == null ? Text.NotPicked : ElementDescription.Describe(_source));

        /// <summary>Gets the line that describes the target element.</summary>
        public string TargetLine => Text.TargetLabel + ": " +
            (_target == null ? Text.NotPicked : ElementDescription.Describe(_target));

        /// <summary>Gets the line that counts the rows and the differences.</summary>
        public string ComparisonLine => Text.ComparisonLine(Rows.Count, _differences);

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the source element; the window has to be hidden while it runs.</summary>
        internal void PickSource()
        {
            Element? element = Pick(Text.PickSourcePrompt);

            if (element == null)
            {
                return;
            }

            _source = element;
            Refresh();
        }

        /// <summary>Asks the user for the target element; the window has to be hidden while it runs.</summary>
        internal void PickTarget()
        {
            Element? element = Pick(Text.PickTargetPrompt);

            if (element == null)
            {
                return;
            }

            _target = element;
            Refresh();
        }

        /// <summary>Copies the values of the source into the target and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_source == null || _target == null)
            {
                SetReport(Text.NoPicks);

                return Text.NoPicks;
            }

            CompareOutcome outcome = CompareEngine.Copy(_document, _source, _target, Text, _log);

            Refresh();

            string message = outcome.Different == 0
                ? Text.NothingToCopy
                : BuildReport(outcome);

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

        private Element? Pick(string prompt)
        {
            try
            {
                Reference reference = _uiDocument.Selection.PickObject(ObjectType.Element, prompt);

                return _document.GetElement(reference);
            }
            catch (OperationCanceledException)
            {
                _log.Info("CompareParameters: a pick was cancelled.", nameof(CompareViewModel));

                SetReport(Text.PickCancelled);

                return null;
            }
        }

        private void Refresh()
        {
            Rows.Clear();
            _differences = 0;

            if (_source != null && _target != null)
            {
                IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                    ParameterSupport.ReadValues(_source),
                    ParameterSupport.ReadValues(_target));

                foreach (ParameterRow row in rows)
                {
                    Rows.Add(Format(row));

                    if (row.Differs)
                    {
                        _differences++;
                    }
                }
            }

            RaisePropertyChanged(nameof(SourceLine));
            RaisePropertyChanged(nameof(TargetLine));
            RaisePropertyChanged(nameof(ComparisonLine));
        }

        private static string Format(ParameterRow row)
        {
            return row.Name + ": " + Show(row.Source) + " \u2192 " + Show(row.Target);
        }

        private static string Show(string value)
        {
            return value.Length == 0 ? "\u2014" : value;
        }

        private string BuildReport(CompareOutcome outcome)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Text.Result(
                outcome.Compared,
                outcome.Different,
                outcome.Copied,
                outcome.Skipped,
                outcome.Refused));

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
