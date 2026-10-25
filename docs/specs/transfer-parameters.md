# Transfer parameters — functional specification

## Purpose
One value often has to appear in many places: a section or stage on the whole set of
sheets, the layer build-up of a wall in a text parameter, the number of a room on the
structures around it. Doing that by hand means opening element after element and typing
the same text. This tool holds three transfers, one per tab.

## Tab 1 — transfer a value to sheets
The user picks a source element in the model, chooses one of its parameters, sees the
value it holds, chooses a target parameter from the writable text parameters of the
sheets and ticks the sheets that receive the value.

1. The source element is picked in the model; its parameters fill the source list and the
   current value of the chosen one is shown. A parameter without a value reads as empty.
2. Every sheet of the project is listed; it can be ticked one by one or all at once.
3. On the run the value is read once. An empty value stops the run: there is nothing to
   transfer. In one transaction every ticked sheet receives the value; a sheet without the
   target parameter, one whose parameter is read-only or not a text parameter, is reported
   and skipped, and an element the platform refuses is reported while the run continues.
4. Report: sheets read, written, left without the parameter, and refused.

## Tab 2 — write the structure of a wall or a floor into a parameter
The user picks walls and floors in the model, names the parameter to write and chooses
whether the write goes to the instance (the picked elements) or to their type.

1. For every picked element the compound structure of its type is read layer by layer.
2. The layers become one text value, one layer per line: `1. Brick — 250 mm`. A layer
   without a material is printed as `?`; the width is printed in millimetres, rounded to a
   tenth of one.
3. An element without a compound structure — a basic wall without layers, a curtain wall —
   or with an empty structure is reported and skipped.
4. The write goes to the instance parameter first; when it is missing or cannot receive
   text — read-only or of another kind — the type parameter is used. A target that does
   not exist on either or cannot receive text is reported.
5. One transaction for the whole run.

## Tab 3 — spread a room value onto the structures around it
The user picks rooms in the model, chooses the source parameter of the room, names the
target parameter and ticks what receives the value: walls, floors, ceilings, the rooms
themselves.

1. For every room the value of the source parameter is read; a room without a value is
   counted as empty and skipped.
2. The walls come from the room's boundary segments. The floor under the room and the
   ceiling above it come from the room's geometry: the lowest horizontal face gives the
   floor, the highest gives the ceiling — when the model has no ceilings, that top element
   is the floor above, which is exactly what a floor-to-floor spread needs.
3. The value is written into the target parameter of every receiver — the instance
   parameter first, the type parameter second when the instance one is missing or cannot
   receive text. The room itself receives the value too when that is ticked, and a
   receiver that appears twice is written once.
4. One transaction for the whole run; every receiver left without the parameter is
   reported and the run continues.
5. The report also names the floors of the rooms that were left without the parameter, so
   the elements can be found in the model.

## Results and messages
- Sheet tab: "Nothing to transfer: the source parameter is empty." — nothing changed.
- Structure tab: "Enter the name of the parameter to write." — nothing changed.
- Rooms tab: "Choose at least one receiver: walls, floors, ceilings or the rooms." —
  nothing changed.
- Reports name the elements they left out, one line each, and are bilingual.

## UI surfaces
- One window with three tabs, a shared report area and a close button. Every tab has its
  own pick button (the window hides while the user picks in the model), its own apply
  button and its own hints; the picks and the report live in the view model.
- The ribbon button carries the tool caption and a bilingual description; the tool sits
  in the data panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Parameters: the instance parameter is read first and the type parameter second; only
  text parameters receive a value; a value of another kind is read as its displayed text.
- Sheets: the `ViewSheet` elements of the project.
- Compound structure: `HostObjAttributes.GetCompoundStructure` and its layers
  (`CompoundStructureLayer`), the material of a layer and its width.
- Rooms: the boundary segments (`Room.GetBoundarySegments`) for the walls and the room
  geometry (`SpatialElementGeometryCalculator`) for the floor below and the ceiling above.
- Transactions: one per run, and a run that changes nothing leaves no entry in the undo
  list.

## Scope of this release
Implemented: everything described above — the three tabs, the value transfer to sheets,
the layer text of walls and floors, the room spread, the counts and the lines about what
was left out, and the domain rules covered by unit tests without Revit.

Not implemented: writing into a parameter that is not a text parameter; a separate target
parameter name per receiver; transferring into the elements of linked models — values
stay in the current project; and the modal list of floors left without the parameter that
the replaced tool showed (here the same elements are named in the report).
