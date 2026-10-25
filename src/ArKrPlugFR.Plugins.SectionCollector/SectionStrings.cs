using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SectionCollector
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SectionStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Collect sections", "Сбор сечений");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "The concrete structures of the active 3D view, by level, category and section",
                "The concrete structures of the active 3D view, by level, category and section");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Report", "Отчёт");

        internal static readonly LocalizedText NoView3D =
            new LocalizedText(
                "Open a project with a 3D view.",
                "Откройте проект с 3D-видом.");

        internal static readonly LocalizedText NoStructures =
            new LocalizedText(
                "The 3D view holds no concrete structures of the supported categories.",
                "В 3D-виде нет бетонных конструкций поддерживаемых категорий.");

        internal static readonly LocalizedText CategoryColumns =
            new LocalizedText("Columns", "Колонны");

        internal static readonly LocalizedText CategoryWalls =
            new LocalizedText("Walls", "Стены");

        internal static readonly LocalizedText CategoryFloors =
            new LocalizedText("Floors", "Перекрытия");

        internal static readonly LocalizedText CategoryBeams =
            new LocalizedText("Beams", "Балки");

        internal static readonly LocalizedText NoLevel =
            new LocalizedText("(no level)", "(без уровня)");

        internal static readonly LocalizedText NoSection =
            new LocalizedText("(no section in the name)", "(сечение не указано в имени)");

        internal static readonly LocalizedText Summary =
            new LocalizedText(
                "Collected: {0} element(s) in {1} type(s).",
                "Собрано: {0} элементов, {1} типоразмеров.");

        internal static readonly LocalizedText SelectAll =
            new LocalizedText("Select all elements", "Выделить все элементы");

        internal static readonly LocalizedText Copy =
            new LocalizedText("Copy to clipboard", "Копировать в буфер");

        internal static readonly LocalizedText Copied =
            new LocalizedText(
                "The report is in the clipboard.",
                "Отчёт скопирован в буфер обмена.");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText SelectFailed =
            new LocalizedText(
                "The elements could not be selected: {0}",
                "Не удалось выделить элементы: {0}");
    }
}
