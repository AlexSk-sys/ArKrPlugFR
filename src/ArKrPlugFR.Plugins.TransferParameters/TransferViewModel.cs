using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// The state of the tool window: the three transfers, their picks and their reports.
    /// <para>
    /// The window owns only interaction; the picking, the runs and the reports live here.
    /// The sheets are read once when the window opens, the other lists are filled while
    /// the user picks in the model, and every run goes through its own engine.
    /// </para>
    /// </summary>
    public sealed class TransferViewModel : ObservableObject
    {
        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;

        private readonly List<ViewSheet> _sheets = new List<ViewSheet>();
        private readonly List<Element> _structures = new List<Element>();
        private readonly List<Room> _rooms = new List<Room>();

        private Element? _source;
        private string? _selectedSourceParameter;
        private string? _selectedTargetParameter;
        private string? _selectedRoomSourceParameter;
        private string _structureParameter = string.Empty;
        private string _roomTarget = string.Empty;
        private bool _writeToType;
        private bool _receiverWalls = true;
        private bool _receiverFloors = true;
        private bool _receiverCeilings = true;
        private bool _receiverRooms;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the transfers work on.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public TransferViewModel(UIDocument uiDocument, Document document, TransferText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            LoadSheets();
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public TransferText Text { get; }

        /// <summary>Gets the sheets of the project.</summary>
        public ObservableCollection<SheetRow> SheetRows { get; } = new ObservableCollection<SheetRow>();

        /// <summary>Gets the picked walls and floors, described for the list.</summary>
        public ObservableCollection<string> StructureRows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the picked rooms, described for the list.</summary>
        public ObservableCollection<string> RoomRows { get; } = new ObservableCollection<string>();

        /// <summary>Gets the parameter names the source element offers.</summary>
        public ObservableCollection<string> SourceParameterNames { get; } = new ObservableCollection<string>();

        /// <summary>Gets the parameter names the sheets can receive.</summary>
        public ObservableCollection<string> TargetParameterNames { get; } = new ObservableCollection<string>();

        /// <summary>Gets the parameter names the picked rooms offer.</summary>
        public ObservableCollection<string> RoomSourceParameterNames { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the sheets tab.</summary>
        public string TabSheets => Text.TabSheets;

        /// <summary>Gets the caption of the structures tab.</summary>
        public string TabStructures => Text.TabStructures;

        /// <summary>Gets the caption of the rooms tab.</summary>
        public string TabRooms => Text.TabRooms;

        /// <summary>Gets the caption of the button that picks the source element.</summary>
        public string PickSourceText => Text.PickSource;

        /// <summary>Gets the caption of the button that picks walls and floors.</summary>
        public string PickStructuresText => Text.PickStructures;

        /// <summary>Gets the caption of the button that picks rooms.</summary>
        public string PickRoomsText => Text.PickRooms;

        /// <summary>Gets the caption of the source line.</summary>
        public string SourceCaption => Text.Source;

        /// <summary>Gets the caption of the source parameter field.</summary>
        public string SourceParameterCaption => Text.SourceParameter;

        /// <summary>Gets the caption of the value preview.</summary>
        public string ValueCaption => Text.Value;

        /// <summary>Gets the caption of the target parameter field.</summary>
        public string TargetParameterCaption => Text.TargetParameter;

        /// <summary>Gets the caption of the sheet list.</summary>
        public string SheetsCaption => Text.SheetsCaption;

        /// <summary>Gets the caption of the button that ticks every sheet.</summary>
        public string SelectAllText => Text.SelectAll;

        /// <summary>Gets the caption of the button that clears the sheet ticks.</summary>
        public string ClearSelectionText => Text.ClearSelection;

        /// <summary>Gets the hint of the sheets tab.</summary>
        public string SheetsHint => Text.SheetsHint;

        /// <summary>Gets the caption of the picked element list.</summary>
        public string ElementsCaption => Text.ElementsCaption;

        /// <summary>Gets the caption of the parameter name field.</summary>
        public string ParameterNameCaption => Text.ParameterName;

        /// <summary>Gets the caption of the switch that writes to the type.</summary>
        public string WriteToTypeCaption => Text.WriteToType;

        /// <summary>Gets the hint of the structures tab.</summary>
        public string StructuresHint => Text.StructuresHint;

        /// <summary>Gets the caption of the room list.</summary>
        public string RoomsCaption => Text.RoomsCaption;

        /// <summary>Gets the caption of the walls receiver.</summary>
        public string ReceiverWallsText => Text.ReceiverWalls;

        /// <summary>Gets the caption of the floors receiver.</summary>
        public string ReceiverFloorsText => Text.ReceiverFloors;

        /// <summary>Gets the caption of the ceilings receiver.</summary>
        public string ReceiverCeilingsText => Text.ReceiverCeilings;

        /// <summary>Gets the caption of the rooms receiver.</summary>
        public string ReceiverRoomsText => Text.ReceiverRooms;

        /// <summary>Gets the hint of the rooms tab.</summary>
        public string RoomsHint => Text.RoomsHint;

        /// <summary>Gets the caption of the apply buttons.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that describes the source element.</summary>
        public string SourceLine => _source == null ? Text.NoSourcePicked : ElementDescription.Describe(_source);

        /// <summary>Gets or sets the chosen source parameter of the sheets tab.</summary>
        public string? SelectedSourceParameter
        {
            get => _selectedSourceParameter;

            set
            {
                if (SetProperty(ref _selectedSourceParameter, value))
                {
                    RaisePropertyChanged(nameof(SourceValue));
                }
            }
        }

        /// <summary>Gets or sets the chosen target parameter of the sheets tab.</summary>
        public string? SelectedTargetParameter
        {
            get => _selectedTargetParameter;

            set => SetProperty(ref _selectedTargetParameter, value);
        }

        /// <summary>Gets the current value of the source parameter, or an empty string.</summary>
        public string SourceValue
        {
            get
            {
                if (_source == null || !ParameterNameRules.IsUsable(_selectedSourceParameter))
                {
                    return string.Empty;
                }

                return ParameterSupport.Read(_source, _selectedSourceParameter!) ?? string.Empty;
            }
        }

        /// <summary>Gets the line that counts the ticked sheets.</summary>
        public string SheetsLine => Text.SheetsLine(SheetRows.Count(row => row.IsSelected), _sheets.Count);

        /// <summary>Gets or sets the name of the parameter the structures tab writes.</summary>
        public string StructureParameter
        {
            get => _structureParameter;

            set => SetProperty(ref _structureParameter, value ?? string.Empty);
        }

        /// <summary>Gets or sets a value indicating whether the structure text goes to the type.</summary>
        public bool WriteToType
        {
            get => _writeToType;

            set => SetProperty(ref _writeToType, value);
        }

        /// <summary>Gets the line that counts the picked walls and floors.</summary>
        public string StructuresLine => Text.FloorsLine(_structures.Count);

        /// <summary>Gets or sets the chosen source parameter of the rooms tab.</summary>
        public string? SelectedRoomSourceParameter
        {
            get => _selectedRoomSourceParameter;

            set => SetProperty(ref _selectedRoomSourceParameter, value);
        }

        /// <summary>Gets or sets the name of the parameter the rooms tab writes.</summary>
        public string RoomTarget
        {
            get => _roomTarget;

            set => SetProperty(ref _roomTarget, value ?? string.Empty);
        }

        /// <summary>Gets or sets a value indicating whether the boundary walls receive the value.</summary>
        public bool ReceiverWalls
        {
            get => _receiverWalls;

            set => SetProperty(ref _receiverWalls, value);
        }

        /// <summary>Gets or sets a value indicating whether the floor under the room receives the value.</summary>
        public bool ReceiverFloors
        {
            get => _receiverFloors;

            set => SetProperty(ref _receiverFloors, value);
        }

        /// <summary>Gets or sets a value indicating whether the ceiling above the room receives the value.</summary>
        public bool ReceiverCeilings
        {
            get => _receiverCeilings;

            set => SetProperty(ref _receiverCeilings, value);
        }

        /// <summary>Gets or sets a value indicating whether the room itself receives the value.</summary>
        public bool ReceiverRooms
        {
            get => _receiverRooms;

            set => SetProperty(ref _receiverRooms, value);
        }

        /// <summary>Gets the line that counts the picked rooms.</summary>
        public string RoomsLine => Text.RoomsLine(_rooms.Count);

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Asks the user for the source element; the window has to be hidden while it runs.</summary>
        internal void PickSourceElement()
        {
            try
            {
                Reference reference = _uiDocument.Selection.PickObject(ObjectType.Element, Text.PickSourcePrompt);
                Element? element = _document.GetElement(reference);

                if (element == null)
                {
                    return;
                }

                _source = element;
                RefreshSourceParameters();

                RaisePropertyChanged(nameof(SourceLine));
                RaisePropertyChanged(nameof(SourceValue));
            }
            catch (OperationCanceledException)
            {
                _log.Info("TransferParameters: the pick of the source element was cancelled.", nameof(TransferViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the walls and floors; the window has to be hidden while it runs.</summary>
        internal void PickStructures()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new StructuresFilter(),
                    Text.PickStructuresPrompt);

                _structures.Clear();
                StructureRows.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Element element && (element is Wall || element is Floor))
                    {
                        _structures.Add(element);
                        StructureRows.Add(ElementDescription.Describe(element));
                    }
                }

                RaisePropertyChanged(nameof(StructuresLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("TransferParameters: the pick of walls and floors was cancelled.", nameof(TransferViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Asks the user for the rooms; the window has to be hidden while it runs.</summary>
        internal void PickRooms()
        {
            try
            {
                IList<Reference> references = _uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new RoomsFilter(),
                    Text.PickRoomsPrompt);

                _rooms.Clear();
                RoomRows.Clear();

                foreach (Reference reference in references)
                {
                    if (_document.GetElement(reference) is Room room)
                    {
                        _rooms.Add(room);
                        RoomRows.Add(ElementDescription.Describe(room));
                    }
                }

                RefreshRoomSourceParameters();

                RaisePropertyChanged(nameof(RoomsLine));
            }
            catch (OperationCanceledException)
            {
                _log.Info("TransferParameters: the pick of rooms was cancelled.", nameof(TransferViewModel));

                SetReport(Text.PickCancelled);
            }
        }

        /// <summary>Ticks or clears every sheet.</summary>
        /// <param name="selected">Whether every sheet is ticked.</param>
        public void SelectAllSheets(bool selected)
        {
            foreach (SheetRow row in SheetRows)
            {
                row.IsSelected = selected;
            }

            RaisePropertyChanged(nameof(SheetsLine));
        }

        /// <summary>Runs the sheet transfer and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplySheets()
        {
            if (_source == null)
            {
                SetReport(Text.NoSourcePicked);

                return Text.NoSourcePicked;
            }

            if (!ParameterNameRules.IsUsable(_selectedSourceParameter))
            {
                SetReport(Text.SourceRequired);

                return Text.SourceRequired;
            }

            if (!ParameterNameRules.IsUsable(_selectedTargetParameter))
            {
                SetReport(Text.TargetRequired);

                return Text.TargetRequired;
            }

            var ticked = new List<ViewSheet>();

            foreach (SheetRow row in SheetRows)
            {
                if (row.IsSelected)
                {
                    ticked.Add(row.Sheet);
                }
            }

            if (ticked.Count == 0)
            {
                SetReport(Text.NoSheetsTicked);

                return Text.NoSheetsTicked;
            }

            TransferOutcome outcome = SheetTransferEngine.Run(
                _document,
                ticked,
                SourceValue,
                _selectedTargetParameter!,
                Text,
                _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.ResultSheets(outcome.Considered, outcome.Written, outcome.WithoutParameter, outcome.Failed));

            SetReport(message);

            return message;
        }

        /// <summary>Runs the layer write and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyStructures()
        {
            if (_structures.Count == 0)
            {
                SetReport(Text.NoElementsPicked);

                return Text.NoElementsPicked;
            }

            TransferOutcome outcome = LayerTransferEngine.Run(
                _document,
                _structures,
                _structureParameter,
                _writeToType,
                Text.Culture,
                Text,
                _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.ResultElements(outcome.Considered, outcome.Written, outcome.Skipped, outcome.WithoutParameter, outcome.Failed));

            SetReport(message);

            return message;
        }

        /// <summary>Runs the room spread and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string ApplyRooms()
        {
            if (_rooms.Count == 0)
            {
                SetReport(Text.NoRoomsPicked);

                return Text.NoRoomsPicked;
            }

            var options = new RoomSpreadOptions
            {
                Walls = _receiverWalls,
                Floors = _receiverFloors,
                Ceilings = _receiverCeilings,
                Rooms = _receiverRooms
            };

            TransferOutcome outcome = RoomSpreadEngine.Run(
                _document,
                _rooms,
                _selectedRoomSourceParameter ?? string.Empty,
                _roomTarget,
                options,
                Text,
                _log);

            string message = outcome.Stopped != null
                ? BuildReport(outcome, null)
                : BuildReport(outcome, Text.ResultRooms(outcome.Considered, outcome.Written, outcome.Empty, outcome.Skipped, outcome.WithoutParameter, outcome.Failed));

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

        private void LoadSheets()
        {
            foreach (ViewSheet sheet in new FilteredElementCollector(_document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .OrderBy(item => item.SheetNumber, StringComparer.OrdinalIgnoreCase))
            {
                _sheets.Add(sheet);
                SheetRows.Add(new SheetRow(sheet));
            }

            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ViewSheet sheet in _sheets)
            {
                foreach (string name in ParameterSupport.WritableTextNames(sheet))
                {
                    if (seen.Add(name))
                    {
                        names.Add(name);
                    }
                }
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names)
            {
                TargetParameterNames.Add(name);
            }
        }

        private void RefreshSourceParameters()
        {
            SourceParameterNames.Clear();

            if (_source != null)
            {
                foreach (string name in ParameterSupport.ReadableNames(_source))
                {
                    SourceParameterNames.Add(name);
                }
            }

            SelectedSourceParameter = SourceParameterNames.Count > 0 ? SourceParameterNames[0] : null;
        }

        private void RefreshRoomSourceParameters()
        {
            RoomSourceParameterNames.Clear();

            if (_rooms.Count > 0)
            {
                foreach (string name in ParameterSupport.ReadableNames(_rooms[0]))
                {
                    RoomSourceParameterNames.Add(name);
                }
            }

            SelectedRoomSourceParameter = RoomSourceParameterNames.Count > 0 ? RoomSourceParameterNames[0] : null;
        }

        private string BuildReport(TransferOutcome outcome, string? resultLine)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else if (resultLine != null)
            {
                builder.AppendLine(resultLine);
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

        /// <summary>Lets the user pick walls and floors and nothing else.</summary>
        private sealed class StructuresFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Wall || elem is Floor;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        /// <summary>Lets the user pick rooms and nothing else.</summary>
        private sealed class RoomsFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Room;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
