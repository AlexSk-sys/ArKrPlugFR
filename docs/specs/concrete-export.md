# Export concrete elements for a data pipeline — functional specification

## Purpose
A data pipeline wants the concrete structures of a project as plain rows: what the element
is, where it sits, how big it is and what it is called, with the volume of its material.
Doing that by hand means reading element after element off the properties palette. This
tool walks the concrete elements of a project or of the current selection and writes one
comma-separated file a spreadsheet or a model-training script can read.

## Inputs
- The scope: the whole project, or the elements the user selected in the model.
- The file to write, chosen in a file dialog; the default name is
  `concrete-elements.csv`.

## Behaviour
1. Collect the instances of the supported categories: structural columns, columns,
   structural framing, floors, walls and structural foundations. With the selection scope,
   only the selected elements are looked at.
2. An element counts as concrete when the name of its family, its type or one of its
   materials carries the word «бетон» or «concrete», compared without case. Anything else
   is counted and left out.
3. Every concrete element becomes one row with the columns, in this order: `Id`,
   `Category`, `Family`, `Type`, `Level`, `Materials`, `Volume_m3` (cubic metres, three
   decimals), `Width_mm`, `Depth_mm`, `Height_mm` (the sizes of the bounding box) and
   `Mark` (the mark parameter). The file starts with a header line of these names; the
   names stay English whatever the interface language, because the file is read by
   programs.
4. The volume comes from the volume parameter of the element; an element without one is
   measured by the volume of its solids. A value a cell cannot carry — a refused
   parameter, unreadable geometry — makes the cell empty; the row is still written.
5. The file is written as UTF-8 with a byte-order mark, so a spreadsheet opens it
   correctly; values that carry a comma, a quote or a line break are quoted, and a quote
   inside a value is doubled.
6. Report: elements scanned, rows written, elements left out as not concrete, and refusals.
7. Change nothing in the model: the tool only reads, and it opens no transaction.

## Results and messages
- No file chosen: "Choose the file to write." — nothing written.
- No element to export: "No concrete element was found in the scope." — the file is not
  written.
- Success: "Elements scanned: {0}; rows written: {1}; not concrete: {2}; refused: {3}.
  File: {4}." — informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle,
  the scope switch, the file button and its line, a report area and the export and close
  buttons, and closes on Escape.

## Edge cases and failures
- A file the system refuses to write (a locked path, no rights): reported, the run stops
  with a message that names the file.
- An element without a level, a family or a mark: the cell stays empty.
- A material name with a comma: the cell is quoted.
- A project with no concrete at all: counted, and the file is not created.
- The selection scope with an empty selection: reported as nothing to export.

## UI surfaces
- A tool window: the switch that chooses between the project and the selection, the button
  that chooses the file with the line that shows the chosen path, the hint, a report area
  and the export and close buttons. The scope and the last file name are remembered.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in
  the data panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Categories: the same six structure categories the marking uses.
- Materials: the material ids of the instance and of its type, resolved to names.
- Volume: the volume parameter (`HOST_VOLUME_COMPUTED`) in cubic feet, converted to cubic
  metres; the solids of the element as the fallback.
- No transaction: the export only reads.

## Scope of this release
Implemented: everything described above — the two scopes, the concrete test by names, the
rows and their columns, the volume and the box sizes, the quoting and the byte-order mark,
the counts and the report.

Not implemented: the Excel template of a cost estimate that the replaced tool filled (the
file here is a plain comma-separated table); the formwork and area columns; the ML
prediction itself — training a model belongs to the pipeline the file feeds, not to the
add-in; and reading the elements of linked models.
