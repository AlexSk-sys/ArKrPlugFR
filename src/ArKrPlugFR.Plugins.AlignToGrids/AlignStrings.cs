using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class AlignStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Align columns to grids", "Выровнять колонны по осям");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Pulls the picked columns exactly onto the picked grids",
                "Pulls the picked columns exactly onto the picked grids");

        internal static readonly LocalizedText ColumnsLine =
            new LocalizedText("Columns: {0}", "Колонн: {0}");

        internal static readonly LocalizedText GridsLine =
            new LocalizedText("Grids: {0}", "Осей: {0}");

        internal static readonly LocalizedText PickColumns =
            new LocalizedText("Pick columns", "Выбрать колонны");

        internal static readonly LocalizedText PickGrids =
            new LocalizedText("Pick grids", "Выбрать оси");

        internal static readonly LocalizedText PickColumnsPrompt =
            new LocalizedText(
                "Pick the columns to align",
                "Выберите колонны для выравнивания");

        internal static readonly LocalizedText PickGridsPrompt =
            new LocalizedText(
                "Pick the grids to align to",
                "Выберите оси для выравнивания");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText CaptureLabel =
            new LocalizedText("Capture distance, mm", "Захват, мм");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "A whole number from 1 to 100: a column is aligned when its centre lies this close to a grid. Remembered between sessions.",
                "Целое число от 1 до 100: колонна выравнивается, когда её центр ближе этого расстояния к оси. Запоминается между сеансами.");

        internal static readonly LocalizedText CaptureRequired =
            new LocalizedText(
                "Enter the capture distance as a whole number of millimetres from 1 to 100.",
                "Введите захват целым числом миллиметров от 1 до 100.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Align", "Выровнять");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Align columns to grids", "Выравнивание колонн по осям");

        internal static readonly LocalizedText NoColumns =
            new LocalizedText(
                "Pick the columns to align.",
                "Выберите колонны для выравнивания.");

        internal static readonly LocalizedText NoGrids =
            new LocalizedText(
                "Pick the grids to align to; the picked grids hold no straight lines.",
                "Выберите оси для выравнивания; среди выбранных нет прямолинейных.");

        internal static readonly LocalizedText GridNotStraight =
            new LocalizedText(
                "{0}: the grid is not a straight line; skipped.",
                "{0}: ось не прямолинейна; пропущена.");

        internal static readonly LocalizedText GridNotInPlane =
            new LocalizedText(
                "{0}: the grid does not run in the horizontal plane; skipped.",
                "{0}: ось не лежит в горизонтальной плоскости; пропущена.");

        internal static readonly LocalizedText NotPointBased =
            new LocalizedText(
                "{0}: the column has no location point; skipped.",
                "{0}: у колонны нет точки положения; пропущена.");

        internal static readonly LocalizedText MoveFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Aligned {0} of {1} column(s) to the grids; already on a grid: {2}; no grid within the capture distance: {3}; in groups: {4}; refused: {5}.",
                "Выровнено колонн: {0} из {1}; уже стояли на оси: {2}; без оси в пределах захвата: {3}; в группах: {4}; отказов: {5}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
