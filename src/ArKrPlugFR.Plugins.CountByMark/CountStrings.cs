using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class CountStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Count structures by mark", "Конструкции по марке");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Writes into every bar how many structures carry its structure mark",
                "Writes into every bar how many structures carry its structure mark");

        internal static readonly LocalizedText KindLabel =
            new LocalizedText("Structures to count", "Какие конструкции считать");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "The parameter names — a structure's mark, a bar's structure mark and the number written into a bar — live in the tool's settings file.",
                "Имена параметров — марка конструкции, метка основы стержня и записываемое число — задаются в файле настроек инструмента.");

        internal static readonly LocalizedText KindColumns =
            new LocalizedText("Columns", "Колонны");

        internal static readonly LocalizedText KindWalls =
            new LocalizedText("Walls", "Стены");

        internal static readonly LocalizedText KindFloors =
            new LocalizedText("Floors", "Перекрытия");

        internal static readonly LocalizedText KindBeams =
            new LocalizedText("Beams", "Балки");

        internal static readonly LocalizedText KindAll =
            new LocalizedText("All structures", "Все конструкции");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Run", "Выполнить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Count structures by mark", "Подсчёт конструкций по марке");

        internal static readonly LocalizedText NoStructures =
            new LocalizedText(
                "No structures with a mark of the chosen kind were found.",
                "Не найдено конструкций выбранного вида с заполненной маркой.");

        internal static readonly LocalizedText NoBars =
            new LocalizedText(
                "The project holds no rebar bars.",
                "В проекте нет стержней арматуры.");

        internal static readonly LocalizedText NothingWritten =
            new LocalizedText(
                "Nothing was written: every bar was skipped. See the counts below.",
                "Ничего не записано: все стержни пропущены. Счётчики ниже.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Structures: {0} in {1} mark(s); bars: {2}; updated: {3}; without a structure mark: {4}; without a count for their mark: {5}; without the number parameter: {6}; refused: {7}.",
                "Конструкций: {0} в {1} марке(ах); стержней: {2}; обновлено: {3}; без метки основы: {4}; без числа для их марки: {5}; без параметра числа: {6}; отказов: {7}.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
