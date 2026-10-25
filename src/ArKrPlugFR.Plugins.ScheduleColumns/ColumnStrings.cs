using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class ColumnStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Empty columns of a schedule", "Пустые столбцы спецификации");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Hides the columns of a schedule that tell the reader nothing",
                "Hides the columns of a schedule that tell the reader nothing");

        internal static readonly LocalizedText ScheduleLabel =
            new LocalizedText("Schedule", "Спецификация");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Run", "Выполнить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Empty columns of a schedule", "Пустые столбцы спецификации");

        internal static readonly LocalizedText NoSchedules =
            new LocalizedText("The project holds no schedules.", "В проекте нет спецификаций.");

        internal static readonly LocalizedText NoSchedule =
            new LocalizedText("Choose a schedule first.", "Сначала выберите спецификацию.");

        internal static readonly LocalizedText NoBody =
            new LocalizedText(
                "The schedule has no body to read.",
                "У спецификации нет основной части для чтения.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Columns: hidden {0}; shown {1}; left as they were {2}.",
                "Столбцов: скрыто {0}; показано {1}; оставлено как было {2}.");

        internal static readonly LocalizedText NothingChanged =
            new LocalizedText(
                "Nothing changed: every column already matches its content.",
                "Ничего не изменилось: каждый столбец уже соответствует своему содержанию.");

        internal static readonly LocalizedText NoParameterColumn =
            new LocalizedText(
                "Column {0} carries no parameter and was left alone.",
                "Столбец {0} не связан с параметром и оставлен как есть.");

        internal static readonly LocalizedText Refused =
            new LocalizedText(
                "Column {0} was refused: {1}",
                "Столбец {0} — отказ: {1}");
    }
}
