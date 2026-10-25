using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>What a sheet does with one kind of content when the sheet is copied.</summary>
    public enum SheetViewAction
    {
        /// <summary>The content is not carried over.</summary>
        None = 0,

        /// <summary>The view is duplicated and the duplicate travels to the copy.</summary>
        Duplicate = 1,

        /// <summary>The very same view is placed on the copy again.</summary>
        Reuse = 2,
    }

    /// <summary>How a duplicated view is produced.</summary>
    public enum SheetViewDuplicateOption
    {
        /// <summary>The view is not duplicated.</summary>
        None = 0,

        /// <summary>A plain copy without detailing; the option this tool always uses.</summary>
        Plain = 1,

        /// <summary>A copy that also carries detailing.</summary>
        WithDetailing = 2,

        /// <summary>A dependent copy that stays linked to the source.</summary>
        AsDependent = 3,
    }

    /// <summary>The kind of content one placed view represents on a sheet.</summary>
    public enum SheetViewKind
    {
        /// <summary>An ordinary view: plans, sections, elevations, drafting views and the like.</summary>
        View = 0,

        /// <summary>A legend, which is shared rather than copied.</summary>
        Legend = 1,

        /// <summary>A schedule placed on the sheet.</summary>
        Schedule = 2,
    }

    /// <summary>The facts of one view placed on the source sheet.</summary>
    public sealed record SourceViewFact
    {
        /// <summary>Creates the facts of one placed view.</summary>
        /// <param name="name">Name of the view in the source document.</param>
        /// <param name="kind">What the view is.</param>
        public SourceViewFact(string? name, SheetViewKind kind)
        {
            Name = name ?? string.Empty;
            Kind = kind;
        }

        /// <summary>Gets the name of the view in the source document.</summary>
        public string Name { get; }

        /// <summary>Gets what the view is.</summary>
        public SheetViewKind Kind { get; }
    }

    /// <summary>One line of the plan: what happens to one placed view on every copy.</summary>
    public sealed record PlannedView
    {
        /// <summary>Creates one planned line.</summary>
        /// <param name="name">Name of the source view.</param>
        /// <param name="kind">What the source view is.</param>
        /// <param name="action">What the copy does with it.</param>
        /// <param name="option">How a duplicated view is produced.</param>
        public PlannedView(string? name, SheetViewKind kind, SheetViewAction action, SheetViewDuplicateOption option)
        {
            Name = name ?? string.Empty;
            Kind = kind;
            Action = action;
            Option = option;
        }

        /// <summary>Gets the name of the source view.</summary>
        public string Name { get; }

        /// <summary>Gets what the source view is.</summary>
        public SheetViewKind Kind { get; }

        /// <summary>Gets what the copy does with it.</summary>
        public SheetViewAction Action { get; }

        /// <summary>Gets how a duplicated view is produced.</summary>
        public SheetViewDuplicateOption Option { get; }
    }

    /// <summary>The whole plan for one source sheet: every view and what happens to it.</summary>
    public sealed record SheetCopyPlan
    {
        /// <summary>Creates the plan.</summary>
        /// <param name="views">One planned line per placed view, in source order.</param>
        public SheetCopyPlan(IReadOnlyList<PlannedView>? views)
        {
            Views = views ?? Array.Empty<PlannedView>();
        }

        /// <summary>Gets the planned lines.</summary>
        public IReadOnlyList<PlannedView> Views { get; }
    }

    /// <summary>
    /// Decides which views travel with a sheet and how.
    /// <para>
    /// Ordinary views are duplicated without detailing, legends are shared with
    /// the copy because a legend may live on several sheets, and schedules are
    /// left where they are: this tool does not re-place schedule graphics, so a
    /// schedule is never part of a plan.
    /// </para>
    /// </summary>
    public static class SheetViewPlanner
    {
        /// <summary>Builds the plan for one source sheet.</summary>
        /// <param name="placedViews">The views the source sheet carries.</param>
        /// <param name="options">The switches of the run.</param>
        /// <returns>The plan; one line per source view, in source order.</returns>
        public static SheetCopyPlan Plan(
            IReadOnlyList<SourceViewFact>? placedViews,
            SheetCopyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var planned = new List<PlannedView>();

            if (placedViews != null)
            {
                foreach (SourceViewFact fact in placedViews)
                {
                    if (fact == null)
                    {
                        continue;
                    }

                    planned.Add(PlanOne(fact, options));
                }
            }

            return new SheetCopyPlan(planned);
        }

        private static PlannedView PlanOne(SourceViewFact fact, SheetCopyOptions options)
        {
            switch (fact.Kind)
            {
                case SheetViewKind.View:
                    return options.CopyViews
                        ? new PlannedView(fact.Name, fact.Kind, SheetViewAction.Duplicate, SheetViewDuplicateOption.Plain)
                        : new PlannedView(fact.Name, fact.Kind, SheetViewAction.None, SheetViewDuplicateOption.None);

                case SheetViewKind.Legend:
                    return options.CopyLegends
                        ? new PlannedView(fact.Name, fact.Kind, SheetViewAction.Reuse, SheetViewDuplicateOption.None)
                        : new PlannedView(fact.Name, fact.Kind, SheetViewAction.None, SheetViewDuplicateOption.None);

                default:
                    // Schedules stay on the source sheet; this tool offers no
                    // switch for them, so the plan never carries one.
                    return new PlannedView(fact.Name, fact.Kind, SheetViewAction.None, SheetViewDuplicateOption.None);
            }
        }
    }
}
