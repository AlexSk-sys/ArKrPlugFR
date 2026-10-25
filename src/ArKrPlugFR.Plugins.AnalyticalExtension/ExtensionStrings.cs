using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class ExtensionStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Extend the analytical model up", "Продление аналитической модели вверх");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Extends analytical columns and walls up to the nearest floor or beam above",
                "Extends analytical columns and walls up to the nearest floor or beam above");

        internal static readonly LocalizedText SourcesLabel =
            new LocalizedText("Columns and walls to extend", "Колонны и стены для продления");

        internal static readonly LocalizedText TargetsLabel =
            new LocalizedText("Floors and beams to extend to", "Перекрытия и балки, до которых продлевать");

        internal static readonly LocalizedText PickSources =
            new LocalizedText("Pick columns and walls", "Выбрать колонны и стены");

        internal static readonly LocalizedText PickTargets =
            new LocalizedText("Pick floors and beams", "Выбрать перекрытия и балки");

        internal static readonly LocalizedText ReachLabel =
            new LocalizedText("Reach in plan, mm", "Досягаемость в плане, мм");

        internal static readonly LocalizedText SourcesNone =
            new LocalizedText("Sources: none picked", "Источники: не выбраны");

        internal static readonly LocalizedText TargetsNone =
            new LocalizedText("Targets: none picked", "Цели: не выбраны");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Extend", "Продлить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Extend the analytical model up", "Продление аналитической модели вверх");

        internal static readonly LocalizedText PickSourcesPrompt =
            new LocalizedText(
                "Pick the columns and walls to extend, then press Finish",
                "Укажите колонны и стены для продления и нажмите «Готово»");

        internal static readonly LocalizedText PickTargetsPrompt =
            new LocalizedText(
                "Pick the floors and beams to extend to, then press Finish",
                "Укажите перекрытия и балки, до которых продлевать, и нажмите «Готово»");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText(
                "The pick was cancelled; the previous choice is kept.",
                "Выбор отменён; прежний выбор сохранён.");

        internal static readonly LocalizedText NoSources =
            new LocalizedText("Pick the columns and walls first.", "Сначала укажите колонны и стены.");

        internal static readonly LocalizedText NoTargets =
            new LocalizedText("Pick the floors and beams first.", "Сначала укажите перекрытия и балки.");

        internal static readonly LocalizedText NoTargetBoxes =
            new LocalizedText(
                "None of the targets has a bounding box.",
                "Ни у одной цели нет габаритной рамки.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Sources: {0}; extended: {1}; without a target above: {2}; without an analytical line: {3}; attached at the top: {4}; refused: {5}.",
                "Источников: {0}; продлено: {1}; без цели сверху: {2}; без аналитической линии: {3}; с прикреплённым верхом: {4}; отказов: {5}.");

        internal static readonly LocalizedText NothingExtended =
            new LocalizedText(
                "Nothing was extended: every member already reaches its target.",
                "Ничего не продлено: каждый элемент уже доходит до своей цели.");

        internal static readonly LocalizedText NoAnalyticalLine =
            new LocalizedText(
                "{0} was skipped: the platform holds no analytical line for it.",
                "{0} пропущен: платформа не даёт для него аналитическую линию.");

        internal static readonly LocalizedText NoTargetAbove =
            new LocalizedText(
                "{0} was skipped: no target lies above it within the reach.",
                "{0} пропущен: в пределах досягаемости нет цели выше него.");

        internal static readonly LocalizedText Attached =
            new LocalizedText(
                "{0} was skipped: its top is attached to another element.",
                "{0} пропущен: его верх прикреплён к другому элементу.");

        internal static readonly LocalizedText Refused =
            new LocalizedText("{0} was refused: {1}", "{0} — отказ: {1}");

        internal static readonly LocalizedText RaisedColumn =
            new LocalizedText(
                "{0} was raised through its top level; the analytical model follows it.",
                "{0} поднят через верхний уровень; аналитическая модель следует за ним.");
    }
}
