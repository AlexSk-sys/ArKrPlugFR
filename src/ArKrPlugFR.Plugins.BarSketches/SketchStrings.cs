using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SketchStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Sketch sheets of bars", "Листы-эскизы стержней");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Makes a sheet per bar: a view cropped to the bar and a note with its facts",
                "Makes a sheet per bar: a view cropped to the bar and a note with its facts");

        internal static readonly LocalizedText BarsLabel =
            new LocalizedText("Bars to sketch", "Стержни для эскизов");

        internal static readonly LocalizedText PickBars =
            new LocalizedText("Pick bars", "Выбрать стержни");

        internal static readonly LocalizedText BarsNone =
            new LocalizedText("Bars: none picked", "Стержни: не выбраны");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Make sheets", "Создать листы");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Sketch sheets of bars", "Листы-эскизы стержней");

        internal static readonly LocalizedText PickBarsPrompt =
            new LocalizedText(
                "Pick the bars to sketch, then press Finish",
                "Укажите стержни для эскизов и нажмите «Готово»");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText(
                "The pick was cancelled; the previous choice is kept.",
                "Выбор отменён; прежний выбор сохранён.");

        internal static readonly LocalizedText NoBars =
            new LocalizedText("Pick the bars first.", "Сначала укажите стержни.");

        internal static readonly LocalizedText NoTitleBlock =
            new LocalizedText(
                "The project holds no title block to make a sheet from.",
                "В проекте нет основной надписи, из которой можно создать лист.");

        internal static readonly LocalizedText NoViewFamily =
            new LocalizedText(
                "The project holds no three-dimensional view family.",
                "В проекте нет семейства трёхмерных видов.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Sheets made: {0} of {1} bar(s); skipped: {2}; refused: {3}.",
                "Создано листов: {0} из {1} стержней; пропущено: {2}; отказов: {3}.");

        internal static readonly LocalizedText Remaining =
            new LocalizedText(
                "The next run continues with the remaining {0} bar(s).",
                "Следующий запуск продолжит с оставшимися стержнями: {0}.");

        internal static readonly LocalizedText SheetLine =
            new LocalizedText("{0} — sheet {1}", "{0} — лист {1}");

        internal static readonly LocalizedText NothingMade =
            new LocalizedText(
                "No sheet was made; see the reasons below.",
                "Ни один лист не создан; причины ниже.");

        internal static readonly LocalizedText NoBox =
            new LocalizedText(
                "{0} was skipped: the platform reports no bounding box for it.",
                "{0} пропущен: платформа не сообщает для него габаритную рамку.");

        internal static readonly LocalizedText Refused =
            new LocalizedText("{0} was refused: {1}", "{0} — отказ: {1}");

        internal static readonly LocalizedText ShapeLabel =
            new LocalizedText("Shape", "Форма");

        internal static readonly LocalizedText DiameterLabel =
            new LocalizedText("Diameter", "Диаметр");

        internal static readonly LocalizedText LengthLabel =
            new LocalizedText("Length", "Длина");

        internal static readonly LocalizedText MarkLabel =
            new LocalizedText("Mark", "Марка");

        internal static readonly LocalizedText Unknown =
            new LocalizedText("not known", "неизвестно");
    }
}
