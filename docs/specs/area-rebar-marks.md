# Fill area reinforcement values onto its bars — functional specification

## Purpose
Area reinforcement carries the data of the whole field: its mark, its host, its stage.
The bars that make up the field are separate elements and stay empty, so every schedule of
the bars has to be read next to the field it belongs to. This tool fills the bars from
their field: one chosen parameter of every area reinforcement is written into the same
parameter of the bars that belong to it.

## Inputs
- The scope: the whole project, or the area reinforcement elements picked in the model.
- The name of the parameter to transport; the window offers the names that carry a value
  on at least one of the sources.
- The switch "only bars without a value": on by default, so an existing bar value is kept
  unless the user asks for it to be overwritten.

## Behaviour
1. Require a scope with at least one area reinforcement and a usable parameter name;
   without them, report and change nothing.
2. Read the bars of the project once and group them by their host
   (`Rebar.GetHostId`), so the bars of every field are found without walking the model
   again.
3. For every source: read the chosen parameter. A source whose value is empty is counted
   and skipped; a source without bars is counted and skipped.
4. Write the value into the chosen parameter of every bar of the source — the instance
   parameter first, the type parameter second. A bar that already carries a value is
   counted and kept when the switch says so; a bar that refuses the value — a missing
   parameter, a read-only one, one that is not a text parameter — is reported and the run
   continues.
5. Report: sources read, sources without a value, sources without bars, filled bars,
   bars kept, refused.
6. Change nothing else: no geometry, no other parameter, no element is created or deleted.

## Results and messages
- No source in the scope: "There is no area reinforcement in the scope." — nothing changed.
- No usable parameter name: "Choose the parameter to fill the bars from." — nothing changed.
- Success: "Sources read: {0}; without a value: {1}; without bars: {2}; bars filled: {3};
  bars kept: {4}; refused: {5}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the scope switch, the pick button with its line, the parameter list, the bar switch, a
  report area and the run and close buttons, and closes on Escape.

## Edge cases and failures
- A source without a value: skipped, counted.
- A source without bars (a boundary-only definition): counted.
- A bar whose host is another element: never seen — only the bars of the chosen sources are
  filled.
- Bars shared between sources: impossible, a bar has one host; a bar of a source that
  appears twice in the scope is written twice with the same value and counted once per
  source.
- Thousands of bars: the bars are read once; the whole run is one undoable step, and a run
  that writes nothing leaves no entry in the undo list.

## UI surfaces
- A tool window: the switch that chooses the whole project or the picked fields, the pick
  button and the line that counts what it picked, the parameter list, the switch that
  keeps existing bar values, the hint, a report area and the run and close buttons. The
  scope and the parameter are remembered between sessions.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Area reinforcement: the `AreaReinforcement` elements of the project.
- Bars: rebar elements whose host (`Rebar.GetHostId`) is the area reinforcement.
- Parameters: read as text, written into text parameters only, the instance parameter
  first and the type parameter second.
- Transactions: the whole run is one undoable step.

## Scope of this release
Implemented: everything described above — the two scopes, the parameter list, the value
transport to the bars of every field, the switch that protects existing values, the counts
and the report.

Not implemented: the live preview line of the replaced tool (the report counts what
happened after the run); transporting into the bars of linked models; and writing a value
that is not text — a number parameter is read but not written.
