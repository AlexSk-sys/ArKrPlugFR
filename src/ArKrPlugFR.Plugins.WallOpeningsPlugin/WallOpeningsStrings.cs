using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class WallOpeningsStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Wall openings", "Проёмы в стенах");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Cuts openings in the finish walls opposite the doors and windows of the walls you select",
                "Cuts openings in the finish walls opposite the doors and windows of the walls you select");

        internal static readonly LocalizedText DoorsOption =
            new LocalizedText("Doors", "Двери");

        internal static readonly LocalizedText WindowsOption =
            new LocalizedText("Windows", "Окна");

        internal static readonly LocalizedText OptionsHint =
            new LocalizedText(
                "The switches are remembered for the next run. A finish wall is a wall running beside the selected one, within 150 mm of it.",
                "Переключатели запоминаются для следующего запуска. Отделочной считается стена, идущая рядом с выбранной, в пределах 150 мм от неё.");

        internal static readonly LocalizedText SelectionLine =
            new LocalizedText("Walls in the selection: {0}", "Стен в выделении: {0}");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Apply", "Применить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Cut wall openings", "Вырезание проёмов");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText(
                "Select at least one wall that carries doors or windows.",
                "Выберите хотя бы одну стену с дверями или окнами.");

        internal static readonly LocalizedText NoOpenings =
            new LocalizedText(
                "The selected walls carry no doors or windows matching the chosen options.",
                "На выбранных стенах нет дверей или окон, подходящих под выбранные параметры.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Openings cut: {0}. Already there: {1}. Skipped: {2}.",
                "Проёмов вырезано: {0}. Уже было: {1}. Пропущено: {2}.");

        internal static readonly LocalizedText OpeningFailed =
            new LocalizedText("Wall '{0}': {1}", "Стена «{0}»: {1}");

        internal static readonly LocalizedText WallNotStraight =
            new LocalizedText(
                "Wall '{0}' is curved, so its doors and windows were skipped.",
                "Стена «{0}» криволинейна, поэтому её двери и окна пропущены.");

        internal static readonly LocalizedText OpeningNotRead =
            new LocalizedText(
                "'{0}' could not be measured: {1}",
                "Не удалось определить размер «{0}»: {1}");

        internal static readonly LocalizedText NoSize =
            new LocalizedText(
                "its width and height could not be read",
                "его ширина и высота не читаются");

        internal static readonly LocalizedText RunFailed =
            new LocalizedText("The run failed: {0}", "Прогон не удался: {0}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText LogFolder =
            new LocalizedText("Log folder: {0}", "Папка журнала: {0}");
    }
}
