using ArKrPlugFR.Core.Localization;
using ArKrPlugFR.Core.Marking;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class MarkStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Mark structures by their kind", "Промаркировать конструкции по виду");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Decides the kind of every structure and writes the mark it asks for",
                "Decides the kind of every structure and writes the mark it asks for");

        internal static readonly LocalizedText OnlyEmpty =
            new LocalizedText("Only elements without a mark", "Только элементы без марки");

        internal static readonly LocalizedText MarksCaption =
            new LocalizedText("Mark codes", "Коды марок");

        internal static readonly LocalizedText SuffixesCaption =
            new LocalizedText("Material suffixes", "Суффиксы материалов");

        internal static readonly LocalizedText ColumnLabel =
            new LocalizedText("Column", "Колонна");

        internal static readonly LocalizedText BeamLabel =
            new LocalizedText("Beam", "Балка");

        internal static readonly LocalizedText FloorLabel =
            new LocalizedText("Floor", "Перекрытие");

        internal static readonly LocalizedText WallLabel =
            new LocalizedText("Wall", "Стена");

        internal static readonly LocalizedText FoundationLabel =
            new LocalizedText("Foundation", "Фундамент");

        internal static readonly LocalizedText PileLabel =
            new LocalizedText("Pile", "Свая");

        internal static readonly LocalizedText ConcreteSuffixLabel =
            new LocalizedText("Concrete", "Бетон");

        internal static readonly LocalizedText SteelSuffixLabel =
            new LocalizedText("Steel", "Сталь");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The mark is the code of the kind plus the suffix of the material family; a kind with an empty code is left out. The kind comes from the category and the shape, the family from the material names. The settings are remembered between sessions.",
                "Марка — это код вида плюс суффикс материала; вид с пустым кодом пропускается. Вид определяется категорией и формой, материал — именами материалов. Настройки запоминаются между сеансами.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Mark", "Промаркировать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Mark structures", "Маркировка конструкций");

        internal static readonly LocalizedText MarksRequired =
            new LocalizedText(
                "Set at least one mark code.",
                "Задайте хотя бы один код марки.");

        internal static readonly LocalizedText NoMarkCode =
            new LocalizedText(
                "{0}: the mark code of this kind is empty; skipped.",
                "{0}: код марки этого вида пуст; пропущен.");

        internal static readonly LocalizedText NoTargetParameter =
            new LocalizedText(
                "{0}: the mark parameter is missing or cannot receive text.",
                "{0}: параметр «Марка» отсутствует или не принимает текст.");

        internal static readonly LocalizedText WriteRefused =
            new LocalizedText(
                "{0}: the platform refused to write the mark.",
                "{0}: платформа отказалась записать марку.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Read {0} element(s); marked: {1}; already marked: {2}; without a kind: {3}; without a mark code: {4}; refused: {5}.",
                "Прочитано элементов: {0}; промаркировано: {1}; уже были с маркой: {2}; без вида: {3}; без кода марки: {4}; отказов: {5}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");

        /// <summary>The caption of a kind; used by the report.</summary>
        internal static LocalizedText LabelOf(StructureKind kind)
        {
            switch (kind)
            {
                case StructureKind.Column:
                    return ColumnLabel;

                case StructureKind.Beam:
                    return BeamLabel;

                case StructureKind.Floor:
                    return FloorLabel;

                case StructureKind.Wall:
                    return WallLabel;

                case StructureKind.Foundation:
                    return FoundationLabel;

                default:
                    return PileLabel;
            }
        }
    }
}
