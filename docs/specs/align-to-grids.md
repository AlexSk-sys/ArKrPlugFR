# Align columns to grids — functional specification

## Purpose
Pull columns exactly onto the coordination grids they belong to. Columns drift off the
axes by a few millimetres during modelling; the tool finds, for every picked column, the
nearest grid line within a small capture distance and moves the column so that its centre
lies on that line — or on the crossing of two lines — without touching anything else.

## Inputs
- An open project document with a plan view open.
- Columns, picked in the view. Optional: without a pick there is nothing to align.
- Grids, picked in the view. Required: without lines the run cannot start.
- The capture distance, in millimetres: a whole number from 1 to 100, by default 5. A
  grid line further away than this from the column's centre is not considered.
- The grids must be straight lines; a curved grid is reported and skipped.

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the picked grids as lines in the project's horizontal plane. A grid that yields no
   straight line is reported and skipped. When no usable grid is left, report it and
   change nothing.
3. Read the picked columns. A column that has no location point — one that is not a
   point-based family instance — is reported and skipped. When no column is left, report
   it and change nothing.
4. For every column, read the centre of its location point and work out the target:
   - the nearest grid line whose distance to the centre is within the capture distance;
   - then, the nearest non-parallel grid line also within the capture distance, if any;
   - the target is the crossing of the two lines, or the foot of the perpendicular on the
     single nearest line.
   Only the horizontal position of the centre is considered; the column keeps its vertical
   position and its orientation.
5. A column whose centre is already on the target — within 0.1 mm — is counted as already
   aligned and is not touched.
6. A column that belongs to a group is skipped and counted: moving a member of a group is
   the platform's decision, not the tool's.
7. In one transaction, move every planned column to its target. A column the platform
   refuses to move is reported with its id and the reason, and the run continues.
8. Report: how many columns were read, how many were moved, how many were already on a
   grid, how many had no grid within the capture distance, how many were skipped as group
   members, and how many were refused, with the first few refusals listed one by one.
9. Change nothing else: no element is created or deleted, the grids are never moved, and
   the columns keep their orientation and their vertical position.

## Results and messages
- No active document: error, nothing changed.
- Nothing picked or picked columns left after filtering: "Pick the columns to align." —
  nothing changed.
- No usable grids: "Pick the grids to align to; the picked grids hold no straight lines."
  — nothing changed.
- Success: "Aligned {0} of {1} column(s) to the grids; already on a grid: {2}; no grid
  within the capture distance: {3}; in groups: {4}; refused: {5}." — informational.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures
- A curved grid, or a grid whose line is not in the horizontal plane: reported, skipped.
- A column with a rotated profile: moved as a point; its rotation is not changed.
- A pinned column, or one the platform refuses to move for any other reason: reported with
  its id and the reason, the run continues with the rest.
- A column already on a grid line but past a crossing of two lines: the two-line plan does
  not apply, the single foot is used, and the column stays on its line.
- Two parallel grids of the same direction both within reach: only the nearer one counts;
  a column is never aligned to two parallel lines at once.
- Angled grids: the rules work on the line's own direction, so a column lands on an angled
  axis exactly as on an axis-aligned one.
- Running the tool twice: the second run finds every column already on its target and
  changes nothing.

## UI surfaces
- A tool window: the capture distance, two buttons that pick the columns and the grids, a
  line that counts what was picked, an apply button and a close button. The window hides
  while the user picks in the model and returns after the pick; the result is shown in the
  window and in the shared result window.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the grids panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Family instances of the column categories and their location points.
- Grid elements and their curves; a grid read as a line in the project's horizontal plane.
- Distances and projections in the horizontal plane only; the vertical position is carried
  through untouched.
- Groups: an element whose group id is valid belongs to a group and is left alone.
- Transactions: the whole run is one undoable step and is rolled back when nothing moved.

## Scope of this release
Implemented: everything described above — the picked columns and grids, straight grids in
the horizontal plane, the capture distance, the nearest line and the two-line crossing,
the already-aligned and in-group counts, one transaction, the bilingual window and report,
and the remembered capture distance.

Not implemented: capitals and slab openings (the tool this replaces touched their edges
too; this release aligns columns only); aligning walls, beams or any other categories;
curved grids; automatic reading of grids from linked models (the tool works on the grids
of the current project, which the user picks); and aligning more than the centre — for
example the profile's edges or the column's face.
