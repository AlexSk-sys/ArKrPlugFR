using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SheetBrowserStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Sheet tree from the project browser", "Дерево листов из браузера проекта");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Shows the sheets under the folders of the project browser scheme",
                "Shows the sheets under the folders of the project browser scheme");

        internal static readonly LocalizedText CopyPath =
            new LocalizedText("Copy path", "Копировать путь");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The tree follows the active sheet organization scheme: the groups of the browser, the sheets in the order the scheme sorts them. Select a row — or right-click it — to copy its path. The tool only reads the project.",
                "Дерево повторяет активную схему организации листов: группы браузера, листы в порядке схемы. Выберите строку — или щёлкните её правой кнопкой — чтобы скопировать путь. Инструмент только читает проект.");

        internal static readonly LocalizedText Summary =
            new LocalizedText(
                "Sheets: {0}; groups: {1}; left out by the scheme: {2}; unreadable: {3}.",
                "Листов: {0}; групп: {1}; скрыто схемой: {2}; не прочитано: {3}.");

        internal static readonly LocalizedText NoSheets =
            new LocalizedText("The project holds no sheets.", "В проекте нет листов.");

        internal static readonly LocalizedText NoScheme =
            new LocalizedText(
                "The project has no sheet organization scheme; the sheets are shown in one list.",
                "В проекте не настроена схема организации листов; листы показаны одним списком.");

        internal static readonly LocalizedText NoSelection =
            new LocalizedText(
                "Select a row to copy its path.",
                "Выберите строку, чтобы скопировать путь.");

        internal static readonly LocalizedText Copied =
            new LocalizedText("Copied: {0}", "Скопировано: {0}");

        internal static readonly LocalizedText CopyFailed =
            new LocalizedText(
                "The clipboard is not available: {0}",
                "Буфер обмена недоступен: {0}");

        internal static readonly LocalizedText GroupingError =
            new LocalizedText("Grouping error", "Ошибки группировки");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");
    }
}
