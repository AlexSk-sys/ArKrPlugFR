using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SplitFloors
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SplitStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Split floors into separate floors", "Разделить полы на отдельные полы");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Turns every part of a flat floor into its own floor",
                "Turns every part of a flat floor into its own floor");

        internal static readonly LocalizedText FloorsCaption =
            new LocalizedText("Floors picked", "Выбрано полов");

        internal static readonly LocalizedText PickFloors =
            new LocalizedText("Pick floors", "Выбрать полы");

        internal static readonly LocalizedText Clear =
            new LocalizedText("Clear", "Очистить");

        internal static readonly LocalizedText PickPrompt =
            new LocalizedText("Pick the floors to split", "Выберите полы для разделения");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "Each outer contour of a flat floor becomes its own floor with the openings it holds; the source floor is replaced. One floor is one undoable step.",
                "Каждый внешний контур плоского пола становится отдельным полом со своими проёмами; исходный пол заменяется. Один пол — один шаг отмены.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Split", "Разделить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Split a floor", "Разделение пола");

        internal static readonly LocalizedText NoFloors =
            new LocalizedText(
                "Pick the floors to split.",
                "Выберите полы для разделения.");

        internal static readonly LocalizedText DeleteRefused =
            new LocalizedText(
                "the source floor could not be deleted",
                "исходный пол не удалось удалить");

        internal static readonly LocalizedText FloorsLine =
            new LocalizedText("Floors: {0}", "Полы: {0}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Read {0} floor(s); split: {1}; created: {2}; skipped: {3}; refused: {4}.",
                "Прочитано полов: {0}; разделено: {1}; создано: {2}; пропущено: {3}; отказов: {4}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");

        internal static readonly LocalizedText NoTopFace =
            new LocalizedText(
                "{0}: no flat top face; skipped.",
                "{0}: нет плоской верхней грани; пропущен.");

        internal static readonly LocalizedText SingleContour =
            new LocalizedText(
                "{0}: a single contour; nothing to split.",
                "{0}: один контур; делить нечего.");

        internal static readonly LocalizedText RejectedContours =
            new LocalizedText(
                "{0}: contours left out — degenerate: {1}, inside an opening: {2}, duplicate: {3}.",
                "{0}: пропущено контуров — вырожденных: {1}, внутри проёма: {2}, дубликатов: {3}.");

        internal static readonly LocalizedText SplitFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");
    }
}
