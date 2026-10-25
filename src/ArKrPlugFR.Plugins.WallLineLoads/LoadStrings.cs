using ArKrPlugFR.Core.Localization;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// Every user-facing string of the tool, in English and Russian.
    /// </summary>
    internal static class LoadStrings
    {
        internal static readonly LocalizedText WindowTitle =
            new LocalizedText("Line loads on walls", "Линейные нагрузки на стены");

        // The subtitle under the caption is English in both languages on purpose:
        // it names the tool for anyone reading the Russian window.
        internal static readonly LocalizedText Subtitle =
            new LocalizedText(
                "Puts the weight of the walls onto the slabs at the bottom of every wall",
                "Puts the weight of the walls onto the slabs at the bottom of every wall");

        internal static readonly LocalizedText ScopeLabel =
            new LocalizedText("Walls to load", "Какие стены нагружать");

        internal static readonly LocalizedText ScopeSelected =
            new LocalizedText("Walls selected in the model", "Выделенные стены");

        internal static readonly LocalizedText ScopeView =
            new LocalizedText("All walls on the active view", "Все стены на виде");

        internal static readonly LocalizedText ScopeWorkset =
            new LocalizedText("Walls of one workset", "Стены одного рабочего набора");

        internal static readonly LocalizedText WorksetLabel =
            new LocalizedText("Workset", "Рабочий набор");

        internal static readonly LocalizedText CaseLabel =
            new LocalizedText("Load case", "Вариант нагружений");

        internal static readonly LocalizedText CaseHint =
            new LocalizedText(
                "A name that does not exist yet is created by the run, as a dead load.",
                "Отсутствующее название будет создано при выполнении, как постоянная нагрузка.");

        internal static readonly LocalizedText ForceLabel =
            new LocalizedText("Fz, kN/m", "Fz, кН/м");

        internal static readonly LocalizedText MomentLabel =
            new LocalizedText("Mz, kN·m/m", "Mz, кН·м/м");

        internal static readonly LocalizedText UniformLabel =
            new LocalizedText("Uniform along the wall", "Распределённая по длине стены");

        internal static readonly LocalizedText CommentLabel =
            new LocalizedText("Comment", "Комментарий");

        internal static readonly LocalizedText Apply =
            new LocalizedText("Create loads", "Создать нагрузки");

        internal static readonly LocalizedText Close =
            new LocalizedText("Close", "Закрыть");

        internal static readonly LocalizedText ReportCaption =
            new LocalizedText("Result", "Результат");

        internal static readonly LocalizedText TransactionName =
            new LocalizedText("Line loads on walls", "Линейные нагрузки на стены");

        internal static readonly LocalizedText NoForce =
            new LocalizedText(
                "The force and the moment are both zero: the platform refuses such a load.",
                "Сила и момент равны нулю: платформа такую нагрузку не принимает.");

        internal static readonly LocalizedText NoWalls =
            new LocalizedText(
                "The chosen scope holds no walls.",
                "В выбранной области нет стен.");

        internal static readonly LocalizedText NoStraightWalls =
            new LocalizedText(
                "None of the walls has a straight location line.",
                "Ни у одной стены нет прямой линии расположения.");

        internal static readonly LocalizedText NoWorkset =
            new LocalizedText(
                "The workset «{0}» was not found; choose one from the list.",
                "Рабочий набор «{0}» не найден; выберите его из списка.");

        internal static readonly LocalizedText NotAWorksharedModel =
            new LocalizedText(
                "The project does not use worksets, so the workset scope is not available.",
                "Проект не использует рабочие наборы, поэтому выбор по набору недоступен.");

        internal static readonly LocalizedText NoAnalyticalHost =
            new LocalizedText(
                "On this release a load needs the wall's analytical model, and {0} has none.",
                "В этой версии для нагрузки нужна аналитическая модель стены, а у {0} её нет.");

        internal static readonly LocalizedText NotStraight =
            new LocalizedText(
                "{0} was skipped: it has no straight location line.",
                "{0} пропущена: нет прямой линии расположения.");

        internal static readonly LocalizedText Refused =
            new LocalizedText(
                "{0} was refused: {1}",
                "{0} отклонена: {1}");

        internal static readonly LocalizedText Result =
            new LocalizedText(
                "Created {0} line load(s) from {1} wall(s); skipped: {2}; refused: {3}; load case «{4}», Fz = {5} kN/m, Mz = {6} kN·m/m.",
                "Создано {0} линейных нагрузок из {1} стен; пропущено: {2}; отказов: {3}; вариант нагружений «{4}», Fz = {5} кН/м, Mz = {6} кН·м/м.");

        internal static readonly LocalizedText HostedNote =
            new LocalizedText(
                "On this release the platform created the loads on the walls' analytical elements and then released them; a load that stayed on its host is listed below.",
                "В этой версии платформа создала нагрузки на аналитических элементах стен и затем освободила их; оставшиеся на основе перечислены ниже.");

        internal static readonly LocalizedText CaseCreated =
            new LocalizedText(
                "The load case «{0}» did not exist and was created.",
                "Вариант нагружений «{0}» отсутствовал и был создан.");
    }
}
