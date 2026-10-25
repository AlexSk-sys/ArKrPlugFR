using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.RefreshAnnotations
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class RefreshStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Refresh annotations", "Обновить аннотации");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Makes the tags of the active view recompute their text",
                "Makes the tags of the active view recompute their text");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Refresh annotations", "Обновление аннотаций");

        internal static readonly LocalizedText NoView =
            new LocalizedText(
                "Open a view before running this tool.",
                "Откройте вид и повторите запуск.");

        internal static readonly LocalizedText NoTags =
            new LocalizedText(
                "The active view has no tags to refresh.",
                "В активном виде нет марок для обновления.");

        internal static readonly LocalizedText AllOrphaned =
            new LocalizedText(
                "Every tag in the active view is orphaned; there is nothing to refresh.",
                "Все марки активного вида потеряли элементы; обновлять нечего.");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText(
                "None of the selected elements carries a tag in the active view.",
                "Ни у одного из выбранных элементов нет марки в активном виде.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Refreshed {0} of {1} tag(s) in view '{2}'; orphaned: {3}; refused: {4}.",
                "Обновлено марок: {0} из {1} в виде «{2}»; потеряли элемент: {3}; отказов: {4}.");

        internal static readonly LocalizedText Refused =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
