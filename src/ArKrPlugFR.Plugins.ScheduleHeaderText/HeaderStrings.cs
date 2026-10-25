using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.ScheduleHeaderText
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class HeaderStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Text in a schedule header", "Текст в заголовке спецификации");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Writes a value into the header of a schedule",
                "Writes a value into the header of a schedule");

        internal static readonly LocalizedText ScheduleLabel =
            new LocalizedText("Schedule", "Спецификация");

        internal static readonly LocalizedText ParameterLabel =
            new LocalizedText("Parameter (a field of the schedule)", "Параметр (поле спецификации)");

        internal static readonly LocalizedText ValueLabel =
            new LocalizedText("Value", "Значение");

        internal static readonly LocalizedText FromElements =
            new LocalizedText(
                "Take the first non-empty value from the elements of the schedule",
                "Брать первое непустое значение у элементов спецификации");

        internal static readonly LocalizedText ValuePlaceholder =
            new LocalizedText("Value typed here", "Значение, введённое здесь");

        internal static readonly LocalizedText RowLabel =
            new LocalizedText("Header row (counted from one)", "Строка заголовка (счёт с единицы)");

        internal static readonly LocalizedText ColumnLabel =
            new LocalizedText("Header column (counted from one)", "Столбец заголовка (счёт с единицы)");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Run", "Выполнить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Text in a schedule header", "Текст в заголовке спецификации");

        internal static readonly LocalizedText NoSchedules =
            new LocalizedText(
                "The project holds no schedules.",
                "В проекте нет спецификаций.");

        internal static readonly LocalizedText NoParameters =
            new LocalizedText(
                "The chosen schedule has no fields that carry a parameter.",
                "У выбранной спецификации нет полей с параметром.");

        internal static readonly LocalizedText NoSchedule =
            new LocalizedText("Choose a schedule first.", "Сначала выберите спецификацию.");

        internal static readonly LocalizedText NoParameter =
            new LocalizedText("Choose a parameter first.", "Сначала выберите параметр.");

        internal static readonly LocalizedText NoValue =
            new LocalizedText(
                "The value is empty: type one or take it from the elements.",
                "Значение пустое: введите его или возьмите у элементов.");

        internal static readonly LocalizedText NoCategory =
            new LocalizedText(
                "The schedule does not name a single category, so the value cannot be taken from its elements.",
                "Спецификация не указывает одну категорию, поэтому значение нельзя взять у её элементов.");

        internal static readonly LocalizedText NoElementValue =
            new LocalizedText(
                "No element of the schedule carries a value of «{0}».",
                "Ни один элемент спецификации не имеет значения «{0}».");

        internal static readonly LocalizedText NoHeader =
            new LocalizedText(
                "The schedule has no header area.",
                "У спецификации нет области заголовка.");

        internal static readonly LocalizedText RowOutOfRange =
            new LocalizedText(
                "The header has no such row: it holds {0} row(s).",
                "В заголовке нет такой строки: строк — {0}.");

        internal static readonly LocalizedText ColumnOutOfRange =
            new LocalizedText(
                "The header has no such column: it holds {0} column(s).",
                "В заголовке нет такого столбца: столбцов — {0}.");

        internal static readonly LocalizedText Written =
            new LocalizedText(
                "«{0}» was written into the header of «{1}», row {2}, column {3}.",
                "«{0}» записано в заголовок «{1}», строка {2}, столбец {3}.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText(
                "The platform refused the write: {0}.",
                "Платформа отклонила запись: {0}.");

        internal static readonly LocalizedText BodyNote =
            new LocalizedText(
                "The header area is the only place the platform lets a tool write text into a schedule; the rows are written by the model.",
                "Область заголовка — единственное место, куда платформа разрешает инструменту писать текст в спецификации; строки заполняет модель.");
    }
}
