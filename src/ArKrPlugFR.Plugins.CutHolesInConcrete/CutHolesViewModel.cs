using System;
using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// The state of the options window: the clearance, the link the user picks and the
    /// run they start.
    /// <para>
    /// The clearance is read from the user's settings folder when the window opens and
    /// written back when a run starts, so the next run starts where this one left off.
    /// Picking the link hides the window: the user needs the model in front of them.
    /// </para>
    /// </summary>
    public sealed class CutHolesViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "cut-holes-in-concrete";

        /// <summary>Highest clearance the field accepts, in millimetres.</summary>
        internal const int MaximumClearanceMillimeters = 500;

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private string _clearanceText;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in, for picking the link.</param>
        /// <param name="document">Project the openings are cut in.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public CutHolesViewModel(
            UIDocument uiDocument,
            Document document,
            CutHolesText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            CutHolesSettings settings = ToolSettings.Load(
                AppPaths.SettingsDirectory,
                SettingsName,
                new CutHolesSettings());

            _clearanceText = settings.ClearanceMillimeters.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public CutHolesText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the hint that explains where the equipment comes from.</summary>
        public string LinkLine => Text.LinkLine;

        /// <summary>Gets the caption of the clearance field.</summary>
        public string ClearanceLabel => Text.ClearanceLabel;

        /// <summary>Gets the hint under the clearance field.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the apply button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the clearance of the next run, in millimetres.</summary>
        internal Length Clearance { get; private set; } = Length.FromMillimeters(50d);

        /// <summary>Gets or sets the clearance as typed in the window.</summary>
        public string ClearanceText
        {
            get => _clearanceText;

            set => SetProperty(ref _clearanceText, value ?? string.Empty);
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>
        /// Reads the clearance from the field and stores it for the next session.
        /// </summary>
        /// <param name="error">The message to show when the field cannot be read.</param>
        /// <returns><c>true</c> when the field holds a usable clearance.</returns>
        internal bool TryReadClearance(out string error)
        {
            error = string.Empty;

            if (int.TryParse(
                    _clearanceText.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int value)
                && value >= 0
                && value <= MaximumClearanceMillimeters)
            {
                Clearance = Length.FromMillimeters(value);

                ToolSettings.Save(
                    AppPaths.SettingsDirectory,
                    SettingsName,
                    new CutHolesSettings { ClearanceMillimeters = value });

                return true;
            }

            error = Text.ClearanceRequired;

            return false;
        }

        /// <summary>
        /// Asks the user for a linked model. The window has to be hidden while the pick
        /// runs, so the model is in front of the user.
        /// </summary>
        /// <returns>The picked link, or <c>null</c> when the pick was cancelled.</returns>
        internal RevitLinkInstance? PickLink()
        {
            try
            {
                Reference reference = _uiDocument.Selection.PickObject(
                    ObjectType.Element,
                    new LinkSelectionFilter(),
                    Text.PickPrompt);

                return _document.GetElement(reference) as RevitLinkInstance;
            }
            catch (OperationCanceledException)
            {
                _log.Info("CutHolesInConcrete: the pick of a link was cancelled.", nameof(CutHolesViewModel));

                SetReport(Text.PickCancelled);

                return null;
            }
        }

        /// <summary>Cuts the openings of the picked link and returns the report of the run.</summary>
        /// <param name="link">The linked model the equipment comes from.</param>
        /// <returns>The message to show the user.</returns>
        public string Apply(RevitLinkInstance link)
        {
            ConcreteOutcome outcome = ConcreteCutter.Cut(_document, link, Clearance, Text, _log);

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

        private string BuildReport(ConcreteOutcome outcome)
        {
            if (outcome.Stopped != null)
            {
                return outcome.Stopped;
            }

            var builder = new StringBuilder();

            builder.AppendLine(Text.Counts(outcome.Equipment, outcome.Hosts));
            builder.AppendLine(Text.Result(
                outcome.Cut,
                outcome.AlreadyThere,
                outcome.NotThrough,
                outcome.TooSmall,
                outcome.Failed));

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

        /// <summary>Lets the user pick anything, the pick is read as a link afterwards.</summary>
        private sealed class LinkSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is RevitLinkInstance;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
