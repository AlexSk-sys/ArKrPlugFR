using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SelectStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Select elements by family type", "Выделить элементы по типоразмеру");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Finds the family types of the project and selects their instances",
                "Finds the family types of the project and selects their instances");

        internal static readonly LocalizedText ScopeCaption =
            new LocalizedText("Search scope", "Область поиска");

        internal static readonly LocalizedText ScopeAll =
            new LocalizedText("All families", "Все семейства");

        internal static readonly LocalizedText ScopePlaced =
            new LocalizedText("Placed in the model", "В модели (размещённые)");

        internal static readonly LocalizedText ScopeVisible =
            new LocalizedText("Visible in the current view", "На текущем виде");

        internal static readonly LocalizedText ScopeUnavailableOnSheets =
            new LocalizedText(
                "The current-view scope is not available on a sheet; it needs a model view.",
                "Режим «На текущем виде» недоступен на листе; нужен вид модели.");

        internal static readonly LocalizedText SearchCaption =
            new LocalizedText("Search", "Поиск");

        internal static readonly LocalizedText SearchHint =
            new LocalizedText(
                "The query matches the family name and the type name; an empty query keeps every type.",
                "Строка ищет по имени семейства и типоразмера; пустая строка показывает все типы.");

        internal static readonly LocalizedText TreeCaption =
            new LocalizedText("Types", "Типоразмеры");

        internal static readonly LocalizedText TypesLine =
            new LocalizedText("Types shown: {0}", "Показано типоразмеров: {0}");

        internal static readonly LocalizedText TickAll =
            new LocalizedText("Tick all", "Отметить все");

        internal static readonly LocalizedText UntickAll =
            new LocalizedText("Untick all", "Снять отметки");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Show in view", "Показать на виде");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The tool only reads the project: it selects the instances of the ticked types in the model or on the current view and changes nothing.",
                "Инструмент только читает проект: выделяет экземпляры отмеченных типов в модели или на текущем виде и ничего не меняет.");

        internal static readonly LocalizedText NoCategory =
            new LocalizedText("No category", "Без категории");

        internal static readonly LocalizedText NoTypes =
            new LocalizedText("Tick at least one type.", "Отметьте хотя бы один типоразмер.");

        internal static readonly LocalizedText NoTypesInScope =
            new LocalizedText("The scope holds no types.", "В этой области нет типоразмеров.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Types ticked: {0}; elements selected: {1}; types without instances: {2}.",
                "Отмечено типоразмеров: {0}; выделено элементов: {1}; типоразмеров без экземпляров: {2}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
