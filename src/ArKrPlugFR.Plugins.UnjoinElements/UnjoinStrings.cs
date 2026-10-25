using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// <para>
    /// The strings are typed values rather than keys in a table, so a tool that
    /// forgets a translation fails to compile instead of showing an empty label.
    /// </para>
    /// </summary>
    internal static class UnjoinStrings
    {
        internal static readonly LocalizedText ColumnsTitle =
            new LocalizedText("Unjoin columns from walls", "Отсоединить колонны");

        internal static readonly LocalizedText BeamsTitle =
            new LocalizedText("Unjoin beams from walls", "Отсоединить балки");

        internal static readonly LocalizedText CutsCaption =
            new LocalizedText("Cuts", "Вырезы");

        internal static readonly LocalizedText NoSelectionTitle =
            new LocalizedText("Nothing is selected", "Ничего не выделено");

        internal static readonly LocalizedText NoSelectionHint =
            new LocalizedText(
                "Select the walls you want to clean up and run the tool again.",
                "Выделите стены, которые нужно очистить, и запустите инструмент снова.");

        internal static readonly LocalizedText NoWallsTitle =
            new LocalizedText("No walls in the selection", "Среди выделенных элементов нет стен");

        internal static readonly LocalizedText NoWallsHint =
            new LocalizedText(
                "Select walls, or a group that contains walls. Columns and beams are searched across the whole project.",
                "Выделите стены или группу, внутри которой есть стены. Колонны и балки ищутся по всему проекту.");

        internal static readonly LocalizedText NothingToDoTitle =
            new LocalizedText("Unjoin is not required", "Отсоединение не требуется");

        internal static readonly LocalizedText NothingToDoMessage =
            new LocalizedText(
                "No cut made by {0} was found in the selected walls.",
                "Вырезов {0} в выбранных стенах не найдено.");

        internal static readonly LocalizedText CompletedTitle =
            new LocalizedText("Unjoined successfully", "Отсоединено успешно");

        internal static readonly LocalizedText WallsLine =
            new LocalizedText("Selected walls: {0}", "Выбрано стен: {0}");

        internal static readonly LocalizedText FoundLine =
            new LocalizedText("Cuts found: {0}", "Найдено вырезов: {0}");

        internal static readonly LocalizedText RemovedLine =
            new LocalizedText("Cuts removed: {0}", "Убрано вырезов: {0}");

        internal static readonly LocalizedText FailedLine =
            new LocalizedText("Could not change: {0}", "Не удалось изменить: {0}");

        internal static readonly LocalizedText ColumnsNoun =
            new LocalizedText("columns", "колонн");

        internal static readonly LocalizedText BeamsNoun =
            new LocalizedText("beams", "балок");
    }
}
