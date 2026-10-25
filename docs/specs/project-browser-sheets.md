# Sheet tree from the project browser — functional specification

## Purpose
The project browser shows the sheets of a project under the folders of the active sheet
organization scheme, but the groups have to be expanded one by one and the path of a
sheet is not something a user can copy. This tool reads the same scheme, shows the whole
tree at once, and copies the path of any row as `Group → Subgroup → Sheet`. It only reads
the project.

## Inputs
- An open project with sheets and a sheet organization scheme.
- No picks and no options: the window shows the tree of the active scheme.

## Behaviour
1. Read the active sheet organization of the project. Without one, the sheets are shown
   in a single list and the window says so.
2. Read every sheet of the project and ask the scheme for the folders the sheet sits in,
   top level first. A sheet the scheme excludes with its filters is counted and left out,
   exactly as the browser leaves it out.
3. A sheet whose folders cannot be read is counted as unreadable and shown in its own
   group, so a broken scheme never hides a sheet.
4. Build the tree: the folders as they come, groups ordered by name, the sheets of a group
   ordered by the sorting parameter of the scheme — its number when the scheme has none.
   Every group counts the sheets below it, and a sheet is shown as `Number — Name`.
5. Copy: the selected row — and any row through its context menu — is copied to the
   clipboard as the path `Group → Subgroup → Sheet`, or as the label alone for a sheet at
   the root of the tree.
6. Change nothing: the tool opens no transaction and writes nothing.

## Results and messages
- The summary line: "Sheets: {0}; groups: {1}; left out by the scheme: {2}; unreadable:
  {3}."
- No scheme: "The project has no sheet organization scheme; the sheets are shown in one
  list."
- No sheets: "The project holds no sheets."
- Copy failures — a clipboard another application holds — are reported in the window.
- Every message is provided in Russian and English; the window carries a title, a
  subtitle, the tree, a copy button, a report area and a close button, and closes on
  Escape.

## Edge cases and failures
- No sheet organization scheme: one flat list, no groups.
- A sheet the scheme's filters exclude: counted, never shown.
- A sheet whose folders cannot be read: counted as unreadable and shown under its own
  group.
- A sheet at the root of the scheme: shown without a group.
- Two sheets with the same sorting value: ordered by sheet number.
- The clipboard being held by another application: reported, nothing else changes.

## UI surfaces
- A tool window: the summary line, the tree with expandable groups, the copy button for
  the selected row, a context menu with the same action on every row, a report area and
  the close button.
- The ribbon button carries the tool caption and a bilingual description; the tool sits
  in the sheets panel.
- All text is provided in Russian and English.

## Revit concepts involved
- The sheet organization scheme: `BrowserOrganization.GetCurrentBrowserOrganizationForSheets`,
  its filters (`AreFiltersSatisfied`), its folders per element (`GetFolderItems`), and its
  sorting parameter and order (`SortingParameterId`, `SortingOrder`).
- Sheets: the `ViewSheet` elements of the project, their number and name.
- The clipboard: `Clipboard.SetText`; the tool opens no transaction.

## Scope of this release
Implemented: everything described above — the tree of the active scheme, the counts, the
sorting of the scheme, the copy of a path, and the grouping rules covered by unit tests
without Revit.

Not implemented: editing the scheme or the sheets from the window; reading the view or
schedule schemes (the tool reads the sheet scheme only); and a search field — the browser
tree of a project is small enough to scroll.
