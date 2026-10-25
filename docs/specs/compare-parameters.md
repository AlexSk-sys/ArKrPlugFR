# Compare and copy element parameters — functional specification

## Purpose
When one element behaves unlike its neighbours, the difference usually sits in a parameter:
a lost host, a cleared mark, a wrong type value. This tool puts two elements side by side —
the source that works and the target that does not — lists what differs, and copies the
values of the source into the target, so the target is brought back in line in one step.

## Inputs
- A source element picked in the model: the one that carries the values worth keeping.
- A target element picked in the model: the one to fix.
- Both picks can be redone at any time; the comparison follows the picks.

## Behaviour
1. Pick the source element and the target element. The comparison lists the parameters by
   name in alphabetical order: the names of both elements together, instance parameters and
   the parameters of their types.
2. A row shows the value of the source and the value of the target; a row whose values
   differ is marked as a difference. Absent or empty values read as empty.
3. Copy: for every row that differs, the value of the source is written into the target —
   the instance parameter first, the type parameter second. A row whose source value is
   empty is counted and left alone: the tool fills, it never clears.
4. A value the target refuses — a missing parameter, a read-only one, one that is not a
   text parameter — is reported; the run continues with the rest.
5. Report: rows compared, rows that differ, values copied, values skipped, refusals.
6. Change nothing else: the source element is never written to, no geometry is touched, no
   element is created or deleted.

## Results and messages
- Nothing picked: "Pick the source element and the target element." — nothing changed.
- Nothing to copy: "The source carries no value the target is missing." — informational.
- Success: "Parameters compared: {0}; different: {1}; copied: {2}; skipped: {3}; refused:
  {4}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the two pick buttons with their lines, the comparison list, the copy button, a report
  area and the close button, and closes on Escape.

## Edge cases and failures
- A parameter that exists on one element only: still a row; the empty side reads as empty.
- Values that differ in surrounding spaces only: not a difference.
- A source value that is empty: skipped on copy.
- The same element picked twice: every row is equal and nothing is copied.
- A parameter of a type that many elements share: writing it changes the type, which the
  platform applies to every element of that type — that is the platform's behaviour, not
  something the tool arranges.
- The copy is one undoable step; a run that copies nothing leaves no entry in the undo list.

## UI surfaces
- A tool window: the two pick buttons (the window hides while the user picks in the model
  and returns after the pick), the list of the comparison, the copy button, a report area
  and the close button.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the data panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Parameters: the instance parameters of an element and the parameters of its type, read as
  text — numbers and lengths read as the text the interface shows.
- Writing: the instance parameter first, the type parameter second; only text parameters
  receive a value.
- Transactions: the whole copy is one undoable step.

## Scope of this release
Implemented: everything described above — the two picks, the comparison, the differences
and the copy of the values, with the counts and the report.

Not implemented: comparing more than two elements at once; copying parameter values
between projects or from a linked model (the tool works inside the current project); and
copying values that are not text — a number parameter is read but not written.
