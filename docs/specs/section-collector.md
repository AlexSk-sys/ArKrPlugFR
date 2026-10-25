# Section collector — functional specification

## Purpose
Collect the concrete structures of the active 3D view, group them by level and by
category, read the section of every type from its name, and show the result as a text
report that can be pasted into a note and whose elements can be selected in the model
with one click.

## Inputs
- An open project document with a 3D view.
- The structures to collect, read from these categories: columns (structural and
  architectural), walls, floors and beams.
- Only structures whose family, type or material names mark them as concrete.
- The sections of the types are read from the type names, in the shapes the names carry:
  "400×600", "400 х 600", "D20", "200 мм".

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Take the active 3D view; when the active view is not a 3D view, take the first 3D view
   of the project. With none, report it and change nothing.
3. Collect the elements of the four categories that the view shows and keep the concrete
   ones.
4. For every element read its level, its category, its type name and the section of its
   type; a type whose name carries no section is listed without one.
5. Group the elements by level, then by category, and count them per type and section.
6. Show the report in the tool window:
   - one block per level, then per category, one line per type with its section and its
     count;
   - a button that selects every collected element in the model;
   - a button that copies the report to the clipboard.
7. Change nothing in the model: the tool only reads, selects and copies.

## Results and messages
- No active document: error, nothing changed.
- No 3D view in the project: "Open a project with a 3D view." — nothing changed.
- No concrete structures in the view: "The 3D view holds no concrete structures of the
  supported categories." — nothing changed.
- Success: the report described above, plus a line that counts the collected elements and
  their types.
- Every message is provided in Russian and English; the window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures
- A type name without a section: listed with an empty section, never guessed.
- Separators in names: "x", "х" (Cyrillic), "×" and spaces are all read as the same.
- A type name with several numbers, for example "Колонна 400х600 мм, бетон B25": the first
  pair of numbers is the section.
- An element without a level: grouped under a dash.
- Elements inside groups: collected like any other; the tool does not ungroup anything.
- Running the tool again with nothing changed produces the same report; it is safe to run
  repeatedly.

## UI surfaces
- A tool window: the report in a read-only text area, the buttons "Select all elements"
  and "Copy to clipboard", and a close button; the window closes on Escape.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the sections panel.
- All text is provided in Russian and English.

## Revit concepts involved
- The active view and the 3D views of a project; elements visible in a view.
- Categories of columns, walls, floors and beams, and the concrete names of a project.
- The level of an element, its type and its family.
- Type names as the source of a section, in millimetres.

## Scope of this release
Implemented: everything described above — the four categories, the concrete filter, the
level and category grouping, the section read from the type name, the counts, the text
report, selection in the model and copying to the clipboard.

Not implemented: the tree with click-to-select per group (selection is all-or-nothing in
this release); the placement classes of the tool this replaces (надземная, подземная,
шахта and the like), which came from project comments and are not portable; piles as a
separate group; sections computed from geometry when the name carries none; and exporting
the report to a file.
