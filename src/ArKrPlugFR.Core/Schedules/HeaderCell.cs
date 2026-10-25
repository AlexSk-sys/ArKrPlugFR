using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Schedules
{
    /// <summary>Why a header cell could not be resolved.</summary>
    public enum HeaderCellProblem
    {
        /// <summary>The cell lies inside the header area.</summary>
        None,

        /// <summary>The row lies outside the header area.</summary>
        RowOutOfRange,

        /// <summary>The column lies outside the header area.</summary>
        ColumnOutOfRange
    }

    /// <summary>
    /// Finds a cell inside the header area of a schedule.
    /// <para>
    /// A schedule header is a table section of its own: it has its own first and last row
    /// and column, and the user counts them from one. The rule is plain arithmetic, so it
    /// lives here and is tested without Revit; the caller asks the platform for the bounds
    /// and writes into the cell this returns.
    /// </para>
    /// </summary>
    public static class HeaderCell
    {
        /// <summary>Resolves a cell counted from one inside the header area.</summary>
        /// <param name="firstRow">First row of the header area.</param>
        /// <param name="lastRow">Last row of the header area.</param>
        /// <param name="firstColumn">First column of the header area.</param>
        /// <param name="lastColumn">Last column of the header area.</param>
        /// <param name="rowIndex">Row counted from one inside the header area.</param>
        /// <param name="columnIndex">Column counted from one inside the header area.</param>
        /// <param name="row">The row of the cell, or zero when it could not be resolved.</param>
        /// <param name="column">The column of the cell, or zero when it could not be resolved.</param>
        /// <param name="problem">What went wrong, or <see cref="HeaderCellProblem.None"/>.</param>
        /// <returns><c>true</c> when the cell lies inside the header area.</returns>
        public static bool TryResolve(
            int firstRow,
            int lastRow,
            int firstColumn,
            int lastColumn,
            int rowIndex,
            int columnIndex,
            out int row,
            out int column,
            out HeaderCellProblem problem)
        {
            row = 0;
            column = 0;
            problem = HeaderCellProblem.None;

            if (rowIndex < 1 || firstRow + rowIndex - 1 > lastRow)
            {
                problem = HeaderCellProblem.RowOutOfRange;

                return false;
            }

            if (columnIndex < 1 || firstColumn + columnIndex - 1 > lastColumn)
            {
                problem = HeaderCellProblem.ColumnOutOfRange;

                return false;
            }

            row = firstRow + rowIndex - 1;
            column = firstColumn + columnIndex - 1;

            return true;
        }
    }

    /// <summary>
    /// Picks the value a schedule header should show out of the values its elements carry.
    /// </summary>
    public static class HeaderValue
    {
        /// <summary>Returns the first value that is not empty once trimmed.</summary>
        /// <param name="values">Values of the elements, in the order they are read.</param>
        /// <returns>The first usable value, or <c>null</c> when there is none.</returns>
        public static string? FirstNonEmpty(IEnumerable<string?> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            foreach (string? value in values)
            {
                if (value != null)
                {
                    string trimmed = value.Trim();

                    if (trimmed.Length > 0)
                    {
                        return trimmed;
                    }
                }
            }

            return null;
        }
    }
}
