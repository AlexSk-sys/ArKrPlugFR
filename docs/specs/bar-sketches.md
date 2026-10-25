# Sketch sheets of rebar bars — functional specification

## Purpose

Give every bar of a detail drawing its own sketch: a sheet with a view of the bar, cropped to
it, and a note that names its shape, its diameter, its length and its mark. Drawing a bar
sketch by hand for a whole schedule is slow and repetitive; the tool makes one sheet per
picked bar in a single action, and the geometry it shows is the real bar, not a redrawn
picture of it.

## Inputs

- An open project document.
- The bars to sketch, picked in the model.
- A title block family type, taken from the project; a project without one is reported.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the bars that were picked. The run sketches at most twenty-five of them in one go and
   says how many were left for the next run.
3. For every bar, in one transaction:
   - read the facts of the bar: the name of its shape, the nominal diameter of its bar type,
     its total length and its schedule mark;
   - create a three-dimensional view, named after the shape and its number in the run;
   - crop the view to the bar: a section box around the bar's bounding box, grown a little so
     the bar is not cut off;
   - create a sheet from the title block of the project and place the view on it;
   - write a note on the sheet with the shape, the diameter, the length and the mark.
4. Report: how many sheets were made, the name of every sheet, how many bars were skipped and
   how many the platform refused, with the first few refusals one by one.
5. Change nothing else: no bar, no host and no schedule is edited; the run adds views, sheets
   and notes, and it is one undoable step.

## Results and messages

- No active document: error, nothing changed.
- No bar picked: the run reports it and changes nothing.
- No title block in the project: reported, nothing changed.
- No three-dimensional view family in the project: reported, nothing changed.
- Success: "Sheets made: {0} of {1} bar(s)." followed by one line per sheet, and, when the
  run stopped at the limit, "The next run continues with the remaining {0} bar(s)."
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A bar without a bounding box: skipped and counted.
- A bar without a shape, a mark or a length: the note says what is unknown instead of
  inventing a value.
- A bar the platform refuses: reported with its id and the reason, and the run continues with
  the next bar.
- A view name that already exists in the project: the run adds a number to it, so a second
  run makes its sheets instead of being refused.
- More bars picked than the run sketches: the report says how many are left.
- Running the tool again on the remaining bars makes the next set of sheets.

## UI surfaces

- A tool window: the bar pick with its state, a run button, a report area and a close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved

- `Rebar` elements and their facts: the shape through `GetShapeId`, the diameter through the
  bar type, the total length and the schedule mark.
- A three-dimensional view created with `View3D.CreateIsometric` and cropped with
  `View3D.SetSectionBox`.
- A sheet created with `ViewSheet.Create` from a title block family type, the view placed with
  `Viewport.Create` and the note written with `TextNote.Create`.
- Transactions: the whole run is one undoable step.

## Scope of this release

Implemented: everything described above — the picked bars, the per-bar sheet with a cropped
view and a note, the run limit, the counts and the refusals, one transaction, the bilingual
window and report.

Not implemented: the reference's drawn sketches. The reference built a picture of the bar from
a template library, drew its dimensions onto the picture and inserted the picture into a
schedule; drawing a picture from a template is not part of the platform's public API, and the
pictures themselves belonged to the reference's library. This tool shows the bar's own
geometry in a cropped view instead, and states its dimensions as text, which needs neither a
library nor a drawing engine. Also not implemented: editing the shape library, dimension
lines on the sheet, and grouping bars of one shape onto a shared sheet.
