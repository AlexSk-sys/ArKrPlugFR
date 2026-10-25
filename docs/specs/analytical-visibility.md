# Analytical model in the views — functional specification

## Purpose

Take the analytical model out of the way while the physical model is being worked on, and
bring it back when the calculation is looked at again. A dense analytical model clutters
every plan and section, and switching it off by deleting it would take the loads and the
boundary conditions with it. This tool never deletes anything: it hides the analytical
elements in the views of the model and shows them again on demand.

## Inputs

- An open project document.
- Which way the model should go: hidden or shown; the two directions are two commands.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Collect the elements of the analytical model of the project: on Revit 2020 and 2022 the
   analytical model that belongs to a physical element, plus the analytical members of 2022
   on; on Revit 2024 the analytical members and the analytical panels. The release
   difference lives in the compatibility layer.
3. Collect the views the model is looked at in: three-dimensional views, floor, ceiling and
   engineering plans, sections, details and elevations; templates are left out, and so are
   schedules, sheets, legends and reports.
4. In one transaction, for every such view and every analytical element:
   - an element that is already hidden (or already visible, when the model is being shown)
     is left alone and counted;
   - an element the platform does not allow to be hidden is counted as unchanged;
   - otherwise the element is hidden or shown, and a refusal by the platform is reported
     with the element, the view and the reason, and the run continues.
5. Report: how many analytical elements and views were considered, how many changed, how
   many were already in the asked-for state, how many could not be changed, how many were
   refused, and the first few refusals one by one. The report always states that nothing was
   deleted.
6. Change nothing else: no element is created, moved or deleted; only the per-view hiding
   changes, and the run is one undoable step.

## Results and messages

- No active document: error, nothing changed.
- No analytical model in the project: "The project holds no analytical model." — nothing
  changed.
- No view that shows the model: "The project has no views that show the model." — nothing
  changed.
- Everything already in the asked-for state: the counts say so and the report adds
  "Everything is already in the state the command asked for."
- Success: "Analytical elements: {0}; views: {1}; changed: {2}; already in place: {3};
  cannot be changed: {4}; refused: {5}."
- Every message is provided in Russian and English; the result is shown in the shared result
  window.

## Edge cases and failures

- An element the platform refuses to hide in one view: reported, counted, and the run
  continues with the next element.
- An element that is not visible in a view for another reason (a hidden category, a view
  range): the platform reports it as not hidden, and a refusal is reported rather than
  hidden silently.
- Running the hiding twice: the second run changes nothing and says so.
- Showing after hiding returns every element the hiding took away, because nothing else
  was changed in between.

## UI surfaces

- Two ribbon buttons in the loads panel, one for each direction, each with a bilingual
  caption and description; there is no tool window, the result is shown in the shared
  result window.
- All text is provided in Russian and English.

## Revit concepts involved

- The elements of the analytical model, collected per release: the model attached to a
  physical element on 2020 and 2022, the analytical members from 2022 on and the analytical
  panels on 2024.
- `View.HideElements`, `View.UnhideElements`, `Element.IsHidden(View)` and
  `Element.CanBeHidden(View)` as the per-view hiding of one element at a time, so one
  refusal does not take the rest of the run down with it.
- Transactions: the whole run is one undoable step and is rolled back when nothing changed.

## Scope of this release

Implemented: everything described above — both directions, every model view, the per-element
decision, the refusals with their reasons, one transaction, the bilingual report.

Not implemented: the reference's promise of saving and re-creating loads around a real
disabling of the analytical model. This tool does not disable the analytical model at all —
it hides it — so there is nothing to save: the loads, the boundary conditions and the cases
stay exactly where they were. Disabling the analytical model itself is left to the platform,
which deletes it and everything attached to it.
