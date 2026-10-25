using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class CutHolesStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Openings through concrete", "Отверстия в бетоне");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Cuts openings where the equipment of a linked model passes through concrete",
                "Cuts openings where the equipment of a linked model passes through concrete");

        internal static readonly LocalizedText LinkLine =
            new LocalizedText(
                "Pick a linked model; the tool cuts an opening wherever its equipment or services pass through the concrete walls, floors and columns of this project.",
                "Выберите связанную модель; инструмент вырежет отверстие везде, где её оборудование или коммуникации проходят через бетонные стены, перекрытия и колонны этого проекта.");

        internal static readonly LocalizedText ClearanceLabel =
            new LocalizedText("Clearance on every side, mm", "Запас с каждой стороны, мм");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "A whole number from 0 to 500; remembered between sessions.",
                "Целое число от 0 до 500; запоминается между сеансами.");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Pick a link and cut", "Выбрать связь и вырезать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Cut openings through concrete", "Вырезание отверстий в бетоне");

        internal static readonly LocalizedText PickPrompt =
            new LocalizedText("Pick a linked model", "Выберите связанную модель");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText ClearanceRequired =
            new LocalizedText(
                "Enter the clearance as a whole number of millimetres from 0 to 500.",
                "Введите запас целым числом миллиметров от 0 до 500.");

        internal static readonly LocalizedText LinkNotLoaded =
            new LocalizedText(
                "The linked model has to be loaded; it is not available in this project.",
                "Связанная модель должна быть загружена; в этом проекте она недоступна.");

        internal static readonly LocalizedText NoEquipment =
            new LocalizedText(
                "The linked model holds no equipment or services in the supported categories.",
                "В связанной модели нет оборудования или коммуникаций поддерживаемых категорий.");

        internal static readonly LocalizedText NoConcrete =
            new LocalizedText(
                "The project holds no concrete walls, floors or columns.",
                "В проекте нет бетонных стен, перекрытий или колонн.");

        internal static readonly LocalizedText Counts =
            new LocalizedText(
                "Equipment in the link: {0}; concrete elements: {1}.",
                "Оборудования в связи: {0}; бетонных элементов: {1}.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Openings cut: {0}; already there: {1}; not passing through: {2}; too small: {3}; refused: {4}.",
                "Вырезано отверстий: {0}; уже были: {1}; не проходят насквозь: {2}; слишком малы: {3}; отказов: {4}.");

        internal static readonly LocalizedText CutFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText WallNotStraight =
            new LocalizedText(
                "{0}: the wall is not straight; no opening was cut in it.",
                "{0}: стена не прямолинейна; отверстие в ней не вырезано.");

        internal static readonly LocalizedText NotAFamilyInstance =
            new LocalizedText(
                "{0}: the column is not a family instance; no opening was cut in it.",
                "{0}: колонна не является экземпляром семейства; отверстие в ней не вырезано.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
