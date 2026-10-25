using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class AreaRebarStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Fill area reinforcement values onto its bars", "Перенести значения армирования по площади на стержни");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Writes a value of every area reinforcement into the same parameter of its bars",
                "Writes a value of every area reinforcement into the same parameter of its bars");

        internal static readonly LocalizedText WholeProject =
            new LocalizedText("The whole project", "Весь проект");

        internal static readonly LocalizedText PickSources =
            new LocalizedText("Pick area reinforcement", "Выбрать армирование по площади");

        internal static readonly LocalizedText PickPrompt =
            new LocalizedText(
                "Pick the area reinforcement to fill from",
                "Выберите армирование по площади");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText SourcesLine =
            new LocalizedText("Picked: {0}", "Выбрано: {0}");

        internal static readonly LocalizedText ParameterLabel =
            new LocalizedText("Parameter to fill the bars from", "Параметр для переноса на стержни");

        internal static readonly LocalizedText OnlyEmpty =
            new LocalizedText("Only bars without a value", "Только стержни без значения");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The value of every area reinforcement is written into the same parameter of the bars whose host it is. Empty values are skipped; existing bar values are kept unless you ask otherwise. The settings are remembered between sessions.",
                "Значение каждого армирования по площади записывается в тот же параметр стержней, для которых оно является основой. Пустые значения пропускаются; существующие значения стержней сохраняются, если не задано иное. Настройки запоминаются между сеансами.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Fill the bars", "Заполнить стержни");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Fill bars from area reinforcement", "Заполнение стержней по армированию");

        internal static readonly LocalizedText NoSources =
            new LocalizedText(
                "There is no area reinforcement in the scope.",
                "В выбранной области нет армирования по площади.");

        internal static readonly LocalizedText ParameterRequired =
            new LocalizedText(
                "Choose the parameter to fill the bars from.",
                "Выберите параметр для переноса на стержни.");

        internal static readonly LocalizedText NoSourceValue =
            new LocalizedText(
                "{0}: the area reinforcement has no value in this parameter; skipped.",
                "{0}: у армирования нет значения в этом параметре; пропущено.");

        internal static readonly LocalizedText NoBars =
            new LocalizedText(
                "{0}: the area reinforcement has no bars; skipped.",
                "{0}: у армирования нет стержней; пропущено.");

        internal static readonly LocalizedText NoTargetParameter =
            new LocalizedText(
                "{0}: the parameter is missing or cannot receive text.",
                "{0}: параметр отсутствует или не принимает текст.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Sources read: {0}; without a value: {1}; without bars: {2}; bars filled: {3}; bars kept: {4}; refused: {5}.",
                "Прочитано армирований: {0}; без значения: {1}; без стержней: {2}; заполнено стержней: {3}; сохранено стержней: {4}; отказов: {5}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
