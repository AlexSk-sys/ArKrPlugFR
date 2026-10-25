using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class DuplicateViewsStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Duplicate views", "Дубликация видов");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Copy views, schedules and view templates in one action",
                "Copy views, schedules and view templates in one action");

        internal static readonly LocalizedText ViewsHeader =
            new LocalizedText("Views", "Виды");

        internal static readonly LocalizedText SchedulesHeader =
            new LocalizedText("Schedules", "Ведомости");

        internal static readonly LocalizedText TemplatesHeader =
            new LocalizedText("View templates", "Шаблоны видов");

        internal static readonly LocalizedText SearchHint =
            new LocalizedText("Search by name", "Поиск по имени");

        internal static readonly LocalizedText TreeHint =
            new LocalizedText(
                "Tick a row to copy it and set how many copies it gets; Shift+click selects a range; Escape clears the ticks.",
                "Отметьте строку для копирования и укажите число копий; Shift+щелчок выделяет диапазон; Escape снимает отметки.");

        internal static readonly LocalizedText TemplateSection =
            new LocalizedText("Template settings", "Настройки шаблона");

        internal static readonly LocalizedText TemplateSearchHint =
            new LocalizedText("Search templates", "Поиск шаблонов");

        internal static readonly LocalizedText NoTemplateOption =
            new LocalizedText("No template", "Без шаблона");

        internal static readonly LocalizedText TemplateHint =
            new LocalizedText(
                "The chosen template is applied to every created view. Schedules and the copies of templates themselves are left alone.",
                "Выбранный шаблон назначается каждой созданной копии вида. Ведомости и копии самих шаблонов остаются без шаблона.");

        internal static readonly LocalizedText Duplicate =
            new LocalizedText("Duplicate", "Дублировать");

        internal static readonly LocalizedText DuplicateWithCount =
            new LocalizedText("Duplicate ({0})", "Дублировать ({0})");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText(
                "Nothing selected to copy. Tick at least one item in the tree.",
                "Ничего не выбрано для копирования. Отметьте хотя бы одну строку в дереве.");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Duplicate views", "Дубликация видов");

        internal static readonly LocalizedText Success =
            new LocalizedText("Copies created: {0}.", "Создано копий: {0}.");

        internal static readonly LocalizedText FailedLine =
            new LocalizedText("Could not copy '{0}': {1}", "Не удалось скопировать «{0}»: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText TemplateMismatch =
            new LocalizedText(
                "The chosen template does not fit the ticked items. Choose another template or the 'No template' entry.",
                "Выбранный шаблон не подходит к отмеченным строкам. Выберите другой шаблон или пункт «Без шаблона».");

        internal static readonly LocalizedText LogFolder =
            new LocalizedText("Log folder: {0}", "Папка журнала: {0}");

        internal static readonly LocalizedText Help =
            new LocalizedText("Help", "Справка");

        internal static readonly LocalizedText HelpTitle =
            new LocalizedText("How the tool works", "Как работает инструмент");

        internal static readonly LocalizedText HelpText = new LocalizedText(
            "1. The window lists the views, the schedules and the view templates of the project, each grouped by kind.\r\n" +
            "2. Tick the rows you need and set how many copies each row gets (one to ninety-nine).\r\n" +
            "3. Optionally pick one view template to apply to every created view; schedules and templates ignore it.\r\n" +
            "4. Press Duplicate. All copies are created in one undoable step.\r\n" +
            "Copies are named after their source with an underscore and a number; the source elements are never renamed.",
            "1. Окно показывает виды, ведомости и шаблоны видов проекта, сгруппированные по типу.\r\n" +
            "2. Отметьте нужные строки и укажите число копий для каждой (от одной до девяноста девяти).\r\n" +
            "3. При желании выберите шаблон вида, который будет назначен каждой созданной копии вида; ведомости и шаблоны его не получают.\r\n" +
            "4. Нажмите «Дублировать». Все копии создаются одним шагом отмены.\r\n" +
            "Копии получают имя источника с подчёркиванием и номером; сами исходные элементы не переименовываются.");

        internal static readonly LocalizedText ElementMissing =
            new LocalizedText(
                "the element is no longer in the project",
                "элемента больше нет в проекте");

        internal static readonly LocalizedText CopyRefused =
            new LocalizedText(
                "the project refused to copy this element",
                "проект отказался копировать этот элемент");

        internal static readonly LocalizedText GroupThreeDimensional =
            new LocalizedText("3D views and walkthroughs", "3D-виды и обходы");

        internal static readonly LocalizedText GroupPlans =
            new LocalizedText("Plans", "Планы");

        internal static readonly LocalizedText GroupSections =
            new LocalizedText("Elevations, sections and details", "Фасады, разрезы и узлы");

        internal static readonly LocalizedText GroupDrafting =
            new LocalizedText("Drafting views", "Оформление");

        internal static readonly LocalizedText GroupLegends =
            new LocalizedText("Legends", "Легенды");

        internal static readonly LocalizedText GroupOtherViews =
            new LocalizedText("Other views", "Прочие виды");

        internal static readonly LocalizedText GroupCategorySchedulesOf =
            new LocalizedText("Schedules of '{0}'", "Ведомости категории «{0}»");

        internal static readonly LocalizedText GroupOtherSchedules =
            new LocalizedText("Other schedules", "Прочие ведомости");

        internal static readonly LocalizedText GroupScheduleTemplates =
            new LocalizedText("Schedules", "Ведомости");
    }
}
