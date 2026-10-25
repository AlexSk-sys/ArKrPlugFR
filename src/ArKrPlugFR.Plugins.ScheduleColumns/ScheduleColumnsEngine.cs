using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Schedules;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>What one run changed.</summary>
    internal sealed class ColumnsOutcome
    {
        internal ColumnsOutcome(
            string? stopped,
            string schedule,
            int hidden,
            int shown,
            int left,
            IReadOnlyList<string> notes)
        {
            Stopped = stopped;
            Schedule = schedule;
            Hidden = hidden;
            Shown = shown;
            Left = left;
            Notes = notes ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal string Schedule { get; }

        internal int Hidden { get; }

        internal int Shown { get; }

        internal int Left { get; }

        internal IReadOnlyList<string> Notes { get; }
    }

    /// <summary>
    /// Hides the columns of a schedule that tell the reader nothing and shows the hidden ones
    /// that carry values.
    /// <para>
    /// The decision for a column follows <see cref="ColumnVisibility"/>, which is tested
    /// without Revit: a column whose cells are all empty or zero is hidden, a hidden column
    /// with a value is shown, everything else stays. The parameter behind a column is read
    /// from its cells, so a column without one is reported instead of guessed at. The run is
    /// one undoable step.
    /// </para>
    /// </summary>
    internal static class ScheduleColumnsEngine
    {
        /// <summary>Lists the schedules of the project, ordered by name.</summary>
        /// <param name="document">Project to read.</param>
        /// <returns>The schedules the tool can work on.</returns>
        internal static IReadOnlyList<ViewSchedule> Schedules(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(schedule =>
                    !schedule.IsTemplate &&
                    !schedule.IsTitleblockRevisionSchedule &&
                    !schedule.IsInternalKeynoteSchedule &&
                    schedule.Definition != null)
                .OrderBy(schedule => schedule.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>Runs the hiding and showing over one schedule.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="schedule">The schedule to work on.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run changed.</returns>
        internal static ColumnsOutcome Run(
            Document document,
            ViewSchedule schedule,
            ColumnText text,
            Log log)
        {
            TableData table = schedule.GetTableData();
            TableSectionData? body = table.GetSectionData(SectionType.Body);

            if (body == null || body.NumberOfRows == 0 || body.NumberOfColumns == 0)
            {
                return new ColumnsOutcome(text.NoBody, schedule.Name, 0, 0, 0, Array.Empty<string>());
            }

            ScheduleDefinition definition = schedule.Definition;
            var notes = new List<string>();
            var plan = new List<(ScheduleField Field, ColumnAction Action, int Column)>();
            int hidden = 0;
            int shown = 0;
            int left = 0;

            int firstRow = body.FirstRowNumber;
            int lastRow = body.LastRowNumber;

            for (int column = body.FirstColumnNumber; column <= body.LastColumnNumber; column++)
            {
                var cells = new List<string?>();

                for (int row = firstRow; row <= lastRow; row++)
                {
                    if (body.IsValidRowNumber(row) && body.IsValidColumnNumber(column))
                    {
                        cells.Add(body.GetCellText(row, column));
                    }
                }

                ElementId parameterId = body.GetCellParamId(firstRow, column);
                ScheduleField? field = FindField(definition, parameterId);

                if (field == null)
                {
                    left++;
                    notes.Add(text.NoParameterColumn(column));

                    continue;
                }

                bool hasValues = !ColumnVisibility.IsEmpty(cells);
                ColumnAction action = ColumnVisibility.Decide(field.IsHidden, hasValues);

                if (action == ColumnAction.Leave)
                {
                    left++;
                }
                else
                {
                    plan.Add((field, action, column));
                }
            }

            if (plan.Count > 0)
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    foreach ((ScheduleField field, ColumnAction action, int column) in plan)
                    {
                        try
                        {
                            field.IsHidden = action == ColumnAction.Hide;

                            if (action == ColumnAction.Hide)
                            {
                                hidden++;
                            }
                            else
                            {
                                shown++;
                            }
                        }
                        catch (Exception exception)
                        {
                            notes.Add(text.Refused(column, exception.Message));

                            log.Warn(
                                "ScheduleColumns: column " + column + " was refused: " + exception.Message,
                                nameof(ScheduleColumnsEngine),
                                exception);
                        }
                    }

                    return hidden > 0 || shown > 0;
                });
            }

            log.Info(
                "ScheduleColumns: «" + schedule.Name + "»: " + hidden + " hidden, " + shown + " shown, " +
                left + " left.",
                nameof(ScheduleColumnsEngine));

            return new ColumnsOutcome(null, schedule.Name, hidden, shown, left, notes);
        }

        private static ScheduleField? FindField(ScheduleDefinition definition, ElementId parameterId)
        {
            if (parameterId == null || parameterId == ElementId.InvalidElementId)
            {
                return null;
            }

            for (int index = 0; index < definition.GetFieldCount(); index++)
            {
                ScheduleField field = definition.GetField(index);

                if (field.ParameterId == parameterId)
                {
                    return field;
                }
            }

            return null;
        }
    }
}
