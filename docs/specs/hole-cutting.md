# Hole cutting — functional specification

## Purpose
Cut a rectangular opening in a wall where a service crosses it. The user selects one
wall and one service element — a duct, a pipe, a cable tray or a conduit — and the tool
works out where that service passes through the wall and cuts an opening around the
crossing, larger than the crossing by a technological clearance on every side.

## Inputs
- An open project document; the tool works on the current selection and shows no window.
- Exactly one wall and exactly one service element of a supported kind must be selected.
  Supported kinds: ducts, pipes, cable trays, conduits.
- The clearance on every side of the crossing, in millimetres. It is a default of the
  design — 50 mm — and is not asked for in the window; every number here is a
  configurable default, not a fixed constraint.
- The wall must be straight: its location curve is a line.

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the selection. Unless it holds exactly one wall and exactly one supported service
   element, report what was found and change nothing; no transaction is opened.
3. Work out the crossing of the service and the wall: the solid of the service
   intersected with the solid of the wall. When either element yields no usable solid,
   fall back to the intersection of the two bounding boxes.
4. Read the crossing in the wall's own frame: how far it reaches along the wall from the
   wall's base point, and how high it reaches above that point. A service that crosses at
   an angle therefore produces the extent it really covers, not just its own width.
5. Add the clearance on all four sides: the opening is as large as the crossing plus the
   clearance on each side.
6. Cut the opening in the wall from the two opposite corners of that rectangle, both
   corners lying in the wall's plane, so the opening is cut square to the wall.
7. Report the size of the crossing and the size of the opening, in millimetres, together
   with the name of the wall.
8. Change nothing else: no element is moved, copied or deleted, the service is left as it
   is, and no temporary element is created anywhere in the project.

## Results and messages
- Empty or unusable selection: "Select one wall and one duct, pipe, cable tray or
  conduit." — error, nothing changed.
- Selection holding other elements: "The selection must hold one wall and one service
  element: a duct, a pipe, a cable tray or a conduit." with what was found instead.
- No crossing: "The service does not cross the wall." — error, nothing changed.
- Crossing too small to cut: "The crossing is too small to cut an opening in." — error,
  nothing changed.
- Curved wall: "Only straight walls are supported; the selected wall is curved." — error,
  nothing changed.
- Platform refusal: "The opening could not be cut: REASON" — error, the change is rolled
  back.
- Success: "Opening W × H mm cut in wall 'NAME' (crossing W × H mm)." — informational.
- Every message is provided in Russian and English; the result window carries a title, a
  status line and a close button, and closes on Escape.

## Edge cases and failures
- No active document, or a document closed between the command starting and the run:
  nothing is cut.
- Two walls, two services, a service of an unsupported kind (a fitting, an air terminal,
  a family instance of any other category), or a linked element: reported, nothing cut.
- A service that passes through the wall more than once is covered by a single opening
  around the whole crossing, because the crossing of the solids describes both passes as
  one shape.
- A service that merely touches the wall face without crossing it: the crossing has no
  usable size, and the run stops with the message above.
- An opening that would swallow the wall (a crossing as large as the wall itself): the
  platform may refuse it; the refusal is reported and the wall is left alone.
- An element deleted or changed while the command runs: reported, the change is rolled
  back.
- The tool cuts an opening of the platform itself, so no family has to be loaded and no
  view has to be open.

## UI surfaces
- No window: the tool runs from the ribbon on the current selection and reports through
  the shared result window (caption, status line, close button, Escape to close).
- The ribbon button carries the tool caption and a bilingual description.
- All text is provided in Russian and English.

## Revit concepts involved
- Wall, its location curve (a line), its base point and direction, and its plane.
- Duct, pipe, cable tray and conduit elements and their categories.
- Solid geometry of an element, the boolean intersection of two solids, and bounding
  boxes as the fallback when geometry is not available.
- The platform's rectangular opening on a wall, defined by two opposite corners in the
  wall's plane.
- Transactions: the opening is cut in one undoable step and rolled back when the platform
  refuses it.
- Selection of the active document.

## Scope of this release
Implemented: everything described above — the selection check, the crossing as the
intersection of the two solids with a bounding-box fallback, the projection of the
crossing onto the wall's frame, the clearance on all four sides, the native rectangular
opening, the bilingual messages and the report of both sizes.

Not implemented: cutting with a void family of a chosen opening family (a native
opening is used instead, which is why nothing has to be loaded into the project); round
or shaped openings; openings in floors, ceilings, roofs, columns or shafts; cutting in
one step from a whole selection of walls and services (one wall and one service per run);
and a settings file for the clearance, which is a default of the design for now.
