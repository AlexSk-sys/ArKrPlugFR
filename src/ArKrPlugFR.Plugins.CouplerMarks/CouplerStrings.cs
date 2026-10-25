using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.CouplerMarks
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class CouplerStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Mark rebar couplers from the nearest bar", "Промаркировать соединители по ближайшему стержню");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Writes the mark and the category of the nearest bar's host into the picked couplers",
                "Writes the mark and the category of the nearest bar's host into the picked couplers");

        internal static readonly LocalizedText PickCouplers =
            new LocalizedText("Pick couplers", "Выбрать соединители");

        internal static readonly LocalizedText PickPrompt =
            new LocalizedText("Pick the couplers to mark", "Выберите соединители для маркировки");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText CouplersCaption =
            new LocalizedText("Picked couplers", "Выбранные соединители");

        internal static readonly LocalizedText CouplersLine =
            new LocalizedText("Couplers: {0}", "Соединителей: {0}");

        internal static readonly LocalizedText ToleranceLabel =
            new LocalizedText("Tolerance, mm", "Допуск, мм");

        internal static readonly LocalizedText ToleranceHint =
            new LocalizedText(
                "A whole number from 1 to 2000: a bar is taken when the centre of its box is this close to the central axis of the coupler. Remembered between sessions.",
                "Целое число от 1 до 2000: стержень учитывается, когда центр его габарита ближе этого расстояния к центральной линии соединителя. Запоминается между сеансами.");

        internal static readonly LocalizedText MarkParameterLabel =
            new LocalizedText("Parameter that receives the mark of the host", "Параметр для метки основы");

        internal static readonly LocalizedText CategoryParameterLabel =
            new LocalizedText("Parameter that receives the category of the host", "Параметр для категории основы");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "Both values are written together; a coupler missing one of the parameters or a host without data is reported instead of being written half way. The parameters are looked up on the coupler first, then on its type.",
                "Оба значения записываются вместе; соединитель без одного из параметров или основа без данных попадут в отчёт вместо частичной записи. Параметры ищутся сначала у соединителя, затем у его типа.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Mark", "Промаркировать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Mark rebar couplers", "Маркировка соединителей");

        internal static readonly LocalizedText NoCouplers =
            new LocalizedText("Pick the couplers to mark.", "Выберите соединители для маркировки.");

        internal static readonly LocalizedText ParametersRequired =
            new LocalizedText(
                "Enter the name of the parameter that receives the mark and the name of the one that receives the category.",
                "Введите имя параметра для метки и имя параметра для категории.");

        internal static readonly LocalizedText ToleranceRequired =
            new LocalizedText(
                "Enter the tolerance as a whole number of millimetres from 1 to 2000.",
                "Введите допуск целым числом миллиметров от 1 до 2000.");

        internal static readonly LocalizedText NoGeometry =
            new LocalizedText(
                "{0}: the coupler has no geometry; skipped.",
                "{0}: у соединителя нет геометрии; пропущен.");

        internal static readonly LocalizedText NoBarNearby =
            new LocalizedText(
                "{0}: no bar within the tolerance; skipped.",
                "{0}: в пределах допуска нет стержня; пропущен.");

        internal static readonly LocalizedText NoHostData =
            new LocalizedText(
                "{0}: the host of the bar has no mark or no category; skipped.",
                "{0}: у основы стержня нет марки или категории; пропущен.");

        internal static readonly LocalizedText NoTargetParameter =
            new LocalizedText(
                "{0}: one of the two parameters is missing or cannot receive text.",
                "{0}: один из двух параметров отсутствует или не принимает текст.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText HalfPair =
            new LocalizedText(
                "{0}: the second value could not be written; check the coupler.",
                "{0}: второе значение не записалось; проверьте соединитель.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Read {0} coupler(s); updated: {1}; no bar in reach: {2}; host without data: {3}; refused: {4}.",
                "Прочитано соединителей: {0}; обновлено: {1}; без стержня в допуске: {2}; основа без данных: {3}; отказов: {4}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
