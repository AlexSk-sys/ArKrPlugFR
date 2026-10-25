# Unjoining columns and beams from walls — functional specification

## Purpose

Remove the cuts that columns and beams make in the walls around them. A structural model joins
a beam or a column to a wall so the wall shows a clean opening; on a drawing that opening is
often unwanted, and removing the joins by hand — one pair at a time, with the join tool — is
slow and easy to get wrong. The tool removes every such join in the selected walls in one
undoable step and reports what it could not change.

## Inputs

- An open project document.
- The walls to clean up, selected in the model: walls, or groups that contain walls.
- Which neighbours to remove the cuts of; the tool has two buttons for that:
  - columns — structural columns and architectural columns;
  - beams — structural framing.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the selection, expanding groups into their members. Without a selection, or without a
   wall among the selected elements, report it and change nothing.
3. For every selected wall, ask the platform which elements it is joined to. The columns or
   beams of the whole project are read once and only the ones that share space with a wall —
   their bounding boxes overlap within the search tolerance — are kept.
4. Rule out the pairs that are not a cut between the wall and a neighbour of the chosen kind,
   so the run offers only pairs the tool can actually change.
5. In one transaction, for every offered pair: ask the platform whether the two elements are
   still joined, unjoin them, and read the state back. A pair that is no longer joined is
   skipped, not counted as a failure; a pair that stays joined after the call, or whose call
   throws, is counted as a failure and listed with both elements.
6. Report: the document, the selected walls, the cuts found, the cuts removed, the cuts that
   could not be changed with both elements listed, and, when nothing needed removing, that no
   cut was found.
7. Change nothing else: no element is created, moved or deleted; only the joins between the
   selected walls and their columns or beams change, and the whole run is one undoable step.

## Results and messages

- No active document: error, nothing changed.
- Nothing selected: "Nothing is selected" with a hint to select walls and run again.
- No wall in the selection: "No walls in the selection" with a hint that columns and beams are
  searched across the whole project.
- No cut found: "Unjoin is not required" — nothing changed.
- Success: the document title, "Unjoined successfully", the counts of walls, cuts found, cuts
  removed and failures, followed by one line per removed pair and one line per failed pair.
- Every message is provided in Russian and English; the result is shown in the shared result
  window.

## Edge cases and failures

- A wall without geometry: skipped; a selection of such walls only counts as "no walls".
- A pair the platform refuses: reported with both element descriptions, and the run continues.
- A pair the model has already unjoined between the run's search and its edit: skipped
  silently, because nothing has to change.
- Joins between the selected walls and other kinds of elements (walls to walls, walls to
  floors): left alone.
- Running the tool twice: the second run finds no cut and reports that unjoin is not required.

## UI surfaces

- Two ribbon buttons in the walls panel, one for each kind of neighbour, each with a bilingual
  caption and description; there is no tool window, the result is shown in the shared result
  window.
- All text is provided in Russian and English.

## Revit concepts involved

- `JoinGeometryUtils.GetJoinedElements` as the cheap question "what is joined to this wall?",
  and `JoinGeometryUtils.AreElementsJoined` / `UnjoinGeometry` as the check and the edit.
- Bounding boxes of the walls and of the neighbours, and the search tolerance of the domain
  rule, to keep the offered pairs to elements that can really share a cut.
- Categories of walls, structural and architectural columns, and structural framing.
- Transactions: the whole run is one undoable step and is rolled back when nothing changed.

## Scope of this release

Implemented: everything described above — the two buttons, the selection with groups, the
bounding-box prefilter, the join rule, the read-back after every unjoin, the counts and the
failures, one transaction, the bilingual report.

Not implemented: unjoining walls from anything but columns and beams; a choice of walls from
the whole project without selecting them; undoing one pair out of a run; any change to the
geometry or the parameters of the elements.
