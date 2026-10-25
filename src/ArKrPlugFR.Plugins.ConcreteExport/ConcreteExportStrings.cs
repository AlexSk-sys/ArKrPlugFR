using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class ConcreteExportStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Export concrete elements", "Экспорт бетонных конструкций");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Writes the concrete structures of a project or a selection into a comma-separated file",
                "Writes the concrete structures of a project or a selection into a comma-separated file");

        internal static readonly LocalizedText OnlySelection =
            new LocalizedText("Only the selected elements", "Только выделенные элементы");

        internal static readonly LocalizedText ChooseFile =
            new LocalizedText("Choose the file...", "Выбрать файл...");

        internal static readonly LocalizedText FileLabel =
            new LocalizedText("File", "Файл");

        internal static readonly LocalizedText NotChosen =
            new LocalizedText("not chosen", "не выбран");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The rows carry the id, the category, the family, the type, the level, the materials, the volume in cubic metres, the sizes of the box and the mark. Only elements whose family, type or material names carry the word concrete are written; the file opens in any spreadsheet. The scope and the file are remembered between sessions.",
                "Строки содержат идентификатор, категорию, семейство, тип, этаж, материалы, объём в м³, габариты и марку. Записываются только элементы, в имени семейства, типа или материала которых есть слово «бетон»; файл открывается любой таблицей. Область и файл запоминаются между сеансами.");

        internal static readonly LocalizedText Export =
            new LocalizedText("Export", "Экспортировать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText FileRequired =
            new LocalizedText("Choose the file to write.", "Выберите файл для записи.");

        internal static readonly LocalizedText NothingToExport =
            new LocalizedText(
                "No concrete element was found in the scope.",
                "В выбранной области не найдено бетонных конструкций.");

        internal static readonly LocalizedText FileFailed =
            new LocalizedText(
                "The file '{0}' could not be written: {1}",
                "Файл '{0}' не удалось записать: {1}");

        internal static readonly LocalizedText RowFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Elements scanned: {0}; rows written: {1}; not concrete: {2}; refused: {3}. File: {4}.",
                "Прочитано элементов: {0}; записано строк: {1}; не бетон: {2}; отказов: {3}. Файл: {4}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
