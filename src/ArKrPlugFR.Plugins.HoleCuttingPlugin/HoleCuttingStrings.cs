using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.HoleCuttingPlugin
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class HoleCuttingStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Cut an opening", "Вырезать отверстие");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Cuts a rectangular opening in a wall where a service crosses it",
                "Cuts a rectangular opening in a wall where a service crosses it");

        internal static readonly LocalizedText SelectionLine =
            new LocalizedText("Wall and service from the current selection", "Стена и коммуникация из текущего выделения");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Cut an opening", "Вырезание отверстия");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText(
                "Select one wall and one duct, pipe, cable tray or conduit.",
                "Выберите одну стену и одну коммуникацию: воздуховод, трубу, кабельный лоток или кабелепровод.");

        internal static readonly LocalizedText WrongSelection =
            new LocalizedText(
                "The selection must hold one wall and one service element — a duct, a pipe, a cable tray or a conduit. Found: {0}.",
                "В выделении должны быть одна стена и одна коммуникация — воздуховод, труба, кабельный лоток или кабелепровод. Найдено: {0}.");

        internal static readonly LocalizedText FoundCounts =
            new LocalizedText(
                "walls: {0}, services: {1}, other elements: {2}",
                "стен: {0}, коммуникаций: {1}, прочих элементов: {2}");

        internal static readonly LocalizedText CurvedWall =
            new LocalizedText(
                "Only straight walls are supported; the selected wall is curved.",
                "Поддерживаются только прямые стены; выбранная стена криволинейна.");

        internal static readonly LocalizedText NoCrossing =
            new LocalizedText(
                "The service does not cross the wall.",
                "Коммуникация не пересекает стену.");

        internal static readonly LocalizedText TooSmall =
            new LocalizedText(
                "The crossing is too small to cut an opening in.",
                "Пересечение слишком мало для отверстия.");

        internal static readonly LocalizedText CutFailed =
            new LocalizedText(
                "The opening could not be cut: {0}",
                "Не удалось вырезать отверстие: {0}");

        internal static readonly LocalizedText Success =
            new LocalizedText(
                "Opening {0} × {1} mm cut in wall '{2}' (crossing {3} × {4} mm).",
                "Отверстие {0} × {1} мм вырезано в стене «{2}» (пересечение {3} × {4} мм).");

        internal static readonly LocalizedText LogFolder =
            new LocalizedText("Log folder: {0}", "Папка журнала: {0}");
    }
}
