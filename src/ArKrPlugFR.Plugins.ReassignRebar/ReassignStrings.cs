using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.ReassignRebar
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class ReassignStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Reassign rebar", "Переназначить арматуру");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Attaches the picked rebar to the structure it actually belongs to",
                "Attaches the picked rebar to the structure it actually belongs to");

        internal static readonly LocalizedText PickBars =
            new LocalizedText(
                "Step 1 of 2: pick the rebar to reassign",
                "Шаг 1 из 2: выберите арматуру для переназначения");

        internal static readonly LocalizedText PickHost =
            new LocalizedText(
                "Step 2 of 2: pick the column, wall, beam or floor to attach the rebar to",
                "Шаг 2 из 2: выберите колонну, стену, балку или перекрытие, к которым привязать арматуру");

        internal static readonly LocalizedText Cancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText NothingPicked =
            new LocalizedText(
                "No rebar was picked.",
                "Арматура не выбрана.");

        internal static readonly LocalizedText HostWithoutMark =
            new LocalizedText(
                "The picked structure has no mark; fill it in before running the tool.",
                "У выбранной конструкции не заполнена марка; заполните её и повторите запуск.");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Reassign rebar", "Переназначение арматуры");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Reassigned: {0}; IFC marks updated: {1}; skipped: {2}; refused: {3}.",
                "Переназначено: {0}; обновлено марок IFC: {1}; пропущено: {2}; отказов: {3}.");

        internal static readonly LocalizedText Refused =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
