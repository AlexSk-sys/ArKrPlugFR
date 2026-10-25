using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class TransferStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Transfer parameters", "Перенос параметров");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Copies a value onto sheets, into a layer text or around a room",
                "Copies a value onto sheets, into a layer text or around a room");

        // Tabs.
        internal static readonly LocalizedText TabSheets =
            new LocalizedText("To sheets", "На листы");

        internal static readonly LocalizedText TabStructures =
            new LocalizedText("Structures", "Структуры");

        internal static readonly LocalizedText TabRooms =
            new LocalizedText("Rooms", "Помещения");

        // Pick buttons and prompts.
        internal static readonly LocalizedText PickSource =
            new LocalizedText("Pick an element", "Выбрать элемент");

        internal static readonly LocalizedText PickStructures =
            new LocalizedText("Pick walls and floors", "Выбрать стены и перекрытия");

        internal static readonly LocalizedText PickRooms =
            new LocalizedText("Pick rooms", "Выбрать помещения");

        internal static readonly LocalizedText PickSourcePrompt =
            new LocalizedText("Pick the element the value comes from", "Выберите элемент — источник значения");

        internal static readonly LocalizedText PickStructuresPrompt =
            new LocalizedText("Pick the walls and floors to fill", "Выберите стены и перекрытия для заполнения");

        internal static readonly LocalizedText PickRoomsPrompt =
            new LocalizedText("Pick the rooms to spread", "Выберите помещения для переноса");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        // Sheets tab.
        internal static readonly LocalizedText Source =
            new LocalizedText("Source", "Источник");

        internal static readonly LocalizedText SourceParameter =
            new LocalizedText("Source parameter", "Параметр-источник");

        internal static readonly LocalizedText Value =
            new LocalizedText("Value", "Значение");

        internal static readonly LocalizedText TargetParameter =
            new LocalizedText("Target parameter", "Параметр-приёмник");

        internal static readonly LocalizedText SheetsCaption =
            new LocalizedText("Sheets that receive the value", "Листы, которым переносится значение");

        internal static readonly LocalizedText SelectAll =
            new LocalizedText("Select all", "Выбрать все");

        internal static readonly LocalizedText ClearSelection =
            new LocalizedText("Clear", "Снять выбор");

        internal static readonly LocalizedText SheetsHint =
            new LocalizedText(
                "The value is read from the source element and written into the chosen parameter of every ticked sheet. An empty value stops the run; sheets without the parameter are reported.",
                "Значение читается с элемента-источника и записывается в выбранный параметр каждого отмеченного листа. Пустое значение останавливает перенос; листы без параметра попадут в отчёт.");

        // Structures tab.
        internal static readonly LocalizedText ElementsCaption =
            new LocalizedText("Walls and floors that receive the text", "Стены и перекрытия, которым пишется текст");

        internal static readonly LocalizedText ParameterName =
            new LocalizedText("Parameter name", "Имя параметра");

        internal static readonly LocalizedText WriteToType =
            new LocalizedText("Write to the type instead of the instance", "Записать в тип вместо экземпляра");

        internal static readonly LocalizedText StructuresHint =
            new LocalizedText(
                "The layer structure of the wall or floor type is written as text, one layer per line: 1. Brick — 250 mm. Only a text parameter can receive it; the instance parameter is written first, the type parameter second.",
                "Слоистая конструкция типа стены или перекрытия записывается текстом, по строке на слой: 1. Кирпич — 250 мм. Принимает только текстовый параметр; сначала пишется параметр экземпляра, затем параметр типа.");

        // Rooms tab.
        internal static readonly LocalizedText RoomsCaption =
            new LocalizedText("Rooms that spread their value", "Помещения, раздающие своё значение");

        internal static readonly LocalizedText ReceiverWalls =
            new LocalizedText("Walls", "Стены");

        internal static readonly LocalizedText ReceiverFloors =
            new LocalizedText("Floors", "Перекрытия");

        internal static readonly LocalizedText ReceiverCeilings =
            new LocalizedText("Ceilings", "Потолки");

        internal static readonly LocalizedText ReceiverRooms =
            new LocalizedText("The rooms themselves", "Сами помещения");

        internal static readonly LocalizedText RoomsHint =
            new LocalizedText(
                "The value of the room is written onto its boundary walls, the floor under it and the ceiling above it — the floor of the storey over when the model has no ceilings. Only a text parameter can receive it.",
                "Значение помещения записывается на его граничные стены, перекрытие под ним и потолок над ним — при отсутствии потолков берётся перекрытие сверху. Принимает только текстовый параметр.");

        // Buttons.
        internal static readonly LocalizedText Apply =
            new LocalizedText("Apply", "Применить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        // Transactions.
        internal static readonly LocalizedText SheetTransaction =
            new LocalizedText("Transfer a value to sheets", "Перенос значения на листы");

        internal static readonly LocalizedText LayerTransaction =
            new LocalizedText("Write the structure into a parameter", "Запись конструкции в параметр");

        internal static readonly LocalizedText RoomTransaction =
            new LocalizedText("Spread a room value", "Перенос значения помещения");

        // Stops.
        internal static readonly LocalizedText NoSourceValue =
            new LocalizedText(
                "Nothing to transfer: the source parameter is empty.",
                "Переносить нечего: параметр-источник пуст.");

        internal static readonly LocalizedText NoSourcePicked =
            new LocalizedText(
                "Pick the element the value comes from.",
                "Выберите элемент — источник значения.");

        internal static readonly LocalizedText SourceRequired =
            new LocalizedText("Choose the source parameter.", "Выберите параметр-источник.");

        internal static readonly LocalizedText TargetRequired =
            new LocalizedText(
                "Enter the name of the parameter to write.",
                "Введите имя параметра для записи.");

        internal static readonly LocalizedText NothingTicked =
            new LocalizedText(
                "Choose at least one receiver: walls, floors, ceilings or the rooms.",
                "Выберите хотя бы одного приёмника: стены, перекрытия, потолки или сами помещения.");

        internal static readonly LocalizedText NoSheetsTicked =
            new LocalizedText("Tick at least one sheet.", "Отметьте хотя бы один лист.");

        internal static readonly LocalizedText NoElementsPicked =
            new LocalizedText("Pick the walls and floors first.", "Сначала выберите стены и перекрытия.");

        internal static readonly LocalizedText NoRoomsPicked =
            new LocalizedText("Pick the rooms first.", "Сначала выберите помещения.");

        // Lines about what was left out.
        internal static readonly LocalizedText NoLayers =
            new LocalizedText(
                "{0}: no compound structure; skipped.",
                "{0}: нет слоистой конструкции; пропущен.");

        internal static readonly LocalizedText EmptyLayers =
            new LocalizedText(
                "{0}: the structure has no layers; skipped.",
                "{0}: в конструкции нет слоёв; пропущен.");

        internal static readonly LocalizedText NoTargetParameter =
            new LocalizedText(
                "{0}: no writable text parameter with this name.",
                "{0}: нет записываемого текстового параметра с таким именем.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText UnplacedRoom =
            new LocalizedText(
                "{0}: the room is not placed; skipped.",
                "{0}: помещение не размещено; пропущено.");

        internal static readonly LocalizedText NoFloorOrCeiling =
            new LocalizedText(
                "{0}: neither the floor nor the ceiling of the room was found.",
                "{0}: ни перекрытие, ни потолок помещения не найдены.");

        // Result lines and counters.
        internal static readonly LocalizedText FloorsLine =
            new LocalizedText("Walls and floors: {0}", "Стен и перекрытий: {0}");

        internal static readonly LocalizedText RoomsLine =
            new LocalizedText("Rooms: {0}", "Помещений: {0}");

        internal static readonly LocalizedText SheetsLine =
            new LocalizedText("Sheets: {0} of {1} ticked", "Листов отмечено: {0} из {1}");

        internal static readonly LocalizedText ResultSheets =
            new LocalizedText(
                "Sheets read: {0}; written: {1}; without the parameter: {2}; refused: {3}.",
                "Листов прочитано: {0}; записано: {1}; без параметра: {2}; отказов: {3}.");

        internal static readonly LocalizedText ResultElements =
            new LocalizedText(
                "Elements read: {0}; written: {1}; skipped: {2}; without the parameter: {3}; refused: {4}.",
                "Прочитано элементов: {0}; записано: {1}; пропущено: {2}; без параметра: {3}; отказов: {4}.");

        internal static readonly LocalizedText ResultRooms =
            new LocalizedText(
                "Rooms read: {0}; written onto: {1}; empty values: {2}; rooms skipped: {3}; receivers without the parameter: {4}; refused: {5}.",
                "Прочитано помещений: {0}; записано: {1}; пустых значений: {2}; пропущено помещений: {3}; приёмников без параметра: {4}; отказов: {5}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
