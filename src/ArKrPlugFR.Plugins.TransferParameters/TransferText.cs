namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// <para>
    /// The window, the engines and the reports take this object instead of a culture
    /// code, so a message is written once and every caller reads the same text.
    /// </para>
    /// </summary>
    public sealed class TransferText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public TransferText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the culture the tool reads its own text in.</summary>
        public string Culture => _culture;

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => TransferStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => TransferStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the sheets tab.</summary>
        public string TabSheets => TransferStrings.TabSheets.Resolve(_culture);

        /// <summary>Gets the caption of the structures tab.</summary>
        public string TabStructures => TransferStrings.TabStructures.Resolve(_culture);

        /// <summary>Gets the caption of the rooms tab.</summary>
        public string TabRooms => TransferStrings.TabRooms.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks the source element.</summary>
        public string PickSource => TransferStrings.PickSource.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks walls and floors.</summary>
        public string PickStructures => TransferStrings.PickStructures.Resolve(_culture);

        /// <summary>Gets the caption of the button that picks rooms.</summary>
        public string PickRooms => TransferStrings.PickRooms.Resolve(_culture);

        /// <summary>Gets the prompt shown while the source element is picked.</summary>
        public string PickSourcePrompt => TransferStrings.PickSourcePrompt.Resolve(_culture);

        /// <summary>Gets the prompt shown while walls and floors are picked.</summary>
        public string PickStructuresPrompt => TransferStrings.PickStructuresPrompt.Resolve(_culture);

        /// <summary>Gets the prompt shown while rooms are picked.</summary>
        public string PickRoomsPrompt => TransferStrings.PickRoomsPrompt.Resolve(_culture);

        /// <summary>Gets the message shown when the pick was cancelled.</summary>
        public string PickCancelled => TransferStrings.PickCancelled.Resolve(_culture);

        /// <summary>Gets the caption of the source line.</summary>
        public string Source => TransferStrings.Source.Resolve(_culture);

        /// <summary>Gets the caption of the source parameter field.</summary>
        public string SourceParameter => TransferStrings.SourceParameter.Resolve(_culture);

        /// <summary>Gets the caption of the value preview.</summary>
        public string Value => TransferStrings.Value.Resolve(_culture);

        /// <summary>Gets the caption of the target parameter field.</summary>
        public string TargetParameter => TransferStrings.TargetParameter.Resolve(_culture);

        /// <summary>Gets the caption of the sheet list.</summary>
        public string SheetsCaption => TransferStrings.SheetsCaption.Resolve(_culture);

        /// <summary>Gets the caption of the button that ticks every sheet.</summary>
        public string SelectAll => TransferStrings.SelectAll.Resolve(_culture);

        /// <summary>Gets the caption of the button that clears the sheet ticks.</summary>
        public string ClearSelection => TransferStrings.ClearSelection.Resolve(_culture);

        /// <summary>Gets the hint of the sheets tab.</summary>
        public string SheetsHint => TransferStrings.SheetsHint.Resolve(_culture);

        /// <summary>Gets the caption of the picked element list.</summary>
        public string ElementsCaption => TransferStrings.ElementsCaption.Resolve(_culture);

        /// <summary>Gets the caption of the parameter name field.</summary>
        public string ParameterName => TransferStrings.ParameterName.Resolve(_culture);

        /// <summary>Gets the caption of the switch that writes to the type.</summary>
        public string WriteToType => TransferStrings.WriteToType.Resolve(_culture);

        /// <summary>Gets the hint of the structures tab.</summary>
        public string StructuresHint => TransferStrings.StructuresHint.Resolve(_culture);

        /// <summary>Gets the caption of the room list.</summary>
        public string RoomsCaption => TransferStrings.RoomsCaption.Resolve(_culture);

        /// <summary>Gets the caption of the walls receiver.</summary>
        public string ReceiverWalls => TransferStrings.ReceiverWalls.Resolve(_culture);

        /// <summary>Gets the caption of the floors receiver.</summary>
        public string ReceiverFloors => TransferStrings.ReceiverFloors.Resolve(_culture);

        /// <summary>Gets the caption of the ceilings receiver.</summary>
        public string ReceiverCeilings => TransferStrings.ReceiverCeilings.Resolve(_culture);

        /// <summary>Gets the caption of the rooms receiver.</summary>
        public string ReceiverRooms => TransferStrings.ReceiverRooms.Resolve(_culture);

        /// <summary>Gets the hint of the rooms tab.</summary>
        public string RoomsHint => TransferStrings.RoomsHint.Resolve(_culture);

        /// <summary>Gets the caption of the apply buttons.</summary>
        public string Apply => TransferStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => TransferStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => TransferStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the sheet transfer in the undo list.</summary>
        public string SheetTransactionName => TransferStrings.SheetTransaction.Resolve(_culture);

        /// <summary>Gets the name of the layer write in the undo list.</summary>
        public string LayerTransactionName => TransferStrings.LayerTransaction.Resolve(_culture);

        /// <summary>Gets the name of the room spread in the undo list.</summary>
        public string RoomTransactionName => TransferStrings.RoomTransaction.Resolve(_culture);

        /// <summary>Gets the message shown when the source parameter is empty.</summary>
        public string NoSourceValue => TransferStrings.NoSourceValue.Resolve(_culture);

        /// <summary>Gets the message shown when no source element was picked.</summary>
        public string NoSourcePicked => TransferStrings.NoSourcePicked.Resolve(_culture);

        /// <summary>Gets the message shown when no source parameter was chosen.</summary>
        public string SourceRequired => TransferStrings.SourceRequired.Resolve(_culture);

        /// <summary>Gets the message shown when the target parameter name is empty.</summary>
        public string TargetRequired => TransferStrings.TargetRequired.Resolve(_culture);

        /// <summary>Gets the message shown when no receiver was ticked.</summary>
        public string NothingTicked => TransferStrings.NothingTicked.Resolve(_culture);

        /// <summary>Gets the message shown when no sheet was ticked.</summary>
        public string NoSheetsTicked => TransferStrings.NoSheetsTicked.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was picked for the structures tab.</summary>
        public string NoElementsPicked => TransferStrings.NoElementsPicked.Resolve(_culture);

        /// <summary>Gets the message shown when no room was picked.</summary>
        public string NoRoomsPicked => TransferStrings.NoRoomsPicked.Resolve(_culture);

        /// <summary>Gets the line of an element without a compound structure.</summary>
        /// <param name="element">Description of the element.</param>
        public string NoLayers(string element) => TransferStrings.NoLayers.Resolve(_culture, element);

        /// <summary>Gets the line of an element whose structure holds no layers.</summary>
        /// <param name="element">Description of the element.</param>
        public string EmptyLayers(string element) => TransferStrings.EmptyLayers.Resolve(_culture, element);

        /// <summary>Gets the line of an element without the target parameter.</summary>
        /// <param name="element">Description of the element.</param>
        public string NoTargetParameter(string element) => TransferStrings.NoTargetParameter.Resolve(_culture, element);

        /// <summary>Gets the line of a refused write.</summary>
        /// <param name="element">Description of the element.</param>
        /// <param name="reason">What the platform reported.</param>
        public string WriteFailed(string element, string reason) => TransferStrings.WriteFailed.Resolve(_culture, element, reason);

        /// <summary>Gets the line of a room that is not placed.</summary>
        /// <param name="room">Description of the room.</param>
        public string UnplacedRoom(string room) => TransferStrings.UnplacedRoom.Resolve(_culture, room);

        /// <summary>Gets the line of a room whose floor and ceiling were not found.</summary>
        /// <param name="room">Description of the room.</param>
        public string NoFloorOrCeiling(string room) => TransferStrings.NoFloorOrCeiling.Resolve(_culture, room);

        /// <summary>Gets the line that counts the picked walls and floors.</summary>
        /// <param name="elements">Elements picked.</param>
        public string FloorsLine(int elements) => TransferStrings.FloorsLine.Resolve(_culture, elements);

        /// <summary>Gets the line that counts the picked rooms.</summary>
        /// <param name="rooms">Rooms picked.</param>
        public string RoomsLine(int rooms) => TransferStrings.RoomsLine.Resolve(_culture, rooms);

        /// <summary>Gets the line that counts the ticked sheets.</summary>
        /// <param name="ticked">Sheets ticked.</param>
        /// <param name="all">Sheets in the project.</param>
        public string SheetsLine(int ticked, int all) => TransferStrings.SheetsLine.Resolve(_culture, ticked, all);

        /// <summary>Gets the result line of the sheet transfer.</summary>
        /// <param name="considered">Sheets read.</param>
        /// <param name="written">Sheets written.</param>
        /// <param name="withoutParameter">Sheets without the target parameter.</param>
        /// <param name="failed">Refusals.</param>
        public string ResultSheets(int considered, int written, int withoutParameter, int failed) =>
            TransferStrings.ResultSheets.Resolve(_culture, considered, written, withoutParameter, failed);

        /// <summary>Gets the result line of the layer write.</summary>
        /// <param name="considered">Elements read.</param>
        /// <param name="written">Elements written.</param>
        /// <param name="skipped">Elements skipped.</param>
        /// <param name="withoutParameter">Elements without the target parameter.</param>
        /// <param name="failed">Refusals.</param>
        public string ResultElements(int considered, int written, int skipped, int withoutParameter, int failed) =>
            TransferStrings.ResultElements.Resolve(_culture, considered, written, skipped, withoutParameter, failed);

        /// <summary>Gets the result line of the room spread.</summary>
        /// <param name="considered">Rooms read.</param>
        /// <param name="written">Values written.</param>
        /// <param name="empty">Rooms with an empty value.</param>
        /// <param name="skipped">Rooms skipped.</param>
        /// <param name="withoutParameter">Receivers without the target parameter.</param>
        /// <param name="failed">Refusals.</param>
        public string ResultRooms(int considered, int written, int empty, int skipped, int withoutParameter, int failed) =>
            TransferStrings.ResultRooms.Resolve(_culture, considered, written, empty, skipped, withoutParameter, failed);

        /// <summary>Gets the line that counts the report lines left out.</summary>
        /// <param name="count">How many lines were left out.</param>
        public string MoreLine(int count) => TransferStrings.MoreLine.Resolve(_culture, count);
    }
}
