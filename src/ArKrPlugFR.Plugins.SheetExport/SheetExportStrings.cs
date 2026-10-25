using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SheetExportStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Sheets to PDF", "Листы в PDF");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Exports the ticked sheets as one PDF file per sheet",
                "Exports the ticked sheets as one PDF file per sheet");

        internal static readonly LocalizedText SheetsLabel =
            new LocalizedText("Sheets", "Листы");

        internal static readonly LocalizedText TickAll =
            new LocalizedText("Tick all", "Отметить все");

        internal static readonly LocalizedText UntickAll =
            new LocalizedText("Untick all", "Снять все");

        internal static readonly LocalizedText FolderLabel =
            new LocalizedText("Folder", "Папка");

        internal static readonly LocalizedText Browse =
            new LocalizedText("Browse…", "Обзор…");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Export", "Экспортировать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText NoSheets =
            new LocalizedText("The project holds no sheets.", "В проекте нет листов.");

        internal static readonly LocalizedText NoTicked =
            new LocalizedText("Tick at least one sheet.", "Отметьте хотя бы один лист.");

        internal static readonly LocalizedText NoFolder =
            new LocalizedText("Give a folder for the files.", "Укажите папку для файлов.");

        internal static readonly LocalizedText FolderMissing =
            new LocalizedText(
                "The folder «{0}» does not exist.",
                "Папки «{0}» не существует.");

        internal static readonly LocalizedText NotSupported =
            new LocalizedText(
                "This release of Revit has no API to export a sheet as a PDF file.",
                "В этой версии Revit нет API для экспорта листа в PDF.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Exported {0} sheet(s) into «{1}».",
                "Экспортировано листов: {0} — в «{1}».");

        internal static readonly LocalizedText FileLine =
            new LocalizedText("{0} → {1}.pdf", "{0} → {1}.pdf");

        internal static readonly LocalizedText Duplicates =
            new LocalizedText(
                "Two or more sheets share the file name «{0}»: the files will overwrite one another.",
                "Несколько листов дают имя файла «{0}»: файлы перезапишут друг друга.");

        internal static readonly LocalizedText Failed =
            new LocalizedText("The platform refused the export: {0}", "Платформа отклонила экспорт: {0}");
    }
}
