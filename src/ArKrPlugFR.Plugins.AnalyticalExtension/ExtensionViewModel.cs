using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Analytical;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>
    /// The state of the tool window: the sources to extend, the targets to extend to and the
    /// reach of the search.
    /// <para>
    /// The window only shows the state and starts the run; the geometry and the writes live
    /// in the engine, and the run is one undoable step.
    /// </para>
    /// </summary>
    public sealed class ExtensionViewModel : ObservableObject
    {
        /// <summary>How many notes are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Element> _sources = new List<Element>();
        private readonly List<Element> _targets = new List<Element>();
        private string _reach = ExtensionRules.DefaultReachMillimeters.ToString("0", CultureInfo.InvariantCulture);
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">Document the picks happen in.</param>
        /// <param name="document">Project to edit.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public ExtensionViewModel(
            UIDocument uiDocument,
            Document document,
            ExtensionText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public ExtensionText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the sources.</summary>
        public string SourcesLabel => Text.SourcesLabel;

        /// <summary>Gets the caption of the targets.</summary>
        public string TargetsLabel => Text.TargetsLabel;

        /// <summary>Gets the caption of the source pick button.</summary>
        public string PickSourcesText => Text.PickSources;

        /// <summary>Gets the caption of the target pick button.</summary>
        public string PickTargetsText => Text.PickTargets;

        /// <summary>Gets the caption of the reach field.</summary>
        public string ReachLabel => Text.ReachLabel;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the state of the sources.</summary>
        public string SourcesState => _sources.Count == 0 ? Text.SourcesNone : Count(_sources.Count);

        /// <summary>Gets the state of the targets.</summary>
        public string TargetsState => _targets.Count == 0 ? Text.TargetsNone : Count(_targets.Count);

        /// <summary>Gets or sets the reach of the search in plan, in millimetres.</summary>
        public string Reach
        {
            get => _reach;
            set => SetProperty(ref _reach, value ?? string.Empty);
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the sources; the window has to be hidden while it runs.</summary>
        internal void PickSources()
        {
            PickMany(_sources, new SourceFilter(), Text.PickSourcesPrompt, nameof(SourcesState));
        }

        /// <summary>Asks the user for the targets; the window has to be hidden while it runs.</summary>
        internal void PickTargets()
        {
            PickMany(_targets, new TargetFilter(), Text.PickTargetsPrompt, nameof(TargetsState));
        }

        /// <summary>Extends the sources and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            if (_sources.Count == 0)
            {
                SetReport(Text.NoSources);

                return Text.NoSources;
            }

            if (_targets.Count == 0)
            {
                SetReport(Text.NoTargets);

                return Text.NoTargets;
            }

            if (!double.TryParse(_reach, NumberStyles.Float, CultureInfo.CurrentCulture, out double reach)
                && !double.TryParse(_reach, NumberStyles.Float, CultureInfo.InvariantCulture, out reach))
            {
                reach = ExtensionRules.DefaultReachMillimeters;
            }

            ExtensionOutcome outcome = ExtensionEngine.Run(_document, _sources, _targets, reach, Text, _log);

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

        private void PickMany(List<Element> store, ISelectionFilter filter, string prompt, string stateProperty)
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(ObjectType.Element, filter, prompt);

                store.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Element element)
                    {
                        store.Add(element);
                    }
                }

                RaisePropertyChanged(stateProperty);
            }
            catch (OperationCanceledException)
            {
                _log.Info("AnalyticalExtension: a pick was cancelled.", nameof(ExtensionViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        private string BuildReport(ExtensionOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(Text.ResultLine(
                    outcome.Sources,
                    outcome.Extended,
                    outcome.NoTarget,
                    outcome.NoLine,
                    outcome.Attached,
                    outcome.Refused));

                if (outcome.Extended == 0)
                {
                    builder.AppendLine(Text.NothingExtended);
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

        private static string Count(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + " ×";
        }

        private sealed class SourceFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element is FamilyInstance
                    || element is Wall
                    || AnalyticalCompat.IsAnalyticalElement(element);
            }

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private sealed class TargetFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category == null)
                {
                    return false;
                }

                long category = ArKrPlugFR.Revit.Compatibility.ElementIdCompat.GetValue(element.Category.Id);

                return category == (long)BuiltInCategory.OST_Floors
                    || category == (long)BuiltInCategory.OST_StructuralFraming;
            }

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
