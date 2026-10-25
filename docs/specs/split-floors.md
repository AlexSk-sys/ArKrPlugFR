# Split floors into separate floors — functional specification

## Purpose
A slab is often modelled as one floor whose sketch holds several closed contours — one
region for every block or room. Schedules, tagging and worksets want the regions
separate. This tool reads a picked floor, finds the outer contour of every part, keeps
the openings of each part with it, and turns every part into its own floor.

## Inputs
- An open project with an active view and floors, picked in it. Optional: without a pick
  there is nothing to split.
- The floors must be flat: a floor is described by the outer and inner contours of its
  top face in the horizontal plane.

## Behaviour
1. Read the picked floors. An element that is not a floor is reported and skipped.
2. For every floor, read the contours of its top faces as polygons. A floor whose
   geometry yields no flat top face is reported and skipped.
3. Every contour is decided by the planar rules of the domain layer:
   - a contour with fewer than three points, or with an area under one square
     millimetre, is degenerate and is reported;
   - a contour that lies inside another contour belongs to the smallest contour that
     contains it;
   - a contour inside nothing is a part; a contour inside a part is one of its openings;
   - a contour inside an opening is not a valid floor profile and is reported;
   - two contours that each lie inside the other are one and the same; the later one is
     reported as a duplicate and does not become a second part.
4. A floor that holds fewer than two parts is reported as skipped: a single contour is
   nothing to split.
5. For every floor with two or more parts, as one undoable step of its own: create one
   floor per part — the part's outer contour plus the openings that belong to it — on
   the same level, with the same floor type, the same height offset, the same structural
   flag and the same workset as the source (the workset matters in a workshared project;
   one the platform refuses leaves the floor on the active workset); then delete the
   source floor. When any part of a floor cannot be created or cannot be given the offset
   of its source, that floor keeps its source and nothing of that floor changes; the
   other floors continue.
6. Report: how many floors were read, split, skipped and refused, how many floors were
   created, and the first lines of the reports one by one.

## Results and messages
- Nothing picked: "Pick the floors to split." — nothing changed.
- Success: "Read {0} floor(s); split: {1}; created: {2}; skipped: {3}; refused: {4}." —
  informational.
- Every message is provided in Russian and English; the window carries a title, a
  subtitle, the pick, clear, split and close buttons, and closes on Escape.

## Edge cases and failures
- A floor with a single contour: reported, nothing changed.
- Curved contours: the contours are tessellated on the Revit side before the planar
  rules run, so arcs and splines are handled like their polygon.
- A contour inside an opening, or a degenerate contour, or a duplicate: reported; the
  floor is still split around the remaining contours.
- A floor whose part cannot be created — a rejected profile, a locked floor: the floor
  is refused with the reason, keeps its source, and the run continues with the rest.
- The rejected profile of one part rolls back that floor only: the split of a floor is
  one undoable step.
- Running the tool again on the floors it created: every one of them holds a single
  contour and is skipped.

## UI surfaces
- A tool window: the pick button, the clear button, the line and list that count the
  picked floors, the split button, a report area and the close button. The window hides
  while the user picks in the model and returns after the pick.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the floors panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Floors and their geometry: the top planar face of every solid of a floor, and the
  edge loops of that face (`Face.GetEdgesAsCurveLoops`), tessellated into points for the
  planar rules.
- Creating a floor from a profile: `Floor.Create` on Revit 2022 and 2024; Revit 2020 uses
  the older `Document.Create.NewFloor`. The difference is isolated in the compatibility
  folder.
- The height offset of a floor is its `FLOOR_HEIGHTABOVELEVEL_PARAM` parameter.
- Transactions: the split of each floor is one undoable step of its own; a floor that
  cannot be split leaves the model unchanged.

## Scope of this release
Implemented: everything described above — the picked floors, the planar classification
of the contours into parts and openings, the per-floor split with level, type, height
offset, structural flag and workset carried over, the per-floor undoable step, the
bilingual window and report.

Not implemented: splitting floors that are not flat (a shape-edited or sloped floor is
reported and skipped), splitting by walls or grid lines instead of the contours the
sketch already holds, and copying instance parameters of the source floor beyond the
height offset, the structural flag and the workset.
