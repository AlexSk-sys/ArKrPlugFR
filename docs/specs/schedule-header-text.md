# Text in a schedule header — functional specification

## Purpose

Put a value into the header of a schedule. A schedule often carries its own designation —
the mark of the product it describes — in the header above the rows, and that value usually
also lives in the elements the schedule lists. The tool takes the value from those elements
and writes it into the header cell, so the designation never has to be typed twice.

## Inputs

- An open project document.
- The schedule, chosen from a list of the schedules of the project.
- The parameter, chosen from the fields of that schedule that carry a parameter.
- Where the value comes from: the first non-empty value of that parameter among the
  elements the schedule lists, or a value typed in the window.
- The header cell: the number of the row and of the column inside the header area, counted
  from one; both default to the fourth row and the first column.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. On start, list the schedules of the project — every schedule that is not a template and
   not a revision schedule of a title block — and, for the chosen schedule, the fields of
   its definition that carry a parameter.
3. When the run is asked for:
   - a schedule must be chosen, and a parameter field of it;
   - when the value comes from the elements, the schedule must name a category, and the
     elements of that category are read in id order; the value is the first non-empty value
     of the parameter; an element whose parameter has no value is skipped;
   - when the value is typed, it must not be empty;
   - the header cell is resolved inside the header area of the schedule; a row or column
     outside that area is reported and nothing is written.
4. In one transaction, write the value into the header cell of the schedule.
5. Report what was written, into which schedule, and into which cell.
6. Change nothing else: no element is created, moved or deleted; only the text of one
   header cell of one schedule changes.

## Results and messages

- No active document: error, nothing changed.
- No schedule chosen: the run reports what is missing and changes nothing.
- No parameter field chosen: the run reports what is missing and changes nothing.
- No value found among the elements: the run reports it and changes nothing.
- The cell lies outside the header area: the run reports it and changes nothing.
- Success: "«{value}» was written into the header of «{schedule}», row {row}, column {column}."
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A schedule without a header area: reported, nothing written.
- A schedule whose category is not a single category (a multi-category schedule): the value
  cannot be taken from elements; the run reports it and asks for a typed value instead.
- An element whose parameter exists but is empty, or whose value cannot be read as text:
  skipped.
- A header cell the platform refuses: reported with the reason, nothing written.
- A cell that already holds text: it is overwritten; the run says so.
- Running the tool again with the same inputs writes the same value; it is safe to run
  repeatedly.

## UI surfaces

- A tool window: the schedule list, the parameter list, the value source with its text
  field, the row and column fields, a run button, a report area and a close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the schedules panel.
- All text is provided in Russian and English.

## Revit concepts involved

- `ViewSchedule` and its `ScheduleDefinition` as the schedule; `ScheduleField` and the
  parameter id of a field as the parameter.
- `Category.GetCategory` and a collector over the category of the schedule as the elements
  the value is taken from.
- `ViewSchedule.GetTableData`, `TableData.GetSectionData(SectionType.Header)` and
  `TableSectionData.SetCellText` as the header cell. The public API forbids writing text
  into a body cell of a standard schedule, which is why this tool writes into the header
  area, where the platform allows it.
- Transactions: the write is one undoable step.

## Scope of this release

Implemented: everything described above — the schedule list, the parameter list, the two
value sources, the header cell with its bounds check, the single write, the bilingual window
and report.

Not implemented: writing into the rows of a standard schedule (the platform forbids it for
body sections); several schedules at once; conditional formatting; any change to the
definition of the schedule.
