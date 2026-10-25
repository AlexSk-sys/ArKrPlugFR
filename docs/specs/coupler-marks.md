# Mark rebar couplers from the nearest bar — functional specification

## Purpose
A mechanical coupler carries the identity of the bar it connects: which structure the bar
belongs to and what that structure is. Filling those fields by hand means measuring which
bar ends next to the coupler and copying two values. This tool does the measuring: for
every picked coupler it finds the bar whose centre is nearest, and when that bar is within
the tolerance it writes the mark and the category of the bar's host into the coupler. It
changes nothing but those parameters.

## Inputs
- Couplers picked in the model: family instances of the shape the project uses for
  mechanical couplers. Nothing picked means nothing to do.
- The tolerance in millimetres: a whole number from 1 to 2000, by default 200. A bar whose
  centre is further from the central axis of the coupler is not considered.
- The name of the parameter that receives the mark of the host, by default
  `ADSK_Метка основы`.
- The name of the parameter that receives the category of the host, by default
  `ADSK_Категория основы`.

## Behaviour
1. Require at least one coupler and usable parameter names; without them, report and change
   nothing.
2. Read the bars of the project once: every rebar element, the centre of its bounding box
   and its host.
3. For every coupler, take the central axis of its bounding box: the longest of its three
   axes through the centre of the box. A coupler without a box is reported and skipped.
4. Find the bar whose centre lies nearest to that axis, within the tolerance. A coupler
   without such a bar is counted and skipped.
5. Read the mark and the category of the bar's host. A bar whose host carries no mark, or
   an element without a category, makes the coupler be reported and skipped — a transfer
   never writes half a pair.
6. Write both values into the coupler in one transaction: the mark into the parameter
   named for it and the category name into the other. A coupler whose parameters are
   missing, read-only or not text parameters is reported; the run continues with the rest.
7. Report: couplers read, updated, without a bar in reach, without host data, and refused.
8. Change nothing else: no geometry is created or moved, and the coupler is never re-hosted.

## Results and messages
- Nothing picked: "Pick the couplers to mark." — nothing changed.
- No usable parameter name: "Enter the name of the parameter that receives the mark and the
  name of the one that receives the category." — nothing changed.
- Tolerance outside its range: "Enter the tolerance as a whole number of millimetres from 1
  to 2000." — nothing changed.
- Success: "Read {0} coupler(s); updated: {1}; no bar in reach: {2}; host without data: {3};
  refused: {4}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the pick button, the picked list, the settings, a report area and the run and close
  buttons, and closes on Escape.

## Edge cases and failures
- A coupler without geometry: reported, skipped.
- A host without a mark or without a category: reported, skipped — neither parameter is
  written for that coupler.
- Two bars at the same distance: the earlier one in the reading order wins.
- A coupler that already carries the values: written again, counted as updated.
- Hundreds of couplers against thousands of bars: the bars are read once, not once per
  coupler.
- The run is one undoable step; a run that has nothing to write leaves no entry in the
  undo list.

## UI surfaces
- A tool window: the button that picks the couplers (the window hides while the user picks
  and returns after the pick), the list of the picked couplers with its count, the
  tolerance field, the two parameter name fields, the hint, a report area and the run and
  close buttons. The settings are remembered between sessions.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Rebar elements, the centres of their bounding boxes, and their host
  (`Rebar.GetHostId`).
- The host: its mark (`ALL_MODEL_MARK`) and its category name.
- The coupler: a family instance, its bounding box and the longest of its three axes.
- Transactions: the whole run is one undoable step; a run that writes nothing is rolled
  back.

## Scope of this release
Implemented: everything described above — the picked couplers, the nearest-bar search by
the central axis, the tolerance, the mark and the category of the host, the two named
parameters, the counts and the report.

Not implemented: scanning the whole project for couplers automatically (the replaced tool
took every coupler of the project when nothing was picked; here the user picks them);
deriving the coupler category from the newer rebar-coupler elements of Revit 2022 and
later; and writing more than the two values — the run touches no other parameter.
