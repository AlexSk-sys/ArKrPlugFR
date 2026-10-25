# Analytical alignment — functional specification

## Purpose

Line the analytical model up with a member that is already right. A calculation model is
built from many members, and a single analytical member — the one that was checked against
the drawing — can serve as the target: the others are brought to its height, and the
analytical nodes around it are brought onto its line. The geometry of the model itself is
not touched; only the analytical elements move.

## Inputs

- An open project document.
- A target element: an element whose location is a curve, for example an analytical member
  that already sits where it belongs.
- The elements to move, picked in the model:
  - for the member alignment, any number of elements with a location curve;
  - for the node alignment, any number of analytical nodes (reference points).

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the curve of the target: the two ends of its location line. A target without a
   location line is reported and nothing is changed.
3. **Align members.** For every picked element with a location line, work out the vertical
   offset that brings the element into the plane of the target:
   - where the two lines cross in plan, the height of the target at the crossing minus the
     height of the element at the crossing;
   - where they do not cross in plan (parallel, or crossing beyond their ends), the
     difference of the heights of their midpoints.
   Then move the element by that offset along the vertical axis. An element that is already
   at the right height is left alone and counted.
4. **Align nodes.** For every picked node, work out the point of the target line nearest to
   the node and move the node there; a node already on the line is left alone and counted.
   Every element of the analytical model whose location line had an end at the node's old
   place is dragged with it: the end moves to the node's new place, so the member stays
   connected to its node.
5. Run every move in one transaction and report: how many elements or nodes were moved, how
   many were already in place, how many were skipped and how many the platform refused.
6. Change nothing else: no physical element, no load and no boundary condition is touched;
   only the analytical elements and nodes move, and the run is one undoable step.

## Results and messages

- No active document: error, nothing changed.
- No target picked: the run reports it and changes nothing.
- No element or node picked: the run reports it and changes nothing.
- The target has no location line: reported, nothing changed.
- Success: "Moved: {0}; already in place: {1}; skipped: {2}; refused: {3}." and, for the
  node alignment, how many members were dragged along.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A target line that is vertical in plan: the crossing rule cannot be used, so the midpoint
  rule decides the offset.
- An element whose location is not a line: skipped and counted.
- A node whose nearest point on the target line lies beyond the end of the target: the node
  moves to the end of the target, never past it.
- A member the platform refuses to move: reported with its id and the reason, and the run
  continues.
- Running the alignment again moves nothing and reports that everything is already in place.

## UI surfaces

- A tool window: the target pick, the two element picks, the two actions, a report area and a
  close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the loads panel.
- All text is provided in Russian and English.

## Revit concepts involved

- Analytical members and other curve-based elements: their location line is the curve the
  run reads and, for a node, the curve it drags.
- Analytical nodes as reference points; their position is read and written directly.
- `ElementTransformUtils.MoveElement` for the member alignment, so the element keeps its
  identity and its parameters.
- Transactions: each action is one undoable step.
- The analytical model of Revit 2020 has a different shape and no reference-point nodes, so
  the tool is offered on Revit 2022 and 2024 only.

## Scope of this release

Implemented: everything described above — the two alignments, the vertical offset rule with
its midpoint fallback, the node projection with its clamp, the dragged member ends, the
counts and the refusals, one transaction per action, the bilingual window and report.

Not implemented: the reference's third action (projecting lines onto a surface); moving a
node's members when the member is not part of the analytical model; alignment in the
horizontal plane; any change to the physical model.
