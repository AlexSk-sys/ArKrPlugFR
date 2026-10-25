# View Template Transfer — functional specification

## Purpose
Copy view templates from the Revit project that is currently active to another project
that is open in the same Revit session, so configured visibility/representation settings
can be reused across projects without manual recreation. The tool is part of a bilingual
(Russian/English) portfolio add-in for Autodesk Revit 2020/2022/2024.

## Inputs
- Active document at the moment the command is launched — the source of templates.
- A target document chosen by the user from all other open, non-linked documents.
- A set of view templates selected in a tree list.
- An "overwrite existing" option flag (default: on) — see Open questions.
- The UI language (Russian or English) for all labels and messages.

## Behaviour
1. The command is exposed on the add-in's custom ribbon tab, in a dedicated panel,
   as a single button; it requires an active document, otherwise it reports an error.
2. On launch the tool collects every view template in the source document
   (all View elements flagged as templates), including templates for schedules.
3. Collected templates are grouped by view type into human-readable categories.
   Default grouping (configurable mapping, several view types may share one category):
   floor/structural/area plans in one group; ceiling plans; 3D views and walkthroughs;
   elevations, sections, legends and detail views together; drafting views;
   engineering plans; schedules. A view type without a mapping gets its own group
   named after the raw view type.
4. Groups are listed sorted alphabetically by group name; templates inside a group are
   sorted alphabetically by template name. Groups start collapsed.
5. Each node carries a tri-state checkbox: checking a group checks all of its templates;
   a template change recomputes the parent state to checked (all children checked),
   unchecked (none checked) or indeterminate (partial).
6. Shift+click on a template applies the state of the previously clicked template to
   every leaf between the two clicks in visible flattened order; group rows are skipped
   in a range operation. A plain click toggles only the clicked node.
7. The target list shows the title of every open document except the source document
   and any linked documents; nothing is preselected.
8. On Transfer the tool validates: a target must be selected and at least one template
   must be checked; otherwise the transfer is aborted with a specific message.
9. Valid selections are copied from the source document into the target document inside
   one transaction in the target, named "Transfer View Templates" (localized).
   Copy/paste uses default options.
10. Per the original contract, existing same-name templates in the target are never
    overwritten; copies arrive as new entries. All selected templates are copied in
    one operation — all-or-nothing, no partial success.
11. Each session writes a diagnostic log keyed to the running Revit version; it records
    launch, loaded template/group/target counts, transfer start (count, target title,
    overwrite flag), completion, and any error message.
12. Only one tool window may be open at a time; re-invoking the command replaces it.

## Results and messages
- Success dialog: number of templates transferred.
- "Select a target project." — no target chosen.
- "No templates selected." — nothing checked.
- Failure dialog: the underlying error message, verbatim, when the copy fails.
- Every outcome is also written to the session log.
- All dialogs are styled to match the tool, not the native Revit task dialog.

## Edge cases and failures
- No active document at launch: command reports an error and does not open the window.
- No other open project: the target list is empty and Transfer stays blocked by validation.
- Source with zero templates: the tree is empty; transfer is blocked by validation.
- Linked documents never appear as targets (read-only in this context).
- Copy failure (e.g. missing family/type dependencies, worksharing or permission issues,
  transaction refusal): the transaction is rolled back, nothing is transferred, and the
  error dialog shows the reason.
- Templates whose view type has no mapping still appear, grouped under the raw type name.
- Closing the target document while the window is open: transfer must fail gracefully
  with an error message, not crash.
- Window is modal and owned by the Revit main window; the Revit UI is blocked while open.

## UI surfaces
- Ribbon: one tab of the add-in, one panel, one command button.
- Main window (modal, resizable, dark themed, borderless with custom caption):
  default size 1100×900 px, minimum 860×620 px (configurable defaults), centered;
  caption area with title, Help, Minimize and Close; Escape closes the window.
- Left panel: template tree (group → template) with tri-state checkboxes and a hint
  about Shift multi-selection.
- Right panel: target-project combo box, options section with the overwrite checkbox,
  primary Transfer button (icon + label).
- Help window: short usage instructions.
- Result dialog: success/failure state, message, close action.
- All text is available in Russian and English.

## Revit concepts involved
View templates (views with the template flag); ViewType enumeration and its mapping to
display groups; Document and UIApplication document lists; linked-document detection;
cross-document element copy (ElementTransformUtils with a set of element ids);
CopyPasteOptions; Transaction in the destination document; ExternalCommand attribute
and execution result; ribbon tab/panel registration; WPF window owned via the Revit
main window handle; per-version session logging.

## Scope of this release
Implemented: the active project is the source and is never written to; the target is
picked from the projects open beside it, with the source itself, linked models and
family documents kept out of the list; templates are collected and grouped by view type
into the named categories, a type without a mapping falling back to its own name, with
groups sorted alphabetically, collapsed by default, tri-state group checkboxes and
Shift+click ranges; the copy runs as one transaction inside the target and is
all-or-nothing — anything that fails rolls the whole step back and the reason is shown;
a project closed while the window is open is reported instead of crashing the run; the
tree shows the names the source project uses, and the platform names the copies inside
the target.

Not implemented yet: the "overwrite existing" checkbox — behaviour step 10 of this
document never overwrites a same-name view, so the window says so in words rather than
offering a switch that could only be ignored; the icon on the Transfer button; and the
log line about an overwrite flag, which does not exist.

## Open questions
- What exactly should the "overwrite existing" checkbox do, given the contract that
  same-name templates are never overwritten? Options: hide the flag, use it to enable
  true overwrite, or use it to control auto-renaming of copies.
- When a same-name template exists in the target, should the copy be skipped, failed,
  or auto-sufixed (e.g. "Name (2)")? Which behaviour does Revit's copy engine produce
  by default per version?
- Should the language follow the Revit UI language automatically or be a manual switch?
- Should templates applied to views in the source be re-applied to corresponding views
  in the target, or are bare template copies sufficient?
- Should the group mapping table be user-configurable, and where is it persisted?
- Is there a practical limit on templates per transfer requiring progress indication?
