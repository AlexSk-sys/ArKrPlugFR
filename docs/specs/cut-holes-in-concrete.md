# Cut holes in concrete — functional specification

## Purpose
Cut rectangular openings in the concrete walls, floors and columns of the current project
where the equipment and services of a linked model pass through them. The user picks the
linked model; the tool reads what crosses the concrete and cuts one native opening per
crossing, larger than the crossing by a clearance on every side. Nothing is loaded into
the project and no temporary element is created: the model gains openings and nothing else.

## Inputs
- An open project document with at least one loaded Revit link.
- One linked model, chosen by the user in the view (a picked `RevitLinkInstance`). The link
  must be loaded; a link that is only attached as a background is reported and skipped.
- The clearance on every side of a crossing, in millimetres: a whole number from 0 to 500,
  by default 50. The value is remembered between sessions.
- The equipment side of the link is read from these categories:
  mechanical equipment, generic models, ducts, pipes, cable trays, conduits.
- The concrete side of the project is read from these categories: walls, floors,
  structural columns and architectural columns.

## What counts as concrete
An element is concrete when any of the following names contains the word "concrete" or
"бетон", compared without case:
- the name of the element's type, or the name of its family for a family instance;
- the name of every material assigned through a parameter of the element or of its type.

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Read the linked document of the picked link. Unloaded links are reported and skipped.
3. Collect the equipment elements of the categories above from the linked document, with
   their geometry carried into the coordinates of the current project by the link's total
   transform. Elements without geometry are skipped.
4. Collect the concrete elements of the current project from the categories above, with
   their geometry read in place.
5. For every equipment element and every concrete element, test the two boxes first; only
   the pairs that touch closely are tested further.
6. Work out the crossing as the intersection of the two solids. When either element yields
   no usable solid, fall back to the intersection of the two boxes.
7. Cut only where the equipment really passes through: the crossing has to cover the whole
   thickness of the concrete element along the direction of passage, within a tolerance of
   1 mm. An equipment that ends inside the concrete, or only grazes it, is counted and
   skipped — no opening is cut for it.
8. Build the opening rectangle:
   - A wall: the crossing is read in the wall's own frame and grown by the clearance; the
     opening is cut square to the wall from two opposite corners of that rectangle.
   - A floor: the crossing's footprint is grown by the clearance and clamped to the
     footprint of the slab; the opening is cut vertically through the slab.
   - A column: the direction of passage is the longest side of the equipment's box; the
     crossing is measured across that direction, grown by the clearance and clamped to the
     column's cross-section; the opening is based on the middle plane of the column
     perpendicular to that direction.
9. An opening whose rectangle would be shorter than 10 mm on any side is not cut; it is
   counted and skipped.
10. An opening that would repeat one already cut earlier in the same run, or one already
    present in the same element, within 10 mm on every edge, is not cut; it is counted.
11. Cut all planned openings in one transaction, so a run is one undoable step. When the
    platform refuses one opening, the refusal is reported with the element it belongs to
    and the rest of the run continues.
12. Report: how many equipment elements were read, how many concrete elements were found,
    how many openings were cut, how many were already there, how many were skipped because
    the equipment does not pass through, how many were too small, and how many were
    refused, with the first few refusals listed one by one.
13. Change nothing else: the linked model is never modified, no element of the current
    project is moved, copied or deleted, and the only new elements are the openings.

## Results and messages
- No live link picked: "The selection was cancelled." — nothing changed.
- The picked link is not loaded: "The linked model has to be loaded; it is not available in
  this project." — nothing changed.
- Nothing to cut from the link: "The linked model holds no equipment or services in the
  supported categories." — nothing changed.
- No concrete in the project: "The project holds no concrete walls, floors or columns." —
  nothing changed.
- Success: counts of equipment, concrete elements, openings cut, openings already there,
  equipment skipped as not passing through, crossings too small, and refusals — each
  refusal listed with its element.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures
- No active document, or a document closed between the command starting and the run:
  nothing is cut.
- An unloaded or detached link: reported, nothing is cut.
- A curved wall cannot be read as a frame: reported with the wall's name, nothing is cut in
  that wall.
- A slanted stacked wall refuses rectangular openings: the refusal is reported, the wall is
  left alone.
- A floor or a column read from a shape the platform cannot cut: the refusal is reported,
  the element is left alone.
- Floors are handled as horizontal slabs: the through test uses the slab's own box, and
  sloped slabs may therefore be reported as not passing through. This is a known
  limitation, not a defect of the run.
- An equipment element that crosses several concrete elements cuts one opening in each.
- Two equipment elements crossing the same place produce one opening, not two.
- Re-running the tool after a run cuts nothing new: the openings of the previous run are
  recognised within 10 mm.
- The opening family of a project is irrelevant: the tool uses the platform's own opening,
  so a project without any opening family is cut exactly like any other.

## UI surfaces
- A tool window: the clearance field, a hint about the linked model, an apply button
  ("Pick a linked model and cut") and a close button. The window hides while the user picks
  the link and returns after the pick; the result is shown in the window and in the shared
  result window.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the openings panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Revit links: `RevitLinkInstance`, its linked document and total transform.
- Categories of equipment and services, and of walls, floors and columns.
- Solid geometry of an element, the boolean intersection of two solids, and bounding boxes
  as the fallback when geometry is not available.
- Family parameters and material parameters as the source of "what is concrete".
- The platform's openings: a rectangular opening on a wall from two corners in its plane; a
  vertical opening through a floor from a closed profile; an opening in a column based on
  the middle plane perpendicular to one of the three axes.
- Openings already present in an element, recognised by their own boxes.
- Transactions: the whole run is one undoable step; a refusal of one opening does not stop
  the rest.

## Scope of this release
Implemented: everything described above — the linked model as the source of equipment and
services, concrete recognition from family, type and material names, the crossing from the
boolean intersection with a bounding-box fallback, the through test against the thickness
of the host, the clearance, the clamping to the host's cross-section for floors and
columns, openings cut natively in walls, floors and columns, the duplicate check within
the run and against the elements' existing openings, one transaction for the whole run,
the bilingual window and report, and the remembered clearance.

Not implemented: cutting by a void family of a project's own opening family (native
openings are used instead, which is why nothing has to be loaded); round or shaped
openings; openings in roofs, ceilings, foundations and shafts; slabs handled as sloped
surfaces rather than through their box; equipment from the current project rather than
from a link; and a preview of the planned openings before the cut.
