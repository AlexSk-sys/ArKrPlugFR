using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class CompareStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Compare and copy element parameters", "Сравнить и перенести параметры элементов");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Lists the parameters of two elements and copies the values of the source into the target",
                "Lists the parameters of two elements and copies the values of the source into the target");

        internal static readonly LocalizedText PickSource =
            new LocalizedText("Pick the source element", "Выбрать элемент-источник");

        internal static readonly LocalizedText PickTarget =
            new LocalizedText("Pick the target element", "Выбрать элемент-приёмник");

        internal static readonly LocalizedText PickSourcePrompt =
            new LocalizedText(
                "Pick the element that carries the values worth keeping",
                "Выберите элемент, значения которого нужно сохранить");

        internal static readonly LocalizedText PickTargetPrompt =
            new LocalizedText(
                "Pick the element to fix",
                "Выберите элемент, который нужно исправить");

        internal static readonly LocalizedText PickCancelled =
            new LocalizedText("The selection was cancelled.", "Выбор отменён.");

        internal static readonly LocalizedText SourceLabel =
            new LocalizedText("Source", "Источник");

        internal static readonly LocalizedText TargetLabel =
            new LocalizedText("Target", "Приёмник");

        internal static readonly LocalizedText NotPicked =
            new LocalizedText("not picked", "не выбран");

        internal static readonly LocalizedText ComparisonCaption =
            new LocalizedText("Comparison", "Сравнение");

        internal static readonly LocalizedText ComparisonLine =
            new LocalizedText("Rows: {0}; different: {1}", "Строк: {0}; различий: {1}");

        internal static readonly LocalizedText Copy =
            new LocalizedText("Copy values to the target", "Перенести значения в приёмник");

        internal static readonly LocalizedText Hint =
            new LocalizedText(
                "The comparison shows the instance parameters and the parameters of the types; empty source values are skipped, the tool fills and never clears. A parameter of a type is shared by every element of that type.",
                "Сравнение показывает параметры экземпляра и параметры типов; пустые значения источника пропускаются — инструмент только заполняет, никогда не очищает. Параметр типа общий для всех элементов этого типа.");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Copy element parameters", "Перенос параметров элемента");

        internal static readonly LocalizedText NoPicks =
            new LocalizedText(
                "Pick the source element and the target element.",
                "Выберите элемент-источник и элемент-приёмник.");

        internal static readonly LocalizedText NothingToCopy =
            new LocalizedText(
                "The source carries no value the target is missing.",
                "У источника нет значений, которых не хватает приёмнику.");

        internal static readonly LocalizedText NoTarget =
            new LocalizedText(
                "{0}: the value could not be written.",
                "{0}: значение не удалось записать.");

        internal static readonly LocalizedText WriteFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Parameters compared: {0}; different: {1}; copied: {2}; skipped: {3}; refused: {4}.",
                "Параметров сравнено: {0}; различий: {1}; перенесено: {2}; пропущено: {3}; отказов: {4}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");
    }
}
