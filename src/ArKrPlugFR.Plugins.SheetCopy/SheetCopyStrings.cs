using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class SheetCopyStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Copy sheets", "Копирование листов");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Copy the sheets of this project into other open Revit projects",
                "Copy the sheets of this project into other open Revit projects");

        internal static readonly LocalizedText SourceLine =
            new LocalizedText("Source: {0}", "Источник: {0}");

        internal static readonly LocalizedText TargetsHeader =
            new LocalizedText("Target projects", "Проекты-приёмники");

        internal static readonly LocalizedText TargetsHint =
            new LocalizedText(
                "Tick the projects the sheets should move into. Each of them receives its own numbered copies.",
                "Отметьте проекты, в которые нужно перенести листы. Каждый из них получит свои копии с номерами.");

        internal static readonly LocalizedText NoTargetsHint =
            new LocalizedText(
                "No other project is open in Revit. Open the projects the sheets should move into, then run the tool again.",
                "В Revit не открыт ни один другой проект. Откройте проекты, в которые нужно перенести листы, и запустите инструмент снова.");

        internal static readonly LocalizedText TargetUnsaved =
            new LocalizedText("unsaved project", "несохранённый проект");

        internal static readonly LocalizedText TargetFamily =
            new LocalizedText("family document", "документ семейства");

        internal static readonly LocalizedText TargetLinked =
            new LocalizedText("linked model", "связанная модель");

        internal static readonly LocalizedText TargetReadOnly =
            new LocalizedText("read-only", "только для чтения");

        internal static readonly LocalizedText TargetLost =
            new LocalizedText(
                "the project is no longer open",
                "проект больше не открыт");

        internal static readonly LocalizedText SheetsHeader =
            new LocalizedText("Sheets of this project", "Листы этого проекта");

        internal static readonly LocalizedText SelectHint =
            new LocalizedText(
                "Tick the sheets that should move into the ticked projects; every target receives its own copies with free sheet numbers.",
                "Отметьте листы, которые нужно перенести в отмеченные проекты; каждый приёмник получит свои копии со свободными номерами.");

        internal static readonly LocalizedText EmptyHint =
            new LocalizedText(
                "This project contains no sheets to copy.",
                "В этом проекте нет листов для копирования.");

        internal static readonly LocalizedText SelectAll =
            new LocalizedText("Select all", "Выбрать все");

        internal static readonly LocalizedText ClearSelection =
            new LocalizedText("Clear", "Снять выделение");

        internal static readonly LocalizedText Copy =
            new LocalizedText("Copy", "Копировать");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Copy sheets", "Копирование листов");

        internal static readonly LocalizedText NothingSelected =
            new LocalizedText("Select at least one sheet to copy.", "Выберите хотя бы один лист для копирования.");

        internal static readonly LocalizedText NoTargetSelected =
            new LocalizedText("Tick at least one target project.", "Отметьте хотя бы один проект-приёмник.");

        internal static readonly LocalizedText LogFolder =
            new LocalizedText("Log folder: {0}", "Папка журнала: {0}");

        internal static readonly LocalizedText GeneralFailure =
            new LocalizedText("Failed to complete the operation: {0}", "Не удалось выполнить операцию: {0}");

        internal static readonly LocalizedText TargetSummary =
            new LocalizedText(
                "Copied {0} of {1} planned sheets into '{2}'.",
                "Скопировано листов: {0} из {1} в «{2}».");

        internal static readonly LocalizedText TargetFailed =
            new LocalizedText("Project '{0}': {1}", "Проект «{0}»: {1}");

        internal static readonly LocalizedText SheetFailed =
            new LocalizedText("Sheet {0}: {1}", "Лист {0}: {1}");

        internal static readonly LocalizedText MoreLine =
            new LocalizedText("… and {0} more", "… и ещё {0}");

        internal static readonly LocalizedText CopyMarker =
            new LocalizedText("(copy)", "(копия)");

        internal static readonly LocalizedText CopyReturnedNothing =
            new LocalizedText(
                "The project refused to copy this sheet.",
                "Проект отказался копировать этот лист.");

        internal static readonly LocalizedText NumberRefused =
            new LocalizedText(
                "The generated sheet number was refused by the project.",
                "Сгенерированный номер листа отклонён проектом.");
    }
}
