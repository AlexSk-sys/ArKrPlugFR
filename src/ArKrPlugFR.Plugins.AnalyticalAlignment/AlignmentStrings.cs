using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class AlignmentStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Align the analytical model", "Выравнивание аналитической модели");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Brings analytical members to a target height and analytical nodes onto a target line",
                "Brings analytical members to a target height and analytical nodes onto a target line");

        internal static readonly LocalizedText TargetLabel =
            new LocalizedText("Target element", "Целевой элемент");

        internal static readonly LocalizedText PickTarget =
            new LocalizedText("Pick the target", "Выбрать целевой элемент");

        internal static readonly LocalizedText MembersLabel =
            new LocalizedText("Elements to move", "Элементы для выравнивания");

        internal static readonly LocalizedText PickMembers =
            new LocalizedText("Pick elements", "Выбрать элементы");

        internal static readonly LocalizedText NodesLabel =
            new LocalizedText("Nodes to move", "Узлы для выравнивания");

        internal static readonly LocalizedText PickNodes =
            new LocalizedText("Pick nodes", "Выбрать узлы");

        internal static readonly LocalizedText AlignMembers =
            new LocalizedText("Align members", "Выровнять элементы");

        internal static readonly LocalizedText AlignNodes =
            new LocalizedText("Align nodes", "Выровнять узлы");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText MembersTransaction =
            new LocalizedText("Align analytical members", "Выравнивание аналитических элементов");

        internal static readonly LocalizedText NodesTransaction =
            new LocalizedText("Align analytical nodes", "Выравнивание аналитических узлов");

        internal static readonly LocalizedText TargetNone =
            new LocalizedText("Target: none picked", "Цель: не выбрана");

        internal static readonly LocalizedText MembersNone =
            new LocalizedText("Elements: none picked", "Элементы: не выбраны");

        internal static readonly LocalizedText NodesNone =
            new LocalizedText("Nodes: none picked", "Узлы: не выбраны");

        internal static readonly LocalizedText PickTargetPrompt =
            new LocalizedText("Pick the target element", "Укажите целевой элемент");

        internal static readonly LocalizedText PickMembersPrompt =
            new LocalizedText(
                "Pick the elements to move, then press Finish",
                "Укажите элементы для выравнивания и нажмите «Готово»");

        internal static readonly LocalizedText PickNodesPrompt =
            new LocalizedText(
                "Pick the nodes to move, then press Finish",
                "Укажите узлы для выравнивания и нажмите «Готово»");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText(
                "The pick was cancelled; the previous choice is kept.",
                "Выбор отменён; прежний выбор сохранён.");

        internal static readonly LocalizedText NoTarget =
            new LocalizedText("Pick the target element first.", "Сначала укажите целевой элемент.");

        internal static readonly LocalizedText NoMembers =
            new LocalizedText("Pick the elements to move first.", "Сначала укажите элементы для выравнивания.");

        internal static readonly LocalizedText NoNodes =
            new LocalizedText("Pick the nodes to move first.", "Сначала укажите узлы для выравнивания.");

        internal static readonly LocalizedText NoTargetLine =
            new LocalizedText(
                "The target element has no location line.",
                "У целевого элемента нет линии расположения.");

        internal static readonly LocalizedText MembersResult =
            new LocalizedText(
                "Moved: {0}; already in place: {1}; skipped: {2}; refused: {3}.",
                "Перемещено: {0}; уже на месте: {1}; пропущено: {2}; отказов: {3}.");

        internal static readonly LocalizedText NodesResult =
            new LocalizedText(
                "Moved: {0}; already in place: {1}; skipped: {2}; refused: {3}; member ends dragged: {4}.",
                "Перемещено: {0}; уже на месте: {1}; пропущено: {2}; отказов: {3}; подтянуто концов стержней: {4}.");

        internal static readonly LocalizedText NothingMoved =
            new LocalizedText(
                "Nothing was moved: everything is already in line with the target.",
                "Ничего не перемещено: всё уже выровнено по цели.");

        internal static readonly LocalizedText SkippedNoLine =
            new LocalizedText(
                "{0} was skipped: it has no location line.",
                "{0} пропущен: нет линии расположения.");

        internal static readonly LocalizedText SkippedNotANode =
            new LocalizedText(
                "{0} was skipped: it is not a node.",
                "{0} пропущен: это не узел.");

        internal static readonly LocalizedText Refused =
            new LocalizedText("{0} was refused: {1}", "{0} — отказ: {1}");
    }
}
