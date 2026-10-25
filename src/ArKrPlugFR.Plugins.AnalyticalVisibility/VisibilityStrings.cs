using ArKrPlugFR.Core.Localization;
using ArKrPlugFR.Core.Visibility;

namespace ArKrPlugFR.Plugins.AnalyticalVisibility
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class VisibilityStrings
    {
        internal static readonly LocalizedText HideTitle =
            new LocalizedText("Hide the analytical model", "Скрыть аналитическую модель");

        internal static readonly LocalizedText ShowTitle =
            new LocalizedText("Show the analytical model", "Показать аналитическую модель");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Hides or shows the analytical model in the views without deleting anything",
                "Hides or shows the analytical model in the views without deleting anything");

        internal static readonly LocalizedText HideTransaction =
            new LocalizedText("Hide the analytical model", "Скрытие аналитической модели");

        internal static readonly LocalizedText ShowTransaction =
            new LocalizedText("Show the analytical model", "Показ аналитической модели");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText NoAnalyticalModel =
            new LocalizedText(
                "The project holds no analytical model.",
                "В проекте нет аналитической модели.");

        internal static readonly LocalizedText NoViews =
            new LocalizedText(
                "The project has no views that show the model.",
                "В проекте нет видов, показывающих модель.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Analytical elements: {0}; views: {1}; changed: {2}; already in place: {3}; cannot be changed: {4}; refused: {5}.",
                "Аналитических элементов: {0}; видов: {1}; изменено: {2}; уже в нужном состоянии: {3}; нельзя изменить: {4}; отказов: {5}.");

        internal static readonly LocalizedText NothingToDo =
            new LocalizedText(
                "Everything is already in the state the command asked for.",
                "Всё уже находится в нужном состоянии.");

        internal static readonly LocalizedText Safe =
            new LocalizedText(
                "Nothing was deleted: the analytical model, its loads and its boundary conditions stay in the project.",
                "Ничего не удалено: аналитическая модель, её нагрузки и граничные условия остаются в проекте.");

        internal static readonly LocalizedText Refused =
            new LocalizedText(
                "{0} in «{1}» was refused: {2}",
                "{0} на виде «{1}» — отказ: {2}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
