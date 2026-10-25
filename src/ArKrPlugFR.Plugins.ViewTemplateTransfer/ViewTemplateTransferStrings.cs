using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class ViewTemplateTransferStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("View template transfer", "Перенос шаблонов видов");

        internal static readonly LocalizedText TargetLine =
            new LocalizedText("Target project: {0}", "Целевой проект: {0}");

        internal static readonly LocalizedText SourceLine =
            new LocalizedText("Source project: {0}", "Исходный проект: {0}");

        internal static readonly LocalizedText TargetsHeader =
            new LocalizedText("Target project", "Проект-приёмник");

        internal static readonly LocalizedText NoTargetsHint =
            new LocalizedText(
                "No other project is open in Revit. Open the project the views should move into, then run the tool again.",
                "В Revit не открыт ни один другой проект. Откройте проект, в который нужно перенести виды, и запустите инструмент снова.");

        internal static readonly LocalizedText NoTargetChosen =
            new LocalizedText(
                "Pick the project the views are copied into.",
                "Выберите проект, в который копируются виды.");

        internal static readonly LocalizedText NeverOverwritten =
            new LocalizedText(
                "existing views are never overwritten",
                "существующие виды не перезаписываются");

        internal static readonly LocalizedText Transfer =
            new LocalizedText("Transfer", "Перенос");

        internal static readonly LocalizedText TransferWithCount =
            new LocalizedText("Transfer ({0})", "Перенос ({0})");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText Help =
            new LocalizedText("Help", "Справка");

        internal static readonly LocalizedText HelpTitle =
            new LocalizedText("How the tool works", "Как работает инструмент");

        internal static readonly LocalizedText HelpText = new LocalizedText(
            "1. The tool lists the views of the project you started it from; tick the ones you need.\r\n" +
            "2. Ticking a group ticks everything in it; Shift+click selects a range.\r\n" +
            "3. Pick the project the views should move into — it must be open beside this one.\r\n" +
            "4. Press Transfer. The copies land in the chosen project, in one undoable step.\r\n" +
            "Existing views are never overwritten: a copy whose name is taken gets a numbered suffix.",
            "1. Инструмент показывает виды проекта, из которого он запущен; отметьте нужные.\r\n" +
            "2. Щелчок по группе отмечает всё в ней; Shift+щелчок выделяет диапазон.\r\n" +
            "3. Выберите проект, в который нужно перенести виды — он должен быть открыт рядом с этим.\r\n" +
            "4. Нажмите «Перенос». Копии появятся в выбранном проекте одним шагом отмены.\r\n" +
            "Существующие виды не перезаписываются: копия с занятым именем получает номерной суффикс.");

        internal static readonly LocalizedText TreeHint =
            new LocalizedText(
                "Click a row to tick it. Shift+click selects a range of views; group rows are skipped.",
                "Щелчок по строке отмечает её. Shift+щелчок выделяет диапазон видов; строки групп пропускаются.");

        internal static readonly LocalizedText NoSelection =
            new LocalizedText("No templates or views are selected.", "Шаблоны и виды не выбраны.");

        internal static readonly LocalizedText TargetLost =
            new LocalizedText(
                "The target project is no longer open.",
                "Целевой проект больше не открыт.");

        internal static readonly LocalizedText TargetReadOnly =
            new LocalizedText(
                "The target project is read-only; nothing can be transferred into it.",
                "Целевой проект доступен только для чтения; перенос в него невозможен.");

        internal static readonly LocalizedText EmptyTree =
            new LocalizedText(
                "The source project has nothing that can be transferred.",
                "В исходном проекте нет ничего, что можно перенести.");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Transfer view templates", "Перенос шаблонов видов");

        internal static readonly LocalizedText ResultCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText SuccessLine =
            new LocalizedText("Transferred: {0}", "Перенесено: {0}");

        internal static readonly LocalizedText FailureLine =
            new LocalizedText("Could not transfer '{0}': {1}", "Не удалось перенести «{0}»: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText RolledBackLine =
            new LocalizedText(
                "Nothing was transferred: at least one view could not be copied, so the whole step was undone. The reasons are listed below.",
                "Ничего не перенесено: один или несколько видов скопировать не удалось, поэтому весь шаг был отменён. Причины перечислены ниже.");

        internal static readonly LocalizedText GroupPlans =
            new LocalizedText("Plans", "Планы");

        internal static readonly LocalizedText GroupCeilingPlans =
            new LocalizedText("Ceiling plans", "Потолочные планы");

        internal static readonly LocalizedText GroupThreeDimensional =
            new LocalizedText("3D views", "3D-виды");

        internal static readonly LocalizedText GroupSections =
            new LocalizedText("Sections and elevations", "Разрезы и виды");

        internal static readonly LocalizedText GroupDrafting =
            new LocalizedText("Drafting views", "Оформление");

        internal static readonly LocalizedText GroupEngineering =
            new LocalizedText("Engineering plans", "Инженерные планы");

        internal static readonly LocalizedText GroupSchedules =
            new LocalizedText("Schedules", "Ведомости");
    }
}
