# Sheet Copy — functional specification

## Purpose
Bulk-copy drawing sheets between open Revit documents in one run. The user picks source sheets in a browser-like tree, ticks one or more target documents, chooses which content carries over (stamp, schedules, legends, drafting views, text, annotations, guide grids, images, parameters), and gets numbered copies with an automatic renumbering rule and a summary report. Typical uses: move a documentation subset into an adjacent project section, or assemble a new package from an existing set.

## Inputs
- Source: the active document when the command starts. At least one other open document must exist to serve as a target.
- Target documents: ticked from a list of open documents; linked models and the source itself are excluded from that list.
- Per sheet: ticked state and a copy count — default 1 copy, minimum 1, digits only, configurable cap default 99 copies per sheet.
- Content switches, remembered between sessions: stamp (title block) and its parameters; schedules placed on the sheet; legends; drafting views with contents; text notes; annotations and detail components; guide grids; images; sheet parameters; viewports (default on).
- Optional category filters for annotations, detail lines, text and images (default on), restricting which categories are pasted.
- Collision preference: reuse existing element or create a copy, plus an "apply to all remaining" flag valid for the current run only.
- Tree grouping: the current browser organisation for sheets, otherwise a remembered ordered list of sheet parameters; sort direction comes from the organisation or a remembered value.
- One per-user JSON settings file in the application-data folder holds switches, filters, grouping list, sort direction and tree expansion state; missing or corrupt content falls back to defaults, write failures are ignored.
- Every number above is a configurable default with units (copy counts in sheets, cache lifetime in seconds).

## Behaviour
1. On start, require an active document; if none, report an error and open no window.
2. Clear and restart the diagnostic log, then open one modal dialog owned by the application's main window.
3. Collect every sheet of the source that is not a sheet-list placeholder and build the tree grouped by the current browser organisation for sheets; when it is unavailable or fails, group by the remembered parameter list; sheets with no folder go to an "Ungrouped" group and grouping failures to their own group.
4. Order sheets with natural (alphanumeric) comparison of sheet numbers (A-1 < A-10); group order and direction follow the organisation, re-read whenever the active view changes, or the remembered direction.
5. Selection: clicking a sheet row toggles its tick; ticking a group applies the state to all descendants (parent shows a mixed state); Shift+click sets the range of visible rows; Ctrl+click toggles one row.
6. Copy-count controls (minus, digits-only field, plus) sit on sheet rows; minus stops at 1 copy; the field rejects non-digits and values above the cap (default 99 copies).
7. On run, collect ticked sheets with counts; if none, show a "nothing selected" message and start no transaction; if no target is ticked, show the "no target" message and start no transaction.
8. Validate every ticked target: it must not be the source document, a linked file, a family document or read-only; an invalid target is reported with a description (title, path or "unsaved", and the failing flag) and receives nothing.
9. Process targets in list order and sheets in tree order; each sheet copy runs in its own transaction named after the operation with sheet number and copy count; failure handling suppresses modal prompts so Revit never blocks on a dialog.
10. Locate the stamp on the source sheet: a title-block family instance, or a generic annotation standing in for the stamp, recognised by a configurable bilingual keyword list applied to family and type names (generic words for "title block" and "stamp" in both languages, no organisation-specific tokens); otherwise fall back to the first family instance of the title-block category.
11. Create a new empty sheet in the target (pre-set with the resolved title-block type when one was found), then assign the number: reuse the source number if it is free, otherwise append the lowest free underscore suffix starting at 1 (source A-01 gives A-01_1, A-01_2, ...); the suffix pattern is a configurable default.
12. Assign the copy the source sheet name with a configurable distinguishing marker so several copies remain separable.
13. Stamp option on, generic-annotation stamp: find the same family and type in the target by family and type name (case-insensitive), otherwise load the family from the source document; if neither works, create the sheet without a stamp and log the event.
14. Stamp option on, title-block instance: recreate it on the copy at the source placement point (document origin when the instance has no point location), reusing the source type.
15. Stamp parameters option on: copy writable parameter values matched by name onto the recreated stamp; skip read-only, absent and unsupported-storage-type values; never copy sheet-number, sheet-name or guide-grid parameters.
16. Sheet parameters option on: the same name-matched, writable-only procedure applied to the sheet element itself.
17. Viewports option on: recreate each source viewport on the copy at its original box centre, skipping view types that may not be placed on sheets; each skip is reported for that view.
18. Drafting views option on: copy each drafting view across with full contents under a unique name (source name plus a configurable "copy" marker, numeric tail increased until free), then place its viewport.
19. Legends option on: for each legend viewport, if a legend of the same name exists in the target, ask whether to reuse it or create a copy, with an "apply to all remaining" checkbox; the answer is cached per legend name for the run; reuse places the existing target legend, a copy duplicates the legend under a unique name and places that.
20. Schedules option on: for each schedule sheet instance, find the schedule view in the target by name, or copy the schedule view across when missing, place the instance at the source position, then restore phase/stage ElementId parameters (names containing "phase" or "stage" in either language) by matching the phase name in the target.
21. Text, annotations, detail lines and images options on: paste the corresponding source-sheet-owned elements onto the copy with an identity transform; the category filters (default on) restrict the fixed category set; the stamp instance and its nested parts are excluded from the annotation set.
22. Revision clouds inside the annotation set are recreated on the copy and remapped to the target revision with the same sequence number; when none matches, that cloud is skipped.
23. Guide grids option on: set the copy's guide-grid reference, creating a grid of the same name in the target when absent; option off: no guide-grid reference is carried over.
24. Cross-document copies resolve duplicate type names with the configured default action (keep the target's existing type), so the run never pauses on a duplicate-type prompt.
25. A failure while producing one copy abandons only that copy; the loop continues with the remaining copies of the same sheet and with the next sheets.
26. Commit a sheet's transaction after all of its copies are made; if the commit fails, every copy of that sheet disappears with the rollback.
27. After the run, show a summary per target: copies created out of copies planned, sheets processed, and the source and target document titles.
28. Persist settings whenever a switch, filter, grouping list or sort direction changes; remember tree expansion so it is restored on the next build.

## Results and messages
- Unavailable document: "Could not obtain the project document. Open a project and run the tool again." — error, no window content.
- Nothing ticked: "Select at least one sheet to copy." plus the log file location — error, no transaction.
- No target: "No target documents selected. Tick open documents in the list." — error, no transaction.
- Invalid target: "Document 'TITLE' cannot be a target (source / linked / family / read-only)." — error, that target is skipped.
- Success: "Successfully created N of M planned copies. Sheets processed: K. Source: S. Target: T." — one summary per target.
- Per-view placement failure: "View NAME cannot be placed on the sheet." — modal, per view, the run continues.
- General failure: "Failed to complete the operation: REASON." — error.
- All dialogs are modal, owned by the tool window, closed by a close button or Escape; they show a title, a status line and a close button. Window and help text are provided in Russian and English with an English subtitle under the caption; no personal contacts appear anywhere.
- A diagnostic log is emptied at each start and appended during the run, one file per application version.

## Edge cases and failures
- Only one open document, or the source among the candidates: no valid target, the run is blocked by the "no target" message.
- A document closed or edited while the window is open: the stored reference no longer resolves; that sheet or target is skipped and reported.
- A generated sheet number rejected by the target (for example taken after a concurrent edit): that copy is abandoned and the loop continues.
- Stamp family missing in the target and not loadable: the sheet is created without a stamp; logged, not reported as a failure.
- Source sheet without a stamp: the copy is created without one, silently.
- Same-named legend already in the target: the prompt appears once per legend name; "apply to all" short-circuits every remaining prompt for the run.
- Schedule, legend or view that cannot be added to the sheet: reported per item, the rest of the run continues.
- Phase name not found in the target: the parameter is left unchanged; revision sequence number missing: the cloud is skipped.
- Read-only, absent or unsupported parameters are skipped silently; number, name and guide-grid parameters are never copied.
- Settings file missing, corrupt or unwritable: defaults are used and the failure is ignored.
- Placeholder sheets (sheet lists) are never listed and never copied.
- Large projects: grouping runs in one pass over the collected sheets, the tree is rebuilt only when grouping or sort changes, and the fallback grouping path may serve the sheet list from a cache with a default lifetime of 5 seconds.
- Characters not permitted in element names are stripped from generated view and sheet names.

## UI surfaces
- One modal tool window owned by the application, with help and close buttons, a bilingual caption with English subtitle, and a one-line description pointing at the help window.
- Left card, "Sheets of the source project": a scrollable tree; sheet rows show a checkbox, minus/field/plus count controls and "number - name"; group rows show an expander and a tri-state checkbox; a footer hint describes the Shift/Ctrl selection gestures.
- Right card, top: the target-documents list with one checkbox per open document (source and links excluded).
- Right card, "What to copy": groups of checkboxes for stamp, stamp parameters, schedules, legends, drafting views, text, annotations, detail lines, guide grids, images, sheet parameters and viewports; Shift+click on a checkbox sets the whole range of visible option checkboxes to one state.
- Footer of the right card: the single "Copy" action button that raises the run.
- Decision dialog: the collision question, "reuse existing" and "create copy" buttons, and an "apply to all remaining" checkbox.
- Result dialog: title, status icon and line, detail text, close button, Escape closes it.
- Help window: purpose, a four-step usage walkthrough, a troubleshooting section and a responsibility section with no personal names.

## Revit concepts involved
- ViewSheet: unique sheet number, sheet name, placeholder (sheet-list) flag.
- Browser organisation for sheets: current organisation, folder levels, sort field and direction; organisation re-read on view activation.
- Viewport and its box centre; placement rules per view type; uniqueness of view names.
- ViewDrafting and ViewLegend duplication versus reuse; view contents pasted per view.
- Title blocks family category versus generic-annotation stand-in recognised by keyword; loading a family into another document.
- Cross-document element copy with an identity transform and copy-paste options; duplicate type name handler.
- ScheduleSheetInstance placement; ViewSchedule lookup by name; phase (stage) ElementId parameters restored by name matching.
- Revision and RevisionCloud; sequence-number matching between documents.
- TextNote, images, detail lines and a fixed set of annotation/tag categories with category filters.
- Parameters: name matching, storage types (string, double, integer, element id), read-only flags.
- Guide grids: the sheet's guide-grid parameter and document-level grid creation by name.
- Transaction per sheet with failure handling that suppresses modal dialogs; external event used to run the copy from a modal window.
- Per-user settings storage and per-version diagnostic logging.

## Scope of this release
The tool ships what follows; the rest of the document describes the full intended
scope and is kept for a later release.

Implemented: the active project is the source and is never written to; the targets are
ticked from the projects open in the same session, with family documents, linked
models and read-only files listed but refused next to their reason; every sheet row
carries a copy count from one to ninety-nine; sheets are ordered by natural
sheet-number comparison; each target receives its own numbers, reusing the source
number when it is free and otherwise the lowest free underscore suffix, with a marked
name per copy; one transaction per target project and one sub-transaction per sheet
inside it, so a sheet that fails takes only itself down; the result reports one
summary line per project with its failures, capped so the box stays readable.

Not implemented yet: the content switches (stamp and its parameters, schedules,
legends, drafting views, text, annotations, detail lines, guide grids, images, sheet
parameters, viewports) — a ticked sheet travels whole, exactly as the platform copies
it, because a sheet cannot be taken apart across documents without that machinery;
the category filters; the collision preference between reusing an element and copying
it; tree grouping by the browser organisation with its fallback parameter list; the
per-user settings file; the Shift/Ctrl range selection in the sheet list; and the help
window (the window carries the hints inline instead).

## Open questions
- Should the target picker be a single-choice dropdown (as first described) or a multi-select list, and may one run write into several targets at once?
- Which content switches are on by default on the very first run?
- Are the four category filters user-visible options or fixed internal constraints?
- What is the intended copy-count cap per sheet (default 99 copies)?
- Which distinguishing marker should the copy's sheet name carry, and where does it sit relative to the source name?
- Should drafting-view name collisions prompt reuse-versus-copy like legends, or always create a uniquely named copy?
- Which fields identify a matching revision: sequence number only, or date and description as well?
- Should schedules be reused by name, copied across only when missing, or always duplicated as new views?
- Which keywords identify a stamp, and must that list be user-editable and bilingual?
- Should the run be previewed (generated sheet numbers listed) before any transaction starts?
- Should the whole run be a single undo step, or one undo step per sheet?
- Should a partially failed target still receive the success summary with a failure count added?
