# Line loads on walls — functional specification

## Purpose

Prepare the calculation model of a floor: put the weight of its partition walls onto the
slabs as line loads. Every partition carries a load per metre of its length — its own
weight, usually with a small moment — and the load belongs at the bottom of the wall, where
the wall meets the slab, not on top of it.

## Inputs

- An open project document.
- The walls to load, chosen in one of three ways: the walls selected in the model, every
  wall visible on the active view, or the walls of one workset.
- The load case: its name, typed in the window or chosen from the names already in the
  project; a name that does not exist yet is created by the run.
- `Fz` — the force along the vertical axis, in kilonewtons per metre; a positive value
  points down.
- `Mz` — the moment about the vertical axis, in kilonewton metres per metre; zero is a
  valid value.
- Whether the load is uniform along the wall, and an optional comment for the load.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the walls of the chosen scope, ordered by element id. A wall that is not a straight
   wall — no location curve, or a curve that is not a line — is skipped and reported.
3. Refuse a load whose force and moment are both zero: the platform rejects it, so the run
   reports it and changes nothing.
4. Find the load case by name among the load cases of the project, comparing names without
   case; when none carries the name, create one — with a load nature of the project, or a
   new nature when the project has none — in the category of dead loads.
5. In one transaction, for every wall of the scope:
   - take the wall's location line; it lies at the base of the wall, which is the bottom of
     the partition;
   - create a non-hosted line load along that line, with the force vector `(0, 0, −Fz)` and
     the moment vector `(0, 0, Mz)`, both in the internal units of the platform;
   - attach the load to the load case;
   - a uniform load carries the same vectors at both ends of the line; a load that is not
     uniform tapers to zero at the far end;
   - write the comment into the load's comment parameter when one is given.
6. Report: the scope, how many loads were created, how many walls were skipped and why, how
   many the platform refused, the load case, the force and the moment, and whether the loads
   are free or stayed on a host.
7. Change nothing else: no wall is moved or edited; the run adds loads and, when the name is
   new, one load case.

## Results and messages

- No active document: error, nothing changed.
- No walls in the scope: "The chosen scope holds no walls." — nothing changed.
- No wall with a straight location line: reported, nothing changed.
- Force and moment both zero: reported, nothing changed.
- Success: "Created {0} line load(s) from {1} wall(s); skipped: {2}; refused: {3}; load case
  «{4}», Fz = {5} kN/m, Mz = {6} kN·m/m."
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A curved or otherwise non-straight wall: skipped and reported with its id.
- A wall the platform refuses (for example one inside a group that cannot take a load):
  reported with its id and the reason; the run continues with the next wall.
- A load case whose name differs only in case from an existing one: the existing case is
  used, no second case is created.
- Running the tool again creates a second set of loads: the tool does not remove loads that
  already exist, because a load may have been edited by hand. The report says how many were
  created.

## UI surfaces

- A tool window: the scope with its three choices and the workset list, the load case, the
  force, the moment, the uniformity switch, the comment, a run button, a report area and a
  close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the loads panel.
- All text is provided in Russian and English.

## Revit concepts involved

- `Wall` elements, their `LocationCurve` and the active view as the scope of the run.
- `LineLoad` and the two ways the platform offers to create one without a host: on Revit
  2020 and 2022 a non-hosted load is created directly, with a horizontal sketch plane at the
  level of the load; on Revit 2024 the platform creates loads only on an analytical host, so
  the load is created on the wall's analytical element and then released from its host. The
  difference lives in `Compatibility`; a wall without an analytical element is skipped on
  2024 and the report says so.
- `LoadCase`, `LoadNature` and the load case categories.
- Unit conversion: the force and the moment are typed in kilonewtons and converted to the
  internal units of the platform through the compatibility layer.
- Transactions: the whole run is one undoable step.

## Scope of this release

Implemented: everything described above — the three scopes, the load case by name, the two
components, the uniformity switch, the comment, one transaction, the bilingual window and
report.

Not implemented: the reference's fallback marker line for a wall whose load could not be
created (the tool reports the wall instead, so nothing is drawn into the model that was not
asked for); curved walls; loads attached to a floor or a beam; editing or removing loads
that already exist; walls from linked models.
