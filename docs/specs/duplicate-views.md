# Duplicate Views — functional specification

## Purpose
The tool creates many copies of views, view templates and schedules in the current project in one action, instead of repeating the built-in "Duplicate View" command item by item. The user ticks the elements to copy in a tree that mirrors the project browser, sets a copy count for each, optionally picks one view template to assign to every created copy, and runs a single operation that creates all copies inside one transaction and then reports how many copies were made.

## Inputs
- An open project document that is the active document of the running session. The tool is meaningless in the family editor, without a document, or when only a linked model is loaded.
- No element selection and no prepared views are required; the tool examines the whole active document.
- Per-element user input, given in the tool window: which elements to copy (checkboxes) and how many copies of each (a count per element).
- Optional input: one view template chosen from a filtered, searchable list, to be assigned to all created copies. The list contains a "No template" entry, which is the default.
- A per-project settings file (JSON) stored beside the project file and named after it, holding the remembered tree sort direction and sort field. It is optional; when absent, defaults apply. It is written on sort changes and read on start.
- Every concrete number below (copy limits, counter start) is a configurable default, not a fixed constraint of the design.

## Behaviour
1. On command start, require an active project document. If there is none, report an error and open no window.
2. Open the tool window at once; build the element tree after the window is shown so the window appears immediately even on large projects.
3. Read the per-project sort settings; if the file is missing or unreadable, use the defaults (sort ascending, by element name).
4. Build three separate trees from the document: views (excluding view templates and sheets), schedules (excluding template schedules), and view templates.
5. Group views by view type into fixed groups: 3D views and walkthroughs; floor, structural and area plans; elevations, sections and detail views; drafting views; legends; other views. Sheets, schedules and panel schedules are not listed as views. A view whose name marks it as a projection view is also excluded.
6. Group schedules by schedule kind: revision schedules, key schedules, material takeoffs, sheet lists, view lists, and schedules of a named category.
7. Group view templates by the view type they apply to.
8. Within each group, order items by name using a case-insensitive comparison, ascending or descending per the remembered setting; re-apply the order from the settings on every start.
9. Every leaf row offers a checkbox, the element name, and a copy counter: a minus button, a plus button and a numeric field. Default copy count is one copy; the minimum is one copy; the maximum is 99 copies per element (default, configurable). The counter accepts digits only.
10. Ticking or clearing a group checkbox applies the same state to every descendant; a group shows an indeterminate state when only some descendants are ticked. Ticking a leaf toggles it alone, and a parent group reflects whether any descendant is ticked.
11. A leaf row toggles from a click on its text as well as on its checkbox. Shift+click sets the range of rows between the previously clicked row and the clicked row to one state. Escape clears the tree selection.
12. A tree search field filters rows by name, case-insensitively, expanding the path to each match and hiding non-matching rows; clearing the field restores the full collapsed tree.
13. The "Template settings" section becomes available when at least one view and/or schedule is ticked and no view template is ticked; otherwise it is hidden and its picker is disabled.
14. When available, the section shows a template search field and a picker listing suitable templates plus a "No template" entry.
15. Filter templates by the kinds of ticked elements: only views ticked — view templates except schedule templates; only schedules ticked — schedule templates only; both kinds ticked — all templates. A template is a schedule template when its view type is a schedule or its name contains a schedule keyword (for example schedule, key, table, list, and their Russian equivalents).
16. Filter templates by the search text: split it on spaces, commas and semicolons, keep a template when any term occurs in its name or type description, case-insensitively; order the results by view-type group, then by name.
17. When a specific template is chosen, check it against every distinct view type among the ticked elements: a schedule template fits only schedules, a legend template fits only legends, and a template of undefined type fits nothing. If it does not fit, ask the user to continue without applying a template or to cancel.
18. On run, collect every ticked element with its copy count. If none is ticked, show a "nothing selected" message and stop without opening a transaction.
19. Begin one transaction for the whole run.
20. For each ticked element, create the requested number of copies:
    1. ordinary views and drafting views — duplicate with detailing, so view-specific elements and annotations come along;
    2. legends — copy the legend element as an independent copy;
    3. view templates — copy the template element;
    4. schedules — copy the schedule element.
21. Never duplicate served or system schedules: skip titleblock/revision schedules, schedules with no definition, template schedules, schedules whose name starts with "Revision", whose name contains the stamp keyword, or whose name is blank. Never duplicate sheets or panel schedules.
22. Name each copy by appending an underscore and the copy number to the source name, numbering from 1 upward. Remove characters that element names do not allow. If the generated name is already taken, append a further counter and keep increasing it until the name is unique. Source elements are never renamed or otherwise modified.
23. Copy the source element's phase/stage value onto each copy when the target exposes a writable phase parameter that stores an element reference.
24. Apply the chosen template to each created view or legend. When no template is chosen, the copy keeps the view template it inherited from the source. A copy that is itself a view template is never assigned a template, and schedules are never assigned a view template.
25. If one copy fails, log the reason, skip that copy and continue with the remaining copies and elements; do not abort the run for a single copy.
26. Commit the transaction. If the transaction cannot be committed, no copy remains, so a failure at this level leaves the document unchanged.
27. After a successful commit, report the number of copies created.
28. Write the sort settings back to the per-project settings file whenever the sort direction changes.

## Results and messages
- Empty selection: "Nothing selected to copy. Tick at least one item in the tree." — informational, no transaction.
- Success: "Copies created: N." where N is the total requested over all ticked elements.
- Document unavailable: "The project document is unavailable. Open a project and run the tool again." — error, no window content.
- Incompatible template (per view type): a message naming the template's kind and the target view's kind, ending with "Please choose another template.", followed by a confirmation "Continue without applying the template?" with Continue and Cancel. Continue proceeds without a template; Cancel stops.
- Confirmation and result dialogs use a shared warning/information window with a title, a status line and a close button; every dialog closes on Escape.
- The main window title carries the tool caption and an English subtitle; the interface text is provided in Russian and English.

## Edge cases and failures
- No active project document, or a document closed between opening the window and running: error, nothing is copied.
- A project with no copyable views, schedules or templates: the tree is empty; running is blocked by the empty-selection message.
- Very large projects: the tree is grouped in memory in a single pass rather than by querying the browser organisation per element, and the tree is built after the window is shown.
- A generated name that already exists: the counter is increased until a free name is found; this is normal and not reported as an error.
- Served/system schedules are silently skipped, because they cannot be duplicated.
- Sheets, panel schedules and undefined view kinds are never listed and never copied.
- Template of undefined type, or a template kind that does not match the ticked element kind: the run proceeds without a template after confirmation.
- A copy that fails (element deleted or changed while the window was open, an element the document refuses to duplicate): skipped and logged; the rest of the run continues.
- Any failure that prevents the whole transaction from committing discards every copy created in that run.
- Duplicating the view that is currently open in the model may be rejected by the document; it is then skipped like any other failing copy.

## UI surfaces
- One modal tool window owned by the application.
- Header: tool caption with an English subtitle, plus help, minimise and close buttons.
- A one-line description of the tool with a pointer to the help window.
- Left panel: a section title, a tree search field, and a scrollable tree of views, schedules and view templates with per-row checkboxes and copy counters, plus a hint line about Shift+click multi-selection.
- A "Template settings" expander (right side or lower area) with a template search field and a picker; it is enabled only under the conditions in step 13 and is collapsed and disabled otherwise.
- Footer: the "Duplicate" action button, which is the only trigger for the run.
- A confirmation window (question with Continue/Cancel) and a result/warning window (title, status line, close button) are used for the messages above.
- A help window describing the tool, the step-by-step usage, and the common failure messages.

## Revit concepts involved
- View and ViewType: the element being copied and its kind (floor plan, ceiling plan, area plan, engineering plan, section, detail, elevation, three-dimensional, walkthrough, drafting view, legend, schedule, panel schedule, sheet, undefined).
- View template: a view whose "is template" flag is set; a template can be assigned to other views through the view's template reference. Templates are grouped by the view type they serve.
- Schedule and its definition: key schedules, material takeoffs, and the served/titleblock revision schedules are distinguished through the schedule definition; revision and stamp schedules are not duplicable.
- Drafting views and view-specific elements: drafting views and ordinary views are duplicated "with detailing", which carries across view-specific elements and annotations; legends and templates are copied whole.
- Phase (stage): a built-in view parameter holding a phase reference; it is carried from source to copy where writable.
- Element name uniqueness: element names are unique within the document, so generated names must be checked and made unique.
- Transaction: the whole run is one transaction, so a commit failure rolls back all copies.
- Project browser organisation: the tree mirrors the project browser's grouping of views and schedules; view-type groups are the fallback grouping when the browser organisation cannot be applied.
- Element identity: copies and source elements are addressed by element identifiers, which can become invalid if the document changes while the tool is open.

## Scope of this release
The tool ships what follows; the rest of the document describes the full intended
scope and is kept for a later release.

Implemented: the tool window opens on the active project and lists three trees — views,
schedules and view templates — each grouped by kind (3D views and walkthroughs, plans,
elevations/sections/details, drafting views, legends, other views; schedules by the
category they list; templates by the view type they serve, schedule templates in a
group of their own); a search field hides non-matching rows without losing ticks; every
row carries a tick and a copy counter from one to ninety-nine; group ticks are
tri-state, Shift+click sets a range, clicking a row's text ticks it and Escape clears
every tick; the template picker is narrowed to what fits the ticked kinds, keeps a
"No template" entry and is searched by name and group; a template that does not fit
every ticked kind is confirmed before the run goes on without it; the whole run is one
transaction with a sub-step per copy, so a copy that fails is reported and the rest of
the run continues while a failure at transaction level leaves the project untouched;
copies are named after their source with an underscore and the first free number, the
source elements are never renamed or modified, ordinary and drafting views are
duplicated with their detailing, and the stage of a view is carried over where the
platform allows it.

Not implemented yet: the per-project settings file that remembers a sort direction and
field (the trees are always ordered by name, ascending, with groups alphabetical);
descending order and any sort control; grouping that mirrors the project browser
organisation rather than the fixed view-type groups; the finer schedule kinds (key
schedules, material takeoffs, sheet lists, view lists) — schedules are grouped by the
category they list; the exclusion of views named as projection views; and the template
section that hides and disables itself when nothing that takes a template is ticked
(the section is always shown and the picker narrows itself instead).

## Open questions
- Should the fixed view-type groups (step 5) and schedule kinds (step 6) mirror the project browser organisation exactly, or is a fixed grouping acceptable as a fallback? The original intends to follow the browser but falls back to a fixed grouping.
- Are the group labels and schedule-kind labels localised in both languages, and where is that label set stored?
- What is the intended maximum copy count, if any, beyond the default of 99? The limit is currently only enforced by the counter's plus button.
- Should the schedule keyword list that decides "schedule template" (step 15) be user-editable, and which languages must it cover?
- Should view-specific elements of a drafting view that the detail copy cannot carry be reported to the user, or silently skipped?
- Is the phase/stage carry-over wanted as a fixed rule, or should the copy always inherit the source's phase by default and only differ when a template dictates otherwise?
- Should copying also place the new views on the current sheet, or is browser-only creation the intended scope?
- What should happen to the per-project settings file when the project has never been saved (no path on disk)?
- Should the tool refuse to run, warn, or proceed when the source view is the currently open view?
- Should the copy count be part of the remembered settings, or only the sort order?
- Is there an expected behaviour when a template is chosen that only fits some of the ticked elements: apply it to the fitting ones and skip the rest, or omit it for the whole run (current behaviour)?
- Should a "no copies were created" outcome be distinguished from a partial failure in the result message?
