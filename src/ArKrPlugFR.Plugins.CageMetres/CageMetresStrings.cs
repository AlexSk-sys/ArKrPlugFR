using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class CageMetresStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Rebar cage metres", "Погонные метры каркаса");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Measures a cage along its layout lines and its own steps",
                "Measures a cage along its layout lines and its own steps");

        internal static readonly LocalizedText LinesLabel =
            new LocalizedText("Layout lines", "Линии раскладки");

        internal static readonly LocalizedText CageLabel =
            new LocalizedText("Cage", "Каркас");

        internal static readonly LocalizedText PickLines =
            new LocalizedText("Pick layout lines", "Выбрать линии");

        internal static readonly LocalizedText PickCage =
            new LocalizedText("Pick the cage", "Выбрать каркас");

        internal static readonly LocalizedText LinesNone =
            new LocalizedText("Lines: none picked", "Линии: не выбраны");

        internal static readonly LocalizedText CageNone =
            new LocalizedText("Cage: none picked", "Каркас: не выбран");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "The parameter names — the number of longitudinal bars, the two steps, the bar length, the three group flags and the calculated length — live in the tool's settings file.",
                "Имена параметров — число продольных, два шага, длина стержня, три признака групп и расчётная длина — задаются в файле настроек инструмента.");

        internal static readonly LocalizedText ResultParameterLabel =
            new LocalizedText("Calculated-length parameter", "Параметр расчётной длины");

        internal static readonly LocalizedText WriteSwitch =
            new LocalizedText(
                "Write the calculated length into the cage",
                "Записывать расчётную длину в каркас");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Run", "Выполнить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Rebar cage metres", "Погонные метры каркаса");

        internal static readonly LocalizedText PickLinesPrompt =
            new LocalizedText(
                "Pick the layout lines of the cage, then press Finish",
                "Укажите линии раскладки каркаса и нажмите «Готово»");

        internal static readonly LocalizedText PickCagePrompt =
            new LocalizedText("Pick the cage", "Укажите каркас");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText(
                "The pick was cancelled; the previous choice is kept.",
                "Выбор отменён; прежний выбор сохранён.");

        internal static readonly LocalizedText NoLines =
            new LocalizedText("Pick the layout lines first.", "Сначала укажите линии раскладки.");

        internal static readonly LocalizedText NoCage =
            new LocalizedText("Pick the cage first.", "Сначала укажите каркас.");

        internal static readonly LocalizedText CageLine =
            new LocalizedText("Cage: {0}", "Каркас: {0}");

        internal static readonly LocalizedText LinesLine =
            new LocalizedText("Lines: {0} at {1} m", "Линии: {0} шт, {1} м");

        internal static readonly LocalizedText LongitudinalLine =
            new LocalizedText(
                "Longitudinal: {0} × {1} m = {2} m",
                "Продольные: {0} × {1} м = {2} м");

        internal static readonly LocalizedText InclinedLine =
            new LocalizedText(
                "Inclined: {0} positions × 2 × {1} m = {2} m",
                "Наклонные: {0} поз × 2 × {1} м = {2} м");

        internal static readonly LocalizedText TransverseLine =
            new LocalizedText(
                "Transverse: {0} positions × ({1}) m = {2} m",
                "Поперечные: {0} поз × ({1}) м = {2} м");

        internal static readonly LocalizedText TotalLine =
            new LocalizedText("Total: {0} m", "Итого: {0} м");

        internal static readonly LocalizedText Written =
            new LocalizedText(
                "The total was written into «{0}».",
                "Итог записан в «{0}».");

        internal static readonly LocalizedText WriteOff =
            new LocalizedText(
                "Writing is switched off in the window; the model is unchanged.",
                "Запись отключена в окне; модель не изменялась.");

        internal static readonly LocalizedText NoParameter =
            new LocalizedText(
                "The parameter «{0}» was not found on the cage or its type.",
                "Параметр «{0}» не найден у каркаса и его типа.");

        internal static readonly LocalizedText ReadOnly =
            new LocalizedText("The parameter «{0}» is read-only.", "Параметр «{0}» только для чтения.");

        internal static readonly LocalizedText NotANumber =
            new LocalizedText(
                "The parameter «{0}» cannot hold a number.",
                "Параметр «{0}» не может хранить число.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("The write was refused: {0}.", "Запись отклонена: {0}.");

        internal static readonly LocalizedText MissingCount =
            new LocalizedText(
                "The parameter «{0}» with the number of longitudinal bars was not found; the longitudinal group is left out.",
                "Параметр «{0}» с числом продольных стержней не найден; группа продольных исключена.");

        internal static readonly LocalizedText MissingStep =
            new LocalizedText(
                "The parameter «{0}» with a step was not found or is zero; its group is left out.",
                "Параметр «{0}» с шагом не найден или равен нулю; его группа исключена.");

        internal static readonly LocalizedText MissingInclinedLength =
            new LocalizedText(
                "No inclined bar length was found; the inclined group is left out.",
                "Длина наклонного стержня не найдена; группа наклонных исключена.");

        internal static readonly LocalizedText MissingTransverseLength =
            new LocalizedText(
                "No transverse bar lengths were found; the transverse group is left out.",
                "Длины поперечных стержней не найдены; группа поперечных исключена.");

        internal static readonly LocalizedText Unclassified =
            new LocalizedText(
                "{0} sub-component(s) carry none of the group flags and were left out.",
                "{0} вложенных элементов не имеют признака группы и не учтены.");

        internal static readonly LocalizedText NoSubComponents =
            new LocalizedText(
                "The cage holds no sub-components; the inclined and transverse groups stay empty.",
                "У каркаса нет вложенных элементов; группы наклонных и поперечных остаются пустыми.");

        internal static readonly LocalizedText WarningsCaption =
            new LocalizedText("Warnings:", "Внимание:");
    }
}
