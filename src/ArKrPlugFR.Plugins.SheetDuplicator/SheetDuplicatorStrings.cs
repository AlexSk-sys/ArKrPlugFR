using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SheetDuplicatorStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Duplicate sheets", "Дубликация листов");

        internal static readonly LocalizedText Subtitle =
            LocalizedText.Invariant(
                "Creates copies of the selected sheets together with their views in one action.");

        internal static readonly LocalizedText SheetsCaption =
            new LocalizedText("Sheets of the current project", "Листы текущего проекта");

        internal static readonly LocalizedText SearchPlaceholder =
            new LocalizedText("Search by number or name", "Поиск по номеру или имени");

        internal static readonly LocalizedText SettingsCaption =
            new LocalizedText("Duplication settings", "Настройки дублирования");

        internal static readonly LocalizedText OptionGroupCaption =
            new LocalizedText("What to duplicate", "Что дублировать");

        internal static readonly LocalizedText OptionViews =
            new LocalizedText("Views", "Виды");

        internal static readonly LocalizedText OptionLegends =
            new LocalizedText("Legends", "Легенды");

        internal static readonly LocalizedText OptionText =
            new LocalizedText("Text notes", "Текстовые заметки");

        internal static readonly LocalizedText OptionImages =
            new LocalizedText("Raster images", "Растровые изображения");

        internal static readonly LocalizedText OptionSchedules =
            new LocalizedText("Schedules", "Ведомости");

        internal static readonly LocalizedText OptionAnnotations =
            new LocalizedText("Annotations, lines and detailing", "Аннотации, линии и деталирование");

        internal static readonly LocalizedText OptionParameters =
            new LocalizedText("Sheet and stamp parameters", "Параметры листа и штампа");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText(
                "Select at least one sheet to duplicate.",
                "Выберите хотя бы один лист для дублирования.");

        internal static readonly LocalizedText Success =
            new LocalizedText(
                "Sheets duplicated successfully.",
                "Листы успешно продублированы.");

        internal static readonly LocalizedText FailedSheet =
            new LocalizedText(
                "Failed to duplicate sheet {0}: {1}",
                "Не удалось продублировать лист {0}: {1}");

        internal static readonly LocalizedText SheetIssue =
            new LocalizedText(
                "Sheet {0}: {1}",
                "Лист {0}: {1}");

        internal static readonly LocalizedText GeneralFailure =
            new LocalizedText(
                "Failed to complete the operation: {0}",
                "Не удалось выполнить операцию: {0}");

        internal static readonly LocalizedText UnplaceableView =
            new LocalizedText(
                "View {0} cannot be placed on the sheet.",
                "Вид {0} нельзя разместить на листе.");

        internal static readonly LocalizedText DraftingUnplaceableView =
            new LocalizedText(
                "Drafting view {0} cannot be placed on the sheet.",
                "Вид чертежа {0} нельзя разместить на листе.");

        internal static readonly LocalizedText CannotDuplicateView =
            new LocalizedText(
                "View {0} cannot be duplicated.",
                "Вид {0} нельзя продублировать.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText NoDocument =
            new LocalizedText(
                "Could not obtain the project document. Open a project and run the tool again.",
                "Не удалось получить документ проекта. Откройте проект и запустите инструмент ещё раз.");

        internal static readonly LocalizedText TransactionNameFor =
            new LocalizedText(
                "Duplicate sheet {0} ({1} copies)",
                "Дубликация листа {0} ({1} копий)");

        internal static readonly LocalizedText ResultCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText ReasonSheetMissing =
            new LocalizedText(
                "the sheet no longer exists.",
                "лист больше существует.");

        internal static readonly LocalizedText ReasonNoFreeNumber =
            new LocalizedText(
                "no free sheet number could be assigned.",
                "не удалось назначить свободный номер листа.");

        internal static readonly LocalizedText DuplicateButtonText =
            new LocalizedText("Duplicate ({0})", "Дублировать ({0})");
    }
}
