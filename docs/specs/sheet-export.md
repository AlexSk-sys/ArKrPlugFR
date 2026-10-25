# Sheets to PDF — functional specification

## Purpose

Hand a set of sheets over as files: one PDF per sheet, named after the sheet, in a folder the
user chooses. Printing a drawing set by hand — opening every sheet, picking the printer,
naming the file — is the slowest part of issuing a set; the tool does it in one action and
reports what it produced.

## Inputs

- An open project document.
- The sheets to export, ticked in a list of every sheet of the project; all of them can be
  ticked at once.
- The folder the files are written into.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. List the sheets of the project: number and name, ordered by number.
3. When the run is asked for:
   - at least one sheet must be ticked;
   - the folder must be given; a folder that does not exist is reported and nothing is
     written;
   - the file name of every ticked sheet is worked out as its number, a separator and its
     name, with the characters a file name cannot hold replaced; a sheet without a number or
     a name still produces a name.
4. Export the ticked sheets with the platform's PDF export: one file per sheet, named after
   the sheet, into the folder. The export is asked not to stop on the first failing sheet, so
   one bad sheet does not take the rest of the set down with it.
5. Report: the folder, how many sheets were ticked, and the file name of every sheet. When
   two sheets would produce the same file name, the report says so before the export runs,
   because one file would overwrite the other.
6. Change nothing in the model: exporting writes files outside the project and leaves the
   document untouched; no transaction is opened.

## Results and messages

- No active document: error, nothing changed.
- No sheet ticked: the run reports it and writes nothing.
- No folder, or a folder that does not exist: the run reports it and writes nothing.
- Two sheets with the same file name: reported with both sheet numbers; the run continues,
  because the user may want the export anyway.
- Success: "Exported {0} sheet(s) into «{1}»." followed by one line per sheet with its file
  name.
- A platform refusal: reported with the reason.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A sheet whose name holds characters a file name cannot hold: they are replaced, and the
  report shows the name that was used.
- A sheet without a name: the file is named after its number alone.
- A sheet the platform refuses to export: reported with the reason; the other sheets are
  still exported.
- A folder that cannot be written into: reported with the reason.

## UI surfaces

- A tool window: the sheet list with a tick per row, a button to tick and untick all of them,
  the folder field with a browse button, a run button, a report area and a close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the sheets panel.
- All text is provided in Russian and English.

## Revit concepts involved

- `ViewSheet` elements and their number and name.
- `Document.Export` with `PDFExportOptions`: one file per sheet (`Combine` off) and a naming
  rule built from the sheet number and the sheet name.
- The PDF export of the platform exists from Revit 2022; Revit 2020 has no API for it, so the
  tool is offered on Revit 2022 and 2024 only, and the release difference lives in the
  compatibility layer.

## Scope of this release

Implemented: everything described above — the sheet list, the tick all, the folder, one PDF
per sheet with the naming rule, the duplicate warning, the report, the bilingual window.

Not implemented: the reference's printer path (a print driver is a property of the machine,
not of the project), its DWG export, its stamp handling and its schedule refresh; sheets of
linked models; combining the set into one file; any change to the model.
