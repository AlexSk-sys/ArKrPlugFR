# Pile data for schedules — functional specification

## Purpose
Piles need three things before a schedule of them can be issued: a number on the plan, the
elevation of their bottom and of the raft over them, and the height of the part that will
be cut off under the raft. Reading that off the model by hand means measuring every pile
against the raft. This tool does the measuring: the user picks the piles and the raft
slabs, and each action writes one set of values into the parameters of the piles. It only
writes parameter values — the geometry is never touched.

## Inputs
- The piles, picked in the model: the usual pile families.
- The raft slabs, picked in the model: the floors or family elements the piles run into.
  The numbering needs no slabs.
- The start number, by default 1.
- The direction of the numbering: top-down (the default) counts from the largest Y down,
  bottom-up from the smallest Y up; ties are counted left to right by X.
- The names of the four parameters: the number (`O_Позиция` by default), the cut height
  (`Высота срубаемой части`), the elevation of the raft bottom (`Отметка низа плиты`) and
  the elevation of the pile bottom (`Отметка низа сваи`).

## Behaviour
1. Numbering: order the picked piles — by their location point, or by the centre of their
   bounding box when a pile has none — with the chosen direction, and write the running
   numbers into the number parameter, the first one being the start number.
2. Elevations: read the lowest point of the picked slabs and of every pile and write both,
   in millimetres, into the two elevation parameters. Without a picked slab the action
   stops and changes nothing.
3. Cut height: for every pile find the slab whose bottom lies nearest below the top of the
   pile among the slabs that overlap the pile in plan; the cut height is the part of the
   pile above that bottom, never more than the pile itself. A pile without such a slab is
   counted and skipped.
4. Every action writes text parameters only; a value the parameter refuses — a missing
   parameter, a read-only one, one that is not a text parameter — is reported and the run
   continues. Values are written without a unit, in millimetres, with up to three decimals.
5. Every action is one undoable step of its own; an action that writes nothing leaves no
   entry in the undo list.
6. Report per action: piles read, values written, skipped, refused.

## Results and messages
- Nothing picked: "Pick the piles first." — nothing changed.
- Elevations without a slab: "Pick the raft slabs first." — nothing changed.
- Success: "Read {0} pile(s); written: {1}; skipped: {2}; refused: {3}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the two picks with their lines, the settings, three action buttons, a report area and the
  close button, and closes on Escape.

## Edge cases and failures
- A pile without a location point: numbered by the centre of its box.
- Two piles at the same place: the earlier one in the reading order comes first.
- A pile that does not reach the slab, or does not overlap it in plan: no cut height; the
  pile is counted.
- A pile whose cut height comes out zero or negative: nothing is written, the pile is
  counted.
- A parameter of a type shared by many piles: writing changes the type, which the platform
  applies to every pile of that type — the platform's behaviour, not something the tool
  arranges.

## UI surfaces
- A tool window: the button that picks the piles and the one that picks the slabs, each
  with the list of what it picked, the start number, the direction switch, the four
  parameter names, the three action buttons, a report area and the close button. The
  settings are remembered between sessions.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the foundations panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Piles and slabs: family instances and floors, their location points and their bounding
  boxes in model coordinates.
- Elevations and lengths: model coordinates are in feet, the values are written in
  millimetres through the domain units.
- Parameters: written as text, the instance parameter first and the type parameter second.
- Transactions: every action is one undoable step of its own.

## Scope of this release
Implemented: everything described above — the numbering with its direction and start
number, the two elevations, the cut height against the picked slabs, the four parameter
names, the counts and the report.

Not implemented: the ranges of positions for the two schedules of the replaced tool; the
automatic detection of pile families by name (the user picks the piles); the anchor and
test pile letters (Р/А/И) of the replaced tool; and elements of linked models.
