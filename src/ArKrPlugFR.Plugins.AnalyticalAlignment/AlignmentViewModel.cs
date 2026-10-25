using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>
    /// The state of the tool window: the target element, the elements and the nodes to move.
    /// <para>
    /// The window only shows the state and starts the two alignments; the geometry and the
    /// moves live in the engine, and each action is one undoable step of its own.
    /// </para>
    /// </summary>
    public sealed class AlignmentViewModel : ObservableObject
    {
        /// <summary>How many notes are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly List<Element> _members = new List<Element>();
        private readonly List<Element> _nodes = new List<Element>();
        private Element? _target;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">Document the picks happen in.</param>
        /// <param name="document">Project to edit.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public AlignmentViewModel(
            UIDocument uiDocument,
            Document document,
            AlignmentText text,
            Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public AlignmentText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the target.</summary>
        public string TargetLabel => Text.TargetLabel;

        /// <summary>Gets the caption of the button that picks the target.</summary>
        public string PickTargetText => Text.PickTarget;

        /// <summary>Gets the caption of the elements to move.</summary>
        public string MembersLabel => Text.MembersLabel;

        /// <summary>Gets the caption of the button that picks the elements.</summary>
        public string PickMembersText => Text.PickMembers;

        /// <summary>Gets the caption of the nodes to move.</summary>
        public string NodesLabel => Text.NodesLabel;

        /// <summary>Gets the caption of the button that picks the nodes.</summary>
        public string PickNodesText => Text.PickNodes;

        /// <summary>Gets the caption of the member alignment button.</summary>
        public string AlignMembersText => Text.AlignMembers;

        /// <summary>Gets the caption of the node alignment button.</summary>
        public string AlignNodesText => Text.AlignNodes;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the state of the target.</summary>
        public string TargetState =>
            _target == null ? Text.TargetNone : ElementDescription.Describe(_target);

        /// <summary>Gets the state of the elements to move.</summary>
        public string MembersState =>
            _members.Count == 0 ? Text.MembersNone : Count(_members.Count);

        /// <summary>Gets the state of the nodes to move.</summary>
        public string NodesState =>
            _nodes.Count == 0 ? Text.NodesNone : Count(_nodes.Count);

        /// <summary>Gets the result of the last action, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the target; the window has to be hidden while it runs.</summary>
        internal void PickTarget()
        {
            try
            {
                Reference reference = _uiDocument.Selection.PickObject(
                    ObjectType.Element,
                    new AnyElementFilter(),
                    Text.PickTargetPrompt);

                _target = _document.GetElement(reference);

                RaisePropertyChanged(nameof(TargetState));
            }
            catch (OperationCanceledException)
            {
                _log.Info("AnalyticalAlignment: the pick of the target was cancelled.", nameof(AlignmentViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the elements to move.</summary>
        internal void PickMembers()
        {
            PickMany(_members, new CurveElementFilter(), Text.PickMembersPrompt, nameof(MembersState));
        }

        /// <summary>Asks the user for the nodes to move.</summary>
        internal void PickNodes()
        {
            PickMany(_nodes, new NodeFilter(), Text.PickNodesPrompt, nameof(NodesState));
        }

        /// <summary>Aligns the picked elements to the height of the target.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyMembers()
        {
            if (_target == null)
            {
                SetReport(Text.NoTarget);

                return Text.NoTarget;
            }

            if (_members.Count == 0)
            {
                SetReport(Text.NoMembers);

                return Text.NoMembers;
            }

            AlignmentOutcome outcome = AlignmentEngine.AlignMembers(_document, _target, _members, Text, _log);

            string message = BuildReport(
                outcome,
                Text.MembersResultLine(outcome.Moved, outcome.InPlace, outcome.Skipped, outcome.Refused));

            SetReport(message);

            return message;
        }

        /// <summary>Aligns the picked nodes onto the target line.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyNodes()
        {
            if (_target == null)
            {
                SetReport(Text.NoTarget);

                return Text.NoTarget;
            }

            if (_nodes.Count == 0)
            {
                SetReport(Text.NoNodes);

                return Text.NoNodes;
            }

            AlignmentOutcome outcome = AlignmentEngine.AlignNodes(_document, _target, _nodes, Text, _log);

            string message = BuildReport(
                outcome,
                Text.NodesResultLine(outcome.Moved, outcome.InPlace, outcome.Skipped, outcome.Refused, outcome.Dragged));

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
                _log.Info("AnalyticalAlignment: a pick was cancelled.", nameof(AlignmentViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        private string BuildReport(AlignmentOutcome outcome, string resultLine)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(resultLine);

                if (outcome.Moved == 0)
                {
                    builder.AppendLine(Text.NothingMoved);
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

            if (outcome.Notes.Count > shown)
            {
                builder.Append(" - ");
                builder.AppendLine("…");
            }

            return builder.ToString().TrimEnd();
        }

        private static string Count(int value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ×";
        }

        private sealed class AnyElementFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => true;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private sealed class CurveElementFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => element.Location is LocationCurve;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private sealed class NodeFilter : ISelectionFilter
        {
            public bool AllowElement(Element element) => element is ReferencePoint;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
