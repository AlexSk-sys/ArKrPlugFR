# Mark structures by their kind — functional specification

## Purpose
A structural model wants a mark on every column, beam, floor, wall, foundation and pile,
and the mark should say what the element is: a column of concrete, a beam of steel. Doing
that by hand means selecting one group after another. This tool walks the model itself:
it decides the kind of every structure from its category and its shape, decides the
material family from the materials of the element, and writes the mark — one configurable
code per kind plus a suffix for the material — into the mark parameter.

## Inputs
- The whole project: every instance of the supported categories — structural columns,
  columns, structural framing, floors, walls and structural foundations.
- The marks: one code per kind, by default `К` for a column, `Б` for a beam, `П` for a
  floor, `СТ` for a wall, `Ф` for a foundation and `С` for a pile. A kind whose code is
  empty is left out and counted.
- The material suffixes: appended when the materials of the element say what the family
  is — by default `м` for concrete and `ст` for steel; both are editable, and an empty
  suffix appends nothing.
- The switch "only elements without a mark": on by default, so an existing mark is never
  overwritten unless the user asks for it.

## Behaviour
1. Walk the instances of the supported categories.
2. Decide the kind of an element:
   - a structural column or a column is a column;
   - structural framing is a beam;
   - a floor is a floor; a wall is a wall;
   - a structural foundation whose height is its largest size is a pile; any other
     foundation is a foundation;
   - anything else has no kind and is counted, never marked.
3. Decide the material family from the names of the materials of the element and its type:
   a name containing a word for concrete — «бетон», «железобетон», «concrete»,
   «reinforced» — makes it concrete; otherwise a name containing a word for steel —
   «сталь», «металл», «steel», «metal» — makes it steel; otherwise it has no family.
4. The mark is the code of the kind followed by the suffix of the material family; a
   family without a suffix appends nothing.
5. In one transaction write the mark into the mark parameter of the element. An element
   whose mark parameter is missing, read-only or not a text parameter is reported; the run
   continues with the rest. An element the switch leaves alone because it already carries
   a mark is counted.
6. Report: elements read, marked, already marked, without a kind, without a mark code,
   and refused.
7. Change nothing else: no geometry, no other parameter, nothing is created or deleted.
   Elements inside a group carry the parameter of the group, so marking one member marks
   the whole group — that is the platform's behaviour, not something the tool arranges.

## Results and messages
- A run that finds nothing to mark says so through its counts; nothing is changed.
- Success: "Read {0} element(s); marked: {1}; already marked: {2}; without a kind: {3};
  without a mark code: {4}; refused: {5}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the six mark fields, the two suffix fields, the switch, a report area and the run and
  close buttons, and closes on Escape.

## Edge cases and failures
- A category outside the supported list: never seen.
- An element without geometry: decided by its category alone, so it still gets a mark.
- A foundation whose largest size is its height: a pile; a broad foundation slab or a
  strip footing: a foundation.
- An element carrying both concrete and steel materials: concrete wins, because that is
  what the structure is made of.
- A kind whose code is empty: left out and counted, never written as an empty mark.
- Thousands of elements: one transaction, one undoable step; a run that writes nothing
  leaves no entry in the undo list.

## UI surfaces
- A tool window: the switch, the six mark codes, the two material suffixes, the hint, a
  report area and the run and close buttons. The settings are remembered between sessions.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the annotations panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Categories: `OST_StructuralColumns`, `OST_Columns`, `OST_StructuralFraming`,
  `OST_Floors`, `OST_Walls`, `OST_StructuralFoundation`, collected with
  `ElementMulticategoryFilter`.
- Materials: `Element.GetMaterialIds` on the instance and on its type, resolved to names.
- Geometry: the bounding box of the element, for telling a pile from a foundation.
- The mark: the `ALL_MODEL_MARK` parameter of the element.
- Transactions: the whole run is one undoable step.

## Scope of this release
Implemented: everything described above — the kind detection, the material family, the
configurable codes and suffixes, the never-overwrite switch, the counts and the report.

Not implemented: the preview window of the replaced tool (here the report names what was
written and what was left out); a second parameter for the product mark of the element;
assemblies marked as a whole; marking from a selection — the run takes the whole project —
and marks derived from the type name beyond the category and the shape.
