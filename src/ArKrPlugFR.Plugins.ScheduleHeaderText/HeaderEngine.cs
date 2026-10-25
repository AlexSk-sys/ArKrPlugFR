using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Schedules;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.ScheduleHeaderText
{
    /// <summary>One field of a schedule that carries a parameter.</summary>
    internal sealed class HeaderField
    {
        internal HeaderField(string name, ElementId parameterId)
        {
            Name = name;
            ParameterId = parameterId;
        }

        internal string Name { get; }

        internal ElementId ParameterId { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>What one write did.</summary>
    internal sealed class HeaderOutcome
    {
        internal HeaderOutcome(string? stopped, string? written, int row, int column)
        {
            Stopped = stopped;
            Written = written;
            Row = row;
            Column = column;
        }

        /// <summary>Gets the reason nothing was written, or <c>null</c>.</summary>
        internal string? Stopped { get; }

        /// <summary>Gets the value that was written, or <c>null</c>.</summary>
        internal string? Written { get; }

        internal int Row { get; }

        internal int Column { get; }
    }

    /// <summary>
    /// Reads the schedules of the project and writes a value into the header of one of them.
    /// <para>
    /// The value comes either from the elements the schedule lists — the first element that
    /// carries the chosen parameter — or from the window. The write goes into the header
    /// area, the one place the platform lets a tool put text into a schedule; the bounds of
    /// that area are read from the platform and the cell is resolved by
    /// <see cref="HeaderCell"/>, which is tested without Revit.
    /// </para>
    /// </summary>
    internal static class HeaderEngine
    {
        /// <summary>Lists the schedules of the project, ordered by name.</summary>
        /// <param name="document">Project to read.</param>
        /// <returns>The schedules a value can be written into.</returns>
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

        /// <summary>Lists the fields of a schedule that carry a parameter.</summary>
        /// <param name="schedule">Schedule to read.</param>
        /// <returns>The fields, in the order of the schedule.</returns>
        internal static IReadOnlyList<HeaderField> Fields(ViewSchedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            var fields = new List<HeaderField>();
            ScheduleDefinition definition = schedule.Definition;

            for (int index = 0; index < definition.GetFieldCount(); index++)
            {
                ScheduleField field = definition.GetField(index);
                ElementId parameterId = field.ParameterId;

                if (parameterId != null && parameterId != ElementId.InvalidElementId)
                {
                    fields.Add(new HeaderField(field.GetName(), parameterId));
                }
            }

            return fields;
        }

        /// <summary>Writes a value into the header of one schedule.</summary>
        /// <param name="document">Project the schedule lives in.</param>
        /// <param name="schedule">Schedule to write into.</param>
        /// <param name="field">The field whose value is taken from the elements.</param>
        /// <param name="fromElements">Whether the value comes from the elements.</param>
        /// <param name="typedValue">The value typed in the window.</param>
        /// <param name="rowIndex">Header row counted from one.</param>
        /// <param name="columnIndex">Header column counted from one.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the write did.</returns>
        internal static HeaderOutcome Run(
            Document document,
            ViewSchedule schedule,
            HeaderField field,
            bool fromElements,
            string typedValue,
            int rowIndex,
            int columnIndex,
            HeaderText text,
            Log log)
        {
            if (fromElements && !HasCategory(document, schedule))
            {
                return new HeaderOutcome(text.NoCategory, null, 0, 0);
            }

            string? value = fromElements
                ? ValueFromElements(document, schedule, field)
                : (typedValue ?? string.Empty).Trim();

            if (value == null)
            {
                return new HeaderOutcome(text.NoElementValue(field.Name), null, 0, 0);
            }

            if (value.Length == 0)
            {
                return new HeaderOutcome(text.NoValue, null, 0, 0);
            }

            TableData table = schedule.GetTableData();
            TableSectionData? header = table.GetSectionData(SectionType.Header);

            if (header == null)
            {
                return new HeaderOutcome(text.NoHeader, null, 0, 0);
            }

            int firstRow = header.FirstRowNumber;
            int lastRow = header.LastRowNumber;
            int firstColumn = header.FirstColumnNumber;
            int lastColumn = header.LastColumnNumber;

            if (!HeaderCell.TryResolve(
                firstRow,
                lastRow,
                firstColumn,
                lastColumn,
                rowIndex,
                columnIndex,
                out int row,
                out int column,
                out HeaderCellProblem problem))
            {
                string message = problem == HeaderCellProblem.RowOutOfRange
                    ? text.RowOutOfRange(lastRow - firstRow + 1)
                    : text.ColumnOutOfRange(lastColumn - firstColumn + 1);

                return new HeaderOutcome(message, null, 0, 0);
            }

            try
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    header.SetCellText(row, column, value);

                    return true;
                });

                log.Info(
                    "ScheduleHeaderText: wrote «" + value + "» into «" + schedule.Name + "» at row " +
                    row.ToString(CultureInfo.InvariantCulture) + ", column " + column.ToString(CultureInfo.InvariantCulture) + ".",
                    nameof(HeaderEngine));

                return new HeaderOutcome(null, value, row, column);
            }
            catch (Exception exception)
            {
                log.Warn(
                    "ScheduleHeaderText: the write was refused: " + exception.Message,
                    nameof(HeaderEngine),
                    exception);

                return new HeaderOutcome(text.WriteFailed(exception.Message), null, 0, 0);
            }
        }

        private static bool HasCategory(Document document, ViewSchedule schedule)
        {
            ElementId categoryId = schedule.Definition.CategoryId;

            return categoryId != ElementId.InvalidElementId && Category.GetCategory(document, categoryId) != null;
        }

        private static string? ValueFromElements(
            Document document,
            ViewSchedule schedule,
            HeaderField field)
        {
            ElementId categoryId = schedule.Definition.CategoryId;

            var values = new List<string?>();

            foreach (Element element in new FilteredElementCollector(document)
                .OfCategoryId(categoryId)
                .WhereElementIsNotElementType()
                .ToElements()
                .OrderBy(item => ElementIdCompat.GetValue(item.Id)))
            {
                values.Add(ValueOf(element, field.ParameterId));

                // A type parameter carries its value on the type, not on the instance, so the
                // type is read as well; the first usable value wins either way.
                if (values[values.Count - 1] == null)
                {
                    Element? type = document.GetElement(element.GetTypeId());

                    if (type != null)
                    {
                        values.Add(ValueOf(type, field.ParameterId));
                    }
                }
            }

            return HeaderValue.FirstNonEmpty(values);
        }

        private static string? ValueOf(Element element, ElementId parameterId)
        {
            Parameter? parameter = element.Parameters
                .Cast<Parameter>()
                .FirstOrDefault(item => item.Id == parameterId);

            if (parameter == null || !parameter.HasValue)
            {
                return null;
            }

            return parameter.AsValueString() ?? parameter.AsString();
        }
    }
}
