# Wall openings — functional specification

## Purpose
Cut openings in the finish walls that line a load-bearing wall, opposite the doors and
windows standing in that load-bearing wall. The user selects one or several bearing
walls, chooses whether doors, windows or both are processed, and the tool finds the
finish walls standing right beside them and cuts an opening in each finish wall where a
door or a window passes through.

## Inputs
- An open project document, and at least one wall in the current selection.
- Two options: process doors, process windows; both are on by default and are remembered
  between sessions in the user's settings folder.
- The gap within which a wall counts as a finish wall lining the selected wall: 150 mm by
  default.
- The width and height of an opening, taken in this order: the opening the bearing wall
  actually has there (a hosted opening element), else the bounding box of the door or
  window instance including its frame, else the width and height parameters of the
  instance.

## Behaviour
1. On start, require an active project document. Without one, report an error and open no
   window.
2. Read the options from the settings file; a missing or corrupt file falls back to both
   switches being on.
3. Open one modal window owned by the application's main window: the two switches, the
   counts of walls in the selection, a short description, and one Apply button.
4. On Apply, take the wall selection. Without a wall, report it and start no transaction.
5. For every selected wall, find the door and window instances it hosts, according to the
   switches.
6. For every such instance, work out the opening it stands in: its rectangle in the
   frame of the bearing wall — how far it reaches along the wall and how high above the
   wall's base point.
7. Find the finish walls of that bearing wall: every other wall of the document whose
   direction is parallel to it and whose distance across it does not exceed the gap. A
   wall standing on the other side of the opening but running the other way round still
   counts.
8. Cut an opening in each finish wall: the same rectangle, at the same place along the
   wall, cut square to the finish wall.
9. Skip a position where the finish wall already carries an opening of that size: an
   existing opening whose four edges lie within ten millimetres of the planned rectangle
   is left alone, so running the tool twice does not cut twice.
10. Regenerate the document once at the end, so the walls and their joins are redrawn
    with the new openings in place.
11. Report how many openings were cut, how many were already there, and how many doors
    and windows could not be used, with the reason.

## Results and messages
- Nothing selected: "Select at least one wall that carries doors or windows." — error.
- No doors or windows on the selection: "The selected walls carry no doors or windows
  matching the chosen options." — informational, nothing cut.
- No finish wall found: reported as a count, not as an error: a bearing wall standing
  alone is normal.
- Success: "Openings cut: N. Already there: M. Skipped: K." — informational.
- An opening the platform refuses: reported per wall with its reason, and the rest of the
  run continues.
- Every message is provided in Russian and English; the window closes on Escape and the
  result appears in the shared result window.

## Edge cases and failures
- No active document: reported, no window.
- A wall that is curved: its hosted doors and windows are processed by the position of
  the instance alone, and finish walls are matched by the gap between the instance and
  the wall, not by parallel directions; a curved wall is never used as a finish wall.
- A selected wall that is itself a finish wall of another selected wall: an opening
  already cut there is recognised and not cut twice.
- The same wall selected twice: it is one wall.
- An opening larger than the finish wall is tall or long: the platform refuses it, which
  is reported and the wall is left alone.
- A door or window deleted while the window is open: skipped with its reason.
- A settings file that cannot be written: the run continues, the options simply are not
  remembered.

## Revit concepts involved
- Wall and its location curve: a line for a straight wall, an arc for a curved one.
- Hosted family instances: doors and windows of the walls of the selection.
- Openings: the platform's rectangular opening on a wall, and hosted opening elements
  that describe a bearing wall's own opening.
- Wall direction, the distance across a wall, and parallelism of two directions.
- Wall joins and document regeneration.
- Transactions: all openings of one run are cut in one undoable step.
- Per-user settings storage as a JSON file.

## Scope of this release
Implemented: everything described above — the two switches remembered in the user's
settings folder, the wall selection, the hosted doors and windows, the opening rectangle
taken from the hosted opening element, else from the instance's box, else from the width
and height parameters, the neighbour rule (parallel within the gap), the cut square to
the finish wall, the duplicate check with a tolerance, one regeneration at the end, and
the bilingual window and report.

Not implemented: using a void family of a chosen opening family (a native opening is cut
instead); recognising which wall is a finish wall from its type, material or function
(only the parallel-within-the-gap rule is used); curved bearing walls as the source of
openings beyond the position of their hosted instances; and cutting openings for doors
or windows that stand in a linked model rather than in the document.
