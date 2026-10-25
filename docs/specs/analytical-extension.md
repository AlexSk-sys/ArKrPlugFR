# Extending the analytical model up to a floor or a beam — functional specification

## Purpose

Close the gaps in a calculation model: a column or a wall that stops short of the floor above
leaves the analysis with a member hanging in the air. The tool extends the analytical model
of the picked columns and walls up to the nearest floor or beam above them, so every member
rests on something.

## Inputs

- An open project document.
- The sources to extend: picked columns and walls, or their analytical elements directly.
- The targets to extend to: picked floors and beams.
- The reach of the search in plan: a distance, 4572 mm (fifteen feet) by default.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read every target: the bounding box of the element, its bottom and its footprint in plan.
   A target without a bounding box is skipped and counted.
3. For every source, find its analytical line:
   - a source that is an analytical element itself brings its own line;
   - a physical element brings the analytical element the platform associates with it; a
     source without one is skipped and counted.
4. Read the two ends of the line: the higher one is the top of the source, the lower one its
   bottom. An element whose top is already at or above a target is left alone.
5. Choose the target for the source: among the targets whose bottom lies above the top of
   the source and whose footprint lies within the reach in plan, the one with the lowest
   bottom. A source without such a target is skipped and counted.
6. Extend the source: the analytical line keeps its place in plan and its bottom, and its top
   moves to the bottom of the chosen target. When the platform does not let the analytical
   line be written directly — which is the case for the analytical members of Revit 2022 —
   the physical column is raised instead, by moving its top to the bottom of the target,
   and the analytical model follows it. A column whose top is attached to another element is
   left alone and counted; a source that cannot be extended at all is reported with its id
   and the reason.
7. Run every extension in one transaction and report: how many sources were extended, how
   many had no target above them, how many had no analytical line, how many were attached
   at the top, how many were refused.
8. Change nothing else: no load and no boundary condition is touched, and no physical element
   moves except the columns of the fallback path, whose top level offset changes.

## Results and messages

- No active document: error, nothing changed.
- No source picked, or no target picked: the run reports what is missing and changes nothing.
- No source has an analytical line: reported, nothing changed.
- Success: "Sources: {0}; extended: {1}; without a target above: {2}; without an analytical
  line: {3}; attached at the top: {4}; refused: {5}."
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A source whose top already reaches a target: left alone and counted as already extended.
- A target below the top of the source: never chosen.
- A target further away in plan than the reach: never chosen.
- Two targets at the same bottom: the nearer one in plan wins; the order is stable.
- A source the platform refuses: reported with its id and the reason, and the run continues.
- Running the tool again extends nothing new and reports that everything already reaches its
  target.

## UI surfaces

- A tool window: the source pick, the target pick, the reach, the run button, a report area
  and a close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the loads panel.
- All text is provided in Russian and English.

## Revit concepts involved

- The analytical line of a source: the location line of the analytical element the platform
  associates with the physical element — the association parameter of Revit 2024, or the
  analytical model id of Revit 2020 and 2022, the difference living in the compatibility
  layer. When the platform names no analytical element for a physical one, the fallback looks
  for an analytical line that lies inside the physical element's box together with its own
  box, which keeps a beam that merely ends inside a column out of the answer.
- The bounding boxes of the targets for their bottom and their footprint.
- Writing the analytical line, or, where the release does not allow it, raising the physical
  column through its top level and top offset parameters.
- Transactions: the whole run is one undoable step.

## Scope of this release

Implemented: everything described above — the two picks, the reach, the choice of the lowest
target above, the extension of the analytical line with the column fallback, the counts and
the refusals, one transaction, the bilingual window and report.

Not implemented: creating an analytical model for a source that has none (the reference
created analytical panels for walls, which needs an API that only Revit 2024 offers); the
reference's wall handling on Revit 2022, which the platform does not support; extending in
any direction other than up; any change to the loads.
