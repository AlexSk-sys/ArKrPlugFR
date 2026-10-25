using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class PileStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Pile data for schedules", "Данные свай для ведомостей");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Numbers the piles and writes the elevations and the cut heights",
                "Numbers the piles and writes the elevations and the cut heights");

        internal static readonly LocalizedText PickPiles =
            new LocalizedText("Pick piles", "Выбрать сваи");

        internal static readonly LocalizedText PickSlabs =
            new LocalizedText("Pick raft slabs", "Выбрать плиты ростверка");

        internal static readonly LocalizedText PickPilesPrompt =
            new LocalizedText("Pick the piles to work on", "Выберите сваи для обработки");

        internal static readonly LocalizedText PickSlabsPrompt =
            new LocalizedText("Pick the slabs the piles run into", "Выберите плиты, в которые входят сваи");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText PilesCaption =
            new LocalizedText("Picked piles", "Выбранные сваи");

        internal static readonly LocalizedText SlabsCaption =
            new LocalizedText("Picked slabs", "Выбранные плиты");

        internal static readonly LocalizedText PilesLine =
            new LocalizedText("Piles: {0}", "Свай: {0}");

        internal static readonly LocalizedText SlabsLine =
            new LocalizedText("Slabs: {0}", "Плит: {0}");

        internal static readonly LocalizedText StartLabel =
            new LocalizedText("Start number", "Начальный номер");

        internal static readonly LocalizedText TopDown =
            new LocalizedText("Top-down numbering", "Нумерация сверху вниз");

        internal static readonly LocalizedText NumberParameterLabel =
            new LocalizedText("Parameter for the number", "Параметр для номера");

        internal static readonly LocalizedText CutParameterLabel =
            new LocalizedText("Parameter for the cut height", "Параметр для высоты сруба");

        internal static readonly LocalizedText SlabParameterLabel =
            new LocalizedText("Parameter for the raft bottom", "Параметр для отметки низа плиты");

        internal static readonly LocalizedText PileParameterLabel =
            new LocalizedText("Parameter for the pile bottom", "Параметр для отметки низа сваи");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "Values are written as text in millimetres. The numbering counts rows by Y and left to right by X; the cut height is the part of the pile above the nearest slab bottom below its top. The settings are remembered between sessions.",
                "Значения записываются текстом в миллиметрах. Нумерация идёт по рядам Y и слева направо по X; сруб — это часть сваи выше ближайшего низа плиты под её верхом. Настройки запоминаются между сеансами.");

        internal static readonly LocalizedText NumberButton =
            new LocalizedText("Number the piles", "Пронумеровать сваи");

        internal static readonly LocalizedText ElevationsButton =
            new LocalizedText("Write the elevations", "Записать отметки");

        internal static readonly LocalizedText CutButton =
            new LocalizedText("Write the cut heights", "Записать сруб");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText NumberTransaction =
            new LocalizedText("Number the piles", "Нумерация свай");

        internal static readonly LocalizedText ElevationTransaction =
            new LocalizedText("Write the pile elevations", "Запись отметок свай");

        internal static readonly LocalizedText CutTransaction =
            new LocalizedText("Write the pile cut heights", "Запись сруба свай");

        internal static readonly LocalizedText NoPiles =
            new LocalizedText("Pick the piles first.", "Сначала выберите сваи.");

        internal static readonly LocalizedText NoSlabs =
            new LocalizedText("Pick the raft slabs first.", "Сначала выберите плиты ростверка.");

        internal static readonly LocalizedText StartRequired =
            new LocalizedText(
                "Enter the start number as a whole number from 1.",
                "Введите начальный номер целым числом от 1.");

        internal static readonly LocalizedText NumberParameterRequired =
            new LocalizedText(
                "Enter the name of the parameter for the number.",
                "Введите имя параметра для номера.");

        internal static readonly LocalizedText NoTarget =
            new LocalizedText(
                "{0}: the parameter '{1}' is missing or cannot receive text.",
                "{0}: параметр '{1}' отсутствует или не принимает текст.");

        internal static readonly LocalizedText NoGeometry =
            new LocalizedText(
                "{0}: the pile has no box; skipped.",
                "{0}: у сваи нет габарита; пропущена.");

        internal static readonly LocalizedText NoSlabForPile =
            new LocalizedText(
                "{0}: no raft slab takes the pile in; skipped.",
                "{0}: нет плиты ростверка, в которую входит свая; пропущена.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText NumberResult =
            new LocalizedText(
                "Read {0} pile(s); numbered: {1}; refused: {2}.",
                "Прочитано свай: {0}; пронумеровано: {1}; отказов: {2}.");

        internal static readonly LocalizedText ElevationResult =
            new LocalizedText(
                "Read {0} pile(s); elevations written: {1}; skipped: {2}; refused: {3}.",
                "Прочитано свай: {0}; записано отметок: {1}; пропущено: {2}; отказов: {3}.");

        internal static readonly LocalizedText CutResult =
            new LocalizedText(
                "Read {0} pile(s); cut heights written: {1}; skipped: {2}; refused: {3}.",
                "Прочитано свай: {0}; записано срубов: {1}; пропущено: {2}; отказов: {3}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
