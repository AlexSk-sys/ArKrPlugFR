# Reassign rebar — functional specification

## Purpose
Attach existing rebar to the structure it actually belongs to. A bar that was placed
against the wrong column, wall, beam or floor gets its host changed with one run: real
bars are re-hosted, bars that arrived from IFC only get the mark of their new structure
written into their text parameter, and nothing else about the model changes.

## Inputs
- An open project document.
- The rebar to reassign, picked in the view; at least one element is required.
- The new host, picked in the view: one column, wall, beam or floor.
- The host's mark, read from the parameter named in the tool's settings file (default
  `Марка`; when empty, `Mark` is tried as well). A host without a mark stops the run.
- The mark parameter of IFC bars, read from the same settings file (default
  `ADSK_Метка основы`).

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Ask for the rebar. Cancelling the pick changes nothing.
3. Ask for the host. Cancelling the pick changes nothing.
4. Read the host's mark; when there is none, report it and change nothing.
5. Classify every picked element and act by its kind:
   - a real bar (Rebar): change its host to the picked structure;
   - a bar inside a system (RebarInSystem): skipped, and counted — the platform gives
     systems no way to change their host;
   - a family instance of the rebar category (a bar that arrived from IFC): write the
     host's mark into its mark parameter; its geometry is not touched;
   - anything else: skipped and counted.
6. All changes run in one transaction. A bar the platform refuses — for example one whose
   shape does not fit the new host — is reported with its id and the reason, and the run
   continues with the rest.
7. Report: how many bars were reassigned, how many IFC bars were updated, how many were
   skipped, how many were refused, with the first few refusals listed one by one.
8. Change nothing else: the host is never moved, no element is created or deleted, and the
   bars keep their geometry.

## Results and messages
- No active document: error, nothing changed.
- The pick of rebar or of the host cancelled: "The selection was cancelled." — nothing
  changed.
- The host has no mark: "The picked structure has no mark; fill it in before running the
  tool." — nothing changed.
- Success: "Reassigned: {0}; IFC marks updated: {1}; skipped: {2}; refused: {3}." —
  informational.
- Every message is provided in Russian and English; the result window carries a title, a
  status line and a close button, and closes on Escape.

## Edge cases and failures
- A bar whose shape does not fit the new host: the platform refuses, the refusal is
  reported, and other bars are still processed.
- A bar already attached to the picked host: the change is accepted and counted.
- An IFC bar without the mark parameter, or with it read-only: skipped and counted.
- A RebarInSystem: skipped and counted, never half-changed.
- A picked element of another category (a tag, a dimension): skipped and counted.
- Running the tool again with the same host: the bars are reassigned to the same host and
  the run reports no refusals; it is safe to run repeatedly.

## UI surfaces
- No window: the tool runs from the ribbon, asks for the two picks with its own prompts,
  and reports through the shared result window (caption, status line, close button,
  Escape to close).
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Rebar elements and their host, changed through the platform's own host setter, which
  refuses shapes the new host cannot carry.
- RebarInSystem elements, which share their host with the system and cannot be moved to
  another one.
- Family instances of the rebar category, used by bars that arrive from IFC; for them only
  the mark is written.
- The mark parameter of a structure, read by name.
- Transactions: the whole run is one undoable step and is rolled back when nothing was
  changed.

## Scope of this release
Implemented: everything described above — the two picks, the host mark, the three kinds of
elements, the skip reasons, one transaction, the bilingual prompts and report.

Not implemented: bars inside rebar systems (the platform gives them no host setter);
moving or reshaping bars; reassigning area, path or fabric reinforcement (this release
carries plain bars and IFC bars); and reading the host from a linked model.
