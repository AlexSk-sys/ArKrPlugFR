# Sheet Duplicator — functional specification

## Purpose
The tool bulk-creates copies of drawing sheets inside the current Revit project in a single action, instead of repeating the built-in sheet copy command once per copy. The user ticks sheets in a tree that mirrors the project browser, sets a copy count per sheet, chooses which parts of the sheet content carry over (views, legends, schedules, text, images, annotations, parameters), and runs one operation that numbers the copies automatically and reports the outcome.

## Inputs
- An open project document that is the active document of the running session. The tool is meaningless in the family editor, without a document, or when only linked models are loaded.
- No pre-selection is required; the whole active document is examined.
- Per-sheet input: ticked / not ticked, and a copy count (default 1 copy, minimum 1 copy, digits only, an optional configurable cap of 99 copies per sheet).
- Content options: six "what to copy" switches (views; legends; raster images including embedded image documents; schedule graphics; text notes; standard annotations, lines, detailing and guide grids) plus one switch for sheet and stamp parameters. All seven are remembered between sessions.
- Optional grouping input: an ordered list of sheet parameters used to build the tree when the browser organisation cannot be applied, plus a search string that filters that list; the grouping panel collapsed/expanded state is remembered.
- Sort input: direction (ascending / descending) taken from the browser organisation when available, otherwise from a remembered value.
- One per-user JSON settings file in the user's application-data folder holds the switches, grouping list, panel state, sort direction/field and remembered tree expansion. It is optional; missing or unreadable content falls back to defaults, and write failures are ignored.
- Every number below is a configurable default, not a fixed constraint.

## Behaviour
1. On start, require an active project document. If none, report an error and open no window.
2. Clear and restart the diagnostic log, then open one modal dialog owned by the application's main window.
3. Collect every sheet of the document that is not a placeholder (sheet-list placeholder rows are excluded) and build the tree.
4. Group by the current browser organisation for sheets when it is available: one tree level per folder item, folder labels as group captions; sheets with no folder go to an "Ungrouped" group; sheets whose grouping throws go to a "Grouping errors" group.
5. Fall back to grouping by the user's ordered parameter list when the organisation is unavailable or fails: each level groups by that parameter's displayed value, blank values form their own bucket, and group captions are the values.
6. Order sheets with natural (alphanumeric) comparison of sheet numbers, so embedded numbers compare numerically (A-1, A-2, ..., A-10); root groups are always ordered naturally; sheets inside a group follow the chosen direction.
7. Re-read the browser organisation when a view is activated; if it changed, refresh the sort field, direction and tree. In the fallback path the sheet list may be served from a cache with a default lifetime of 5 seconds.
8. Selection: clicking a sheet row toggles its tick; ticking a group applies the state to all its descendants; Shift+click sets the whole range of visible rows between the last clicked row and this one to one state; Ctrl+click toggles a single row without clearing the others.
9. Copy count controls (minus button, digits-only field, plus button) appear on sheet rows only; the minus button stops at 1 copy; typing is restricted to digits.
10. On run, collect every ticked sheet with its count. If none is ticked, show a "nothing selected" message and start no transaction.
11. Process the selected sheets one after another. Each sheet gets its own transaction, named after the operation with the sheet number and copy count; warnings raised inside it are deleted, errors are resolved automatically and modal failure prompting is disabled, so Revit never blocks on a dialog.
12. Before the loop, find the stamp on the source sheet: either a title-block family instance or a generic annotation that stands in for the stamp, recognised by a keyword list applied to family, symbol and element names. Record the source viewport list once when the views option is on.
13. Compute the numbering base once per source sheet: scan sheet numbers that start with the source number, take the highest numeric suffix that follows an underscore (0 when none), and number the copies consecutively from that value upward, as an underscore plus the next suffix number appended to the source number (source A-01 gives A-01_1, A-01_2, and so on); the source sheet is never renamed.
14. Create each copy as a new empty sheet (no preset title-block type), assign the generated number, and assign a name derived from the source sheet name with a distinguishing marker so that several copies stay separable.
15. Recreate the stamp on the copy: reuse the source stamp type, place a new instance at the source stamp's insertion point (origin when it has no point location), and do nothing if an instance of that type already exists; a generic-annotation stamp is re-created through the same path.
16. When the parameters option is on, copy every parameter of the source sheet onto the copy: skip parameters whose name contains sheet-number, sheet-name or guide-grid wording, skip names missing on the target and unsupported storage types, attempt writes even on read-only targets and ignore failures; then apply the same procedure to the stamp's parameters.
17. Views option on: for each source viewport except legend and sheet-type views, duplicate the view plainly (no detailing), clear its view template, give it a unique name (source name with disallowed characters stripped, then a numeric tail increased until free), paste view-specific elements of a fixed category set (lines, dimensions, detail components, generic annotations, revision clouds and a set of tag/model categories) from the source view, and place the duplicate on the copy at the source viewport's box centre — after checking that the view may be added to that sheet, otherwise a message is shown for that view.
18. Legends option on: place viewports for the source sheet's legend views on the copy at their original centres, reusing the same legend views.
19. Schedules option on: place viewports for schedule-type views at their original centres (the schedule views themselves are reused, not duplicated), then paste schedule-sheet instances not already handled as viewports; afterwards, on each pasted instance's schedule, restore ElementId parameters named as phase/stage by matching the source phase to the copy by name.
20. Text option on: paste text notes owned by the source sheet onto the copy.
21. Images option on: paste image instances owned by the source sheet onto the copy (raster images, including embedded image documents).
22. Annotations option on: paste sheet-owned elements of a fixed annotation category set, excluding the stamp instance, its nested parts, and generic annotations recognised as stamp parts; if the source sheet references a guide grid, set the copy's guide-grid parameter to the same grid, creating a grid with that name if the project has none.
23. Annotations option off: remove guide grids from the copy — both the assigned guide-grid reference and any guide-grid elements placed on the sheet.
24. A failure while producing one copy is swallowed: that copy is abandoned and the loop continues with the remaining copies of the same sheet.
25. Commit the sheet's transaction once the loop ends; if the commit fails, every copy of that sheet disappears with the rollback.
26. If a sheet's duplication raises an error that escapes the loop, stop the run, report the sheet number and reason, and do not process the remaining sheets.
27. After all selected sheets are processed, show the success message.
28. Persist settings whenever a switch, the grouping list, the panel state or the sort direction changes; remember tree expansion so it can be restored on the next build.

## Results and messages
- Empty selection: "Select at least one sheet to duplicate." — informational, no transaction.
- Success: "Sheets duplicated successfully."
- Unavailable document: "Could not obtain the project document. Open a project and run the tool again." — error, no window content.
- Per-sheet failure: "Failed to duplicate sheet NUMBER: REASON" — error; the run stops there.
- General failure: "Failed to complete the operation: REASON" — error.
- Unplaceable view: "View NAME cannot be placed on the sheet." / "Drafting view NAME cannot be placed on the sheet." — modal, per view.
- All dialogs are modal, owned by the tool window, closed by a close button or Escape; they show a title, a status line and a close button. Window and help text are provided in Russian and English, with an English subtitle under the caption; no personal contacts appear anywhere.
- A diagnostic log is emptied at each start and appended during the run, one file per application version.

## Edge cases and failures
- No active document, a document closed after the window opened, or only links loaded: nothing is copied.
- Placeholder sheets (sheet lists) are never listed and never copied.
- Sheet or view deleted, or the document edited, while the window is open: the stored element reference no longer resolves, and that sheet is skipped or reported.
- A generated number that is still taken is rejected by the document; the copy of that iteration is abandoned and the loop continues.
- A generated view name that already exists is made unique by increasing a counter; this is normal, not an error.
- Missing stamp on the source: the copy is created without one (the new sheet is created without a title-block type).
- Read-only, absent or unsupported parameters are skipped silently; number and name parameters are never copied.
- Schedule, legend or view that cannot be added to the sheet: reported for that item, the rest of the run continues.
- Guide grids are only recreated when the platform exposes the guide-grid creation API; otherwise the reference is simply not set.
- Failure preprocessing deletes warnings and resolves errors, so platform warnings are never surfaced to the user.
- Settings file missing, corrupt or unwritable: defaults are used and the failure is ignored.
- Large projects: grouping runs in a single pass over collected sheets, the sheet list may be cached for a few seconds, and the tree is rebuilt only when grouping or sort changes.
- Characters not permitted in element names are stripped from generated names.

## UI surfaces
- One modal tool window owned by the application, with help, minimise and close buttons, a bilingual caption with English subtitle, and a one-line description pointing at the help window.
- Left card, "Sheets of the current project": a scrollable tree; sheet rows show a checkbox, minus/field/plus count controls and "number - name"; group rows show only an expander and a checkbox; a double-click on a group toggles expansion; a footer hint describes Shift+left-click multi-selection.
- Right card, "Duplication settings": a "what to duplicate" group of six checkboxes and a "parameters" group with one checkbox for sheet and stamp parameters.
- Footer of the right card: the single "Duplicate" action button that triggers the run.
- A collapsible grouping panel with two parameter lists (available / chosen), a search field, and add, remove and reorder actions, per the grouping input above.
- Result and confirmation windows (title, status line, close button) used for the messages above.
- A help window with the tool's purpose, a four-step usage walkthrough, a troubleshooting section and a responsibility/contact section with no personal names.

## Revit concepts involved
- ViewSheet: sheet element, unique sheet number, sheet name, placeholder (sheet-list) flag.
- Browser organisation for sheets: current organisation, folder items, sort field and direction; hierarchy of folder levels.
- Viewport and its box centre; View and ViewType (floor/other plans, sections, drafting views, legends, schedules, sheets); view duplication without detailing; view template reference; uniqueness of view names.
- ScheduleSheetInstance and ViewSchedule; phase (stage) ElementId parameters restored by name matching after paste.
- Element paste between views/sheets with an identity transform and paste options; view-specific elements and the fixed category sets used for content and annotation copying.
- Title blocks category, family symbol type and placement of a family instance on a sheet; generic-annotation stand-ins for a stamp recognised by keyword.
- Guide grids: the sheet's guide-grid parameter and grid elements, created by name when missing.
- TextNote, ImageInstance, Parameters and their storage types (string, double, integer, element id), read-only flags and lookup by name.
- Transaction and sub-transaction with failure handling options, a failure preprocessor that deletes warnings and resolves errors, and forced modal handling switched off.
- Application events (view activation) used to notice a changed browser organisation; external command executed in manual transaction mode.

## Scope of this release
The tool ships what follows; the rest of the document describes the full intended
scope and is kept for a later release.

Implemented: sheets of the active project that are not placeholders, listed with a
search field and ordered naturally by sheet number (A-1 before A-10); a tick and a copy
count from one to ninety-nine on every sheet row; one transaction per sheet, named with
its number and copy count, so a single sheet can be undone on its own while an error
escaping a sheet stops the run and the sheets behind it are left alone; each copy is a
new empty sheet that receives a free number (the source number when it is free,
otherwise the lowest free underscore suffix) and a name marked with its copy number;
the stamp is placed by hand at the source stamp's own point, whether it is a title block
or a generic annotation recognised by a stamp word, and the stamp parameters are copied
onto it; the views placed on the source sheet are duplicated into copies whose view
template is cleared, whose names are made unique, and which are placed on the new sheet,
and the detailing a plain duplicate leaves behind is pasted into each copy (in one batch,
falling back to element by element when the platform refuses the batch); legends are
placed again, and schedules are placed once more at their own points, with the revision
schedules left to the stamp; text notes, images, annotations and guide-grid-free content
travel with the sheet; a failure of one copy is swallowed and the next one continues.

Not implemented yet: grouping by the project browser organisation with the fallback
parameter list, and the collapsible grouping panel that goes with it (the list is flat
with a search field instead); the per-user settings file, so the switches, the grouping
list, the panel state and the sort direction are not remembered between sessions and the
run always starts from the defaults; the guide-grid parameter of the sheet; restoring
the stage of pasted schedules by name; the fixed category sets for annotations and the
category filters; the modal per-view messages for a view that cannot be placed (they are
reported in the result instead, capped so the report stays readable); and the help
window — the window carries its hints inline.

## Open questions
- Should the custom grouping panel be part of the shipped layout, and from which source should the available parameters be enumerated (shared parameters, project parameters, built-ins)?
- Is the browser organisation's sort direction authoritative, or should the user's remembered direction win over it?
- Should the sheet list always be read live instead of allowing a 5-second cache in the fallback grouping path?
- Should copies keep the source sheet name verbatim, or is a distinguishing marker in the name intended?
- On a number collision, should the tool advance to the next free suffix instead of abandoning that copy?
- What is the intended maximum copy count per sheet (default 99 copies)?
- Should schedules and legends be genuinely duplicated as new views, or is reusing the source views on the copy the intended scope?
- Which annotation categories must the fixed category sets cover, and must they be configurable?
- Which keywords identify a stamp (or a stamp part) that must be excluded, and must that list be user-editable and bilingual?
- Should the stamp be detected from the sheet's title-block type rather than by keyword matching?
- What should happen when the source sheet has no stamp: warn, or proceed silently as now?
- Should the result message report a summary (number of sheets and copies created) instead of one generic success line?
- Is a preview of the generated sheet numbers wanted in the dialog before running?
