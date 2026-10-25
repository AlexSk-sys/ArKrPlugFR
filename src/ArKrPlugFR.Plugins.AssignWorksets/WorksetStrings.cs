using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class WorksetStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Assign elements to worksets", "Распределить элементы по рабочим наборам");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Puts model elements into worksets by ordered rules",
                "Puts model elements into worksets by ordered rules");

        internal static readonly LocalizedText RulesCaption =
            new LocalizedText(
                "Rules; the first matching rule wins",
                "Правила; срабатывает первое подходящее");

        internal static readonly LocalizedText KindCaption =
            new LocalizedText("Kind", "Вид");

        internal static readonly LocalizedText PatternCaption =
            new LocalizedText("Match", "Совпадение");

        internal static readonly LocalizedText TargetCaption =
            new LocalizedText("Workset", "Рабочий набор");

        internal static readonly LocalizedText KindCategory =
            new LocalizedText("Category", "Категория");

        internal static readonly LocalizedText KindFamilyPrefix =
            new LocalizedText("Family prefix", "Префикс семейства");

        internal static readonly LocalizedText KindTypePrefix =
            new LocalizedText("Type prefix", "Префикс типа");

        internal static readonly LocalizedText KindParameterValue =
            new LocalizedText("Parameter value", "Значение параметра");

        internal static readonly LocalizedText KindHint =
            new LocalizedText(
                "Category matches the name shown in Revit or the built-in name such as OST_Walls. " +
                "Family and type match name prefixes. Parameter takes the workset name from the value of a text " +
                "parameter; the instance parameter is read first, then the parameter of its type. " +
                "Several values are separated by commas.",
                "Категория сравнивается с именем, которое видно в Revit, или со встроенным именем, например OST_Walls. " +
                "Семейство и тип сравниваются с началом имени. Параметр берёт имя рабочего набора из значения текстового " +
                "параметра: сначала читается параметр экземпляра, затем параметр его типа. " +
                "Несколько значений перечисляются через запятую.");

        internal static readonly LocalizedText AddRule =
            new LocalizedText("Add rule", "Добавить правило");

        internal static readonly LocalizedText RemoveRule =
            new LocalizedText("Remove", "Убрать");

        internal static readonly LocalizedText CreateMissing =
            new LocalizedText(
                "Create worksets that do not exist yet",
                "Создавать отсутствующие рабочие наборы");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Assign", "Распределить");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Assign elements to worksets", "Распределение по рабочим наборам");

        internal static readonly LocalizedText NotWorkshared =
            new LocalizedText(
                "This project is not workshared; the tool needs a project with worksharing enabled.",
                "Проект не переведён на совместную работу; инструменту нужен проект с включённой совместной работой.");

        internal static readonly LocalizedText NoRules =
            new LocalizedText(
                "Add at least one rule: the match must not be empty, and a workset is needed everywhere except in a rule that reads it from a parameter.",
                "Добавьте хотя бы одно правило: совпадение не должно быть пустым, а рабочий набор нужен везде, кроме правила, которое берёт его из параметра.");

        internal static readonly LocalizedText CreatedLine =
            new LocalizedText("Worksets created: {0}.", "Создано рабочих наборов: {0}.");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Read {0} element(s); assigned: {1}; already in place: {2}; no matching rule: {3}; workset missing: {4}; refused: {5}.",
                "Прочитано элементов: {0}; распределено: {1}; уже стояли в своих наборах: {2}; без подходящего правила: {3}; набор отсутствует: {4}; отказов: {5}.");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("and {0} more.", "и ещё {0}.");

        internal static readonly LocalizedText AssignmentFailed =
            new LocalizedText("{0}: {1}", "{0}: {1}");

        internal static readonly LocalizedText AssignmentRefused =
            new LocalizedText(
                "{0}: the platform refused to move the element to this workset.",
                "{0}: платформа отказалась перенести элемент в этот рабочий набор.");

        internal static readonly LocalizedText CreateFailed =
            new LocalizedText("Workset '{0}': {1}", "Рабочий набор '{0}': {1}");

        internal static readonly LocalizedText NotUnique =
            new LocalizedText(
                "Workset '{0}': the name is already taken but the workset could not be resolved; skipped.",
                "Рабочий набор '{0}': имя занято, но сам набор не найден; пропущен.");
    }
}
