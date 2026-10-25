using System;
using System.Collections.Generic;
using System.Globalization;

namespace ArKrPlugFR.Core.Schedules
{
    /// <summary>What is to be done with one column of a schedule.</summary>
    public enum ColumnAction
    {
        /// <summary>Leave the column as it is.</summary>
        Leave,

        /// <summary>Hide the column: it tells the reader nothing.</summary>
        Hide,

        /// <summary>Show the column again: it carries values.</summary>
        Show
    }

    /// <summary>
    /// The rules of hiding the columns of a schedule that tell the reader nothing.
    /// <para>
    /// A column whose every cell is empty or zero is hidden; a hidden column that holds a
    /// value is shown again; everything else is left alone. The rules work on the text the
    /// schedule shows, so they are plain string work and are tested without Revit.
    /// </para>
    /// </summary>
    public static class ColumnVisibility
    {
        /// <summary>
        /// Determines whether a cell shows nothing: its text is empty, or it is a number
        /// equal to zero.
        /// </summary>
        /// <param name="text">The text the cell shows.</param>
        /// <returns><c>true</c> when the cell tells the reader nothing.</returns>
        public static bool IsBlankOrZero(string? text)
        {
            if (text == null)
            {
                return true;
            }

            string trimmed = text.Trim();

            if (trimmed.Length == 0)
            {
                return true;
            }

            return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out double value)
                || double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                ? value == 0
                : false;
        }

        /// <summary>Determines whether every cell of a column tells the reader nothing.</summary>
        /// <param name="cells">The text of the cells of one column.</param>
        /// <returns><c>true</c> when the column holds no value at all.</returns>
        public static bool IsEmpty(IEnumerable<string?> cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            bool any = false;

            foreach (string? cell in cells)
            {
                any = true;

                if (!IsBlankOrZero(cell))
                {
                    return false;
                }
            }

            // A column without a single cell is not known to be empty; leave it alone.
            return any;
        }

        /// <summary>Decides what to do with one column.</summary>
        /// <param name="isHidden">Whether the column is hidden in the schedule.</param>
        /// <param name="hasValues">Whether the column holds at least one value.</param>
        /// <returns>The action for this column.</returns>
        public static ColumnAction Decide(bool isHidden, bool hasValues)
        {
            if (hasValues)
            {
                return isHidden ? ColumnAction.Show : ColumnAction.Leave;
            }

            return isHidden ? ColumnAction.Leave : ColumnAction.Hide;
        }
    }
}
