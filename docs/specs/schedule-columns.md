# Empty columns of a schedule — functional specification

## Purpose

Keep a schedule readable: a column whose every row is empty or zero tells the reader nothing,
and a wide schedule with a handful of such columns is hard to read on screen and on paper.
The tool hides those columns and brings back the ones that carry values again, so the shape of
the schedule follows its content instead of being edited by hand.

## Inputs

- An open project document.
- The schedule to work on, chosen from the schedules of the project.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the schedule's body: every column of it and every row.
3. For every column, read the text of its cells and the parameter behind it:
   - a column whose cells are all empty, or all numbers equal to zero, is hidden;
   - a column that is hidden and holds at least one value that is neither empty nor zero is
     shown again;
   - every other column is left as it is.
4. A cell counts as empty when its text is empty once trimmed, and as zero when its text is a
   number that equals zero — `0`, `0.00` and `0,00` are all zero. Anything else is a value.
5. In one transaction, hide and show the columns, and report: how many were hidden, how many
   were shown, how many were left as they were, and the columns that could not be decided
   because no parameter stands behind them.
6. Change nothing else: no row, no field, no filter and no element is touched; only the
   visibility of the columns changes, and the run is one undoable step.

## Results and messages

- No active document: error, nothing changed.
- No schedule chosen: the run reports it and changes nothing.
- A schedule without a body: reported, nothing changed.
- Success: "Columns: hidden {0}; shown {1}; left as they were {2}."
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A column that carries no parameter (a calculated or combined field): left alone and counted,
  because the tool cannot ask the platform whether it may be hidden.
- A schedule whose cells are all empty: every column is hidden except the ones without a
  parameter; the report says so.
- Running the tool again changes nothing, because the columns that were hidden stay hidden and
  the ones that were shown carry values.
- A schedule the platform refuses to change: reported with the reason.

## UI surfaces

- A tool window: the schedule list, a run button, a report area and a close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the schedules panel.
- All text is provided in Russian and English.

## Revit concepts involved

- `ViewSchedule`, its `ScheduleDefinition` and its `ScheduleField` elements, whose visibility
  the platform lets a tool change (`ScheduleField.IsHidden`).
- `ViewSchedule.GetTableData` and the body section of the table for the cells of every column,
  with the parameter behind a column read from the cell.
- Transactions: the whole run is one undoable step.

## Scope of this release

Implemented: everything described above — the schedule list, the cell reading, the hide and
show rule with its zero handling, the columns without a parameter, one transaction, the
bilingual window and report.

Not implemented: the reference's other buttons (a table of sheets and a schedule placement
refresh); hiding rows; grouping and sorting; any change to the elements the schedule lists.
