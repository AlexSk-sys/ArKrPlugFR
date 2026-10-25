using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class DuplicateFinderStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Duplicate types and families", "Дубликаты типов и семейств");

        internal static readonly LocalizedText TypesTab =
            new LocalizedText("Types", "Типы");

        internal static readonly LocalizedText FamiliesTab =
            new LocalizedText("Families", "Семейства");

        internal static readonly LocalizedText NearTab =
            new LocalizedText("Close matches", "Похожие");

        internal static readonly LocalizedText ColumnFamily =
            new LocalizedText("Family", "Семейство");

        internal static readonly LocalizedText ColumnCategory =
            new LocalizedText("Category", "Категория");

        internal static readonly LocalizedText ColumnType =
            new LocalizedText("Type to remove", "Удаляемый тип");

        internal static readonly LocalizedText ColumnKeptType =
            new LocalizedText("Type to keep", "Оставляемый тип");

        internal static readonly LocalizedText ColumnKeptFamily =
            new LocalizedText("Family to keep", "Оставляемое семейство");

        internal static readonly LocalizedText ColumnInstances =
            new LocalizedText("In project", "В проекте");

        internal static readonly LocalizedText ColumnStatus =
            new LocalizedText("Status", "Состояние");

        internal static readonly LocalizedText ColumnMatchedTypes =
            new LocalizedText("Types, kept of original", "Типов, совпало из исходного");

        internal static readonly LocalizedText ColumnDifference =
            new LocalizedText("Differing parameters", "Отличий в параметрах");

        internal static readonly LocalizedText ColumnTypeA =
            new LocalizedText("Type A", "Тип A");

        internal static readonly LocalizedText ColumnTypeB =
            new LocalizedText("Type B", "Тип B");

        internal static readonly LocalizedText StatusRemovable =
            new LocalizedText("Can be deleted", "Можно удалить");

        internal static readonly LocalizedText StatusUsed =
            new LocalizedText("In use, kept", "Используется, остаётся");

        internal static readonly LocalizedText SelectAll =
            new LocalizedText("Select all", "Выбрать все");

        internal static readonly LocalizedText ClearSelection =
            new LocalizedText("Clear", "Снять выделение");

        internal static readonly LocalizedText CopyList =
            new LocalizedText("Copy list", "Копировать список");

        internal static readonly LocalizedText Delete =
            new LocalizedText("Delete selected", "Удалить выбранные");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ConfirmCaption =
            new LocalizedText("Delete duplicates", "Удаление дубликатов");

        internal static readonly LocalizedText ConfirmText =
            new LocalizedText(
                "Delete {0} item(s)? The change can be undone with one Ctrl+Z.",
                "Удалить объектов: {0}? Изменение отменяется одним Ctrl+Z.");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText("Nothing is selected.", "Ничего не выбрано.");

        internal static readonly LocalizedText NearIntro =
            new LocalizedText(
                "No exact duplicates were found. These types differ in only a few parameters:",
                "Точных дубликатов не найдено. Эти типы отличаются лишь несколькими параметрами:");

        internal static readonly LocalizedText SummaryChecked =
            new LocalizedText("Types compared: {0}", "Сравнено типов: {0}");

        internal static readonly LocalizedText SummaryTypes =
            new LocalizedText(
                "Duplicate types: {0}, of them deletable: {1}",
                "Дублирующихся типов: {0}, из них можно удалить: {1}");

        internal static readonly LocalizedText SummaryFamilies =
            new LocalizedText(
                "Duplicate families: {0}, of them deletable: {1}",
                "Дублирующихся семейств: {0}, из них можно удалить: {1}");

        internal static readonly LocalizedText ResultCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText DeletedLine =
            new LocalizedText("Deleted: {0}", "Удалено: {0}");

        internal static readonly LocalizedText NotDeletedLine =
            new LocalizedText("Could not delete: {0}", "Не удалось удалить: {0}");

        internal static readonly LocalizedText SkippedLine =
            new LocalizedText(
                "Skipped because they are in use: {0}",
                "Пропущено, так как используется: {0}");

        internal static readonly LocalizedText ReadErrorsLine =
            new LocalizedText("Items that could not be read: {0}", "Не удалось прочитать объектов: {0}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText ZoomHint =
            new LocalizedText(
                "Double-click a row to show it in the model.",
                "Двойной щелчок по строке — показать её в модели.");

        internal static readonly LocalizedText UsedHint =
            new LocalizedText(
                "Items with placed instances cannot be selected.",
                "Объекты с размещёнными экземплярами выбрать нельзя.");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Delete duplicate types and families", "Удаление дубликатов типов и семейств");

        internal static readonly LocalizedText CopyHeader =
            new LocalizedText("Duplicate types and families", "Дубликаты типов и семейств");

        internal static readonly LocalizedText CopyTypesHeader =
            new LocalizedText("Types", "Типы");

        internal static readonly LocalizedText CopyFamiliesHeader =
            new LocalizedText("Families", "Семейства");
    }
}
