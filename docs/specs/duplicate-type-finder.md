# Duplicate Type Finder — functional specification

## Purpose
The tool scans every family loaded in the open project and reports two kinds of redundancy: family types inside one family that are identical in all comparable parameter values, and whole families whose complete set of types duplicates that of another family. For each finding it states which item is kept, which item is proposed for removal, and how many placed instances depend on the item being removed. The findings are reviewed in one window from which the surplus types and families can be deleted in a single confirmed action.

## Inputs
- An open project document that is the active document in the running session; the tool is meaningless in a family editor, without a document, or when only a linked model is loaded.
- No element selection and no prepared views are required. The tool always examines the whole active project, never a selection.
- No user configuration is required to run. The defaults that drive comparison are described under Behaviour; every concrete value stated here is a configurable default, not a fixed constraint of the design.
- Families and their types must already be loaded in the project; the tool does not read external family files.
- The user should save or back up the project first, because deletion is permanent.

## Behaviour
1. On start, require an active project document. If there is none, report an error and open nothing.
2. Clear the current element selection in the project, so a previous selection can never scope the run.
3. Begin a fresh analysis log for this run.
4. Collect all families of the project in one pass, skipping entries that are not valid loadable families. If the project holds no families, report an error.
5. Count placed instances per family type in a single pass over all family instances in the project. The unit is whole items (an integer count); each instance is counted once, under its own type.
6. For every family, enumerate its family types; skip a family type whose data cannot be read.
7. Record for each family type: its family, its family category, its type name, and its placed-instance count (zero or more).
8. Build a comparison record for each family type from all of its type parameters, after two exclusions and one normalisation pass.
9. Excluded by default are the identifying and service parameters — the type name and any parameter that merely identifies the type — and the free-text metadata parameters type mark, comments and description. Two further defaults are excluded: the type image and a cost value. These are excluded because a copied type normally has the identifying fields filled on one side and empty or different on the other, yet such types are still duplicates. The excluded set is a default and is intended to be configurable; the original description does not settle how.
10. Include only parameters that carry a value and are not read-only or computed. A parameter that cannot be read contributes an empty value instead of failing the whole comparison.
11. Normalise every value before comparing: trim it; collapse any run of spaces or tabs to a single space; treat empty and whitespace-only values as empty; compare numeric values as numbers so that formatting and insignificant trailing zeros do not create a difference (default precision: 15 significant digits). Non-numeric values are compared as text, case-insensitively.
12. Within one family, a type is a duplicate when its comparison record equals the record of at least one other type of the same family. Types are compared only inside their own family; identical types in two different families are not "duplicate types".
13. When two or more types of a family match, keep one and mark all the others. The kept type is the match whose type name comes first alphabetically (case-insensitive); every other matching type becomes one result row.
14. Detect duplicate families in two passes. Pass 1, full copies: families match when the set of comparison records of one family equals the set of records of the other. Within such a group, keep the family with the most placed instances (tie-break: family name earlier alphabetically); every other family in the group becomes a result row.
15. Pass 2, contained copies: among families not already used in pass 1, if the record set of family A is contained in the record set of family B, they form a pair. The family with the larger set is kept; the family with the smaller contained set is the duplicate. Equal sets were already handled in pass 1.
16. A family appears as a duplicate candidate in at most one result row; once a family is used as either side of a pair it is not reused.
17. For a duplicate family, record the duplicate family and its kept counterpart, the category of the duplicate, the number of types in the duplicate family (N), the number of types in the kept family (M), and the total placed-instance count of all types of the duplicate family.
18. Present results ordered by family name, then by type name, both case-insensitively, in two tables (see UI surfaces).
19. Exclude from selection every row whose instance count is greater than zero. Such a row is not deletable: its checkbox is disabled, "select all" does not tick it, and a range selection does not tick it. An instance count of zero means the item can be removed.
20. On delete, take only the currently ticked, deletable rows. If nothing ticked is deletable but some ticked rows were in use, explain that; if nothing is ticked at all, say so. Neither case opens a transaction.
21. Show a confirmation dialog naming how many types and how many families will be deleted and stating that deletion cannot be undone. If the user cancels, change nothing.
22. Perform deletion in one transaction. Delete each selected type and family item by item; if an item fails (for example because it is still in use), record the reason, skip it, and continue with the rest. Commit if at least one item was deleted; otherwise roll the transaction back.
23. After deletion, remove the successfully deleted rows from their table and update every count: the tab headers, the summary bar and the delete-button label.
24. Report the outcome: how many types and how many families were removed, plus the items that failed and why (default: show up to 10 failed items).
25. On request, copy the current table to the clipboard as tab-separated text with a header row, so it can be pasted into a spreadsheet.
26. On double-click of a row, select and zoom to one placed instance of that type, or of any type of that family, in the project. Hide the tool window during the operation and restore it afterwards. If no instance exists, report that the item is unused.
27. If no duplicates at all are found, show an informational message with how many types were checked and the note that all parameters except the identification and free-text metadata parameters were compared. As a diagnostic, also list pairs of types inside one family that differ in a small number of parameters (default range: 1 to 3 differing parameters), up to a default of 20 pairs and examining up to 400 type pairs per family, and show up to 6 of them in the message.
28. While collecting and comparing, show a progress window with a status line; process families in batches (default: 12 families per batch) so the window stays responsive. All work runs on the application's main thread.
29. Append a run summary to the analysis log: families examined, types collected, duplicate types found, duplicate families found, items deleted, error count.

## Results and messages
- Summary bar: "duplicate types: <total> (removable: <n>) · duplicate families: <total> (removable: <n>)".
- Tab headers carry live counts: "Type duplicates (N)" and "Family duplicates (N)".
- Delete button label: "Delete selected (N)", where N is the number of ticked, removable rows across both tabs; with nothing ticked it reads "Delete selected".
- An empty-state text is shown inside a table that has no rows: "No duplicate types found" / "No duplicate families found".
- Error dialogs: no active document; no families in the project; a selected item could not be found in the project; a double-clicked item has no instances.
- Confirmation dialog: "Will be deleted: types — X, families — Y." (only the relevant clause when one kind is absent), plus "Items with instances in the project will not be deleted. The operation is irreversible."
- Post-deletion result: "Deleted: types — X, families — Y." or "Nothing was deleted.", followed by "Could not delete (n): …" listing up to 10 items with their reasons (an in-use failure is reported as "used in project" rather than a raw error).
- Nothing-to-delete message distinguishes "no rows ticked" from "the ticked rows all have instances in the project", and reminds the user to tick rows whose in-project count is zero.
- No-duplicates message as in step 27, including the checked-type count and the near-duplicate pairs.
- Double-click result: "Selected the instance of type: <family> : <type>" or the equivalent for a family.
- Copy result: "Copied rows: N." or an error if the clipboard is unavailable.
- Every dialog closes on Escape; the main window also closes on Escape.

## Edge cases and failures
- No active document, or no document at all: error, no analysis, no window.
- Project with no families: error.
- Nothing found: an informational message is shown instead of the results window.
- A family with no readable types, or a type with a missing name, is treated as an unnamed item and must never crash the run.
- A parameter or value read failure contributes an empty value; the comparison continues.
- A row with instances greater than zero can never be ticked, is skipped even if it were somehow selected, and is reported.
- Deletion failure for one item is skipped and reported; the remaining items are still deleted and the transaction still commits.
- If the document changes or closes between analysis and deletion, the item is no longer found; report and continue.
- When identical types exist, exactly one is kept and the choice is name-based, so it is deterministic but may not match the user's expectation.
- Non-loadable families and families with no types do not participate at all.
- The tool must never delete an item that has placed instances.
- Only the active project is examined; families that exist solely in linked models are out of scope (see Open questions).

## UI surfaces
- One non-blocking tool window, opened as a separate window owned by the application's main window.
- Header: the tool caption plus an English subtitle, a help button, and minimise, maximise and close controls.
- Top line: a one-sentence description of the tool and a pointer to the help window.
- Summary bar: the counts line and a scope badge reading that all families of the project were examined.
- Two tabs, "Type duplicates (N)" and "Family duplicates (N)", with an underline-style tab strip.
- Type-duplicates table columns, in order: delete checkbox; family; category; type (duplicate); source type; in-project count shown as a small capsule. All non-checkbox columns are read-only.
- Family-duplicates table columns, in order: delete checkbox; family (duplicate); category; source family; types matching shown as "N of M"; in-project count capsule.
- Above each table: "Select all", "Clear selection" and "Copy list" buttons, acting on the active tab only.
- Checkboxes are disabled on rows whose in-project count is greater than zero, with a tooltip explaining why; the capsule is tinted to signal "in use" and otherwise hints "no instances — can be deleted".
- Row interaction: a single click anywhere on a row toggles that row's checkbox; Shift+click replaces the ticks with the range between the previously clicked row and the clicked row, skipping undelectable rows. The table's own highlight follows the checkboxes, so visual selection and tick state stay synchronised.
- Ordering is the tool's own (family, then type name). Header-click sorting is not part of the original and the rows are not editable; the grid virtualises rows for large result sets.
- Double-click on a row selects and zooms to the instance in the project.
- Footer: a hint line ("only rows with capsule 0 can be deleted · double-click selects in the project · Shift selects a range"), the delete button and a close button.
- Help window: what the tool does, how to use it step by step, and the common "why can't I delete this" cases.

## Revit concepts involved
- Family: a loadable component definition; only families loaded in the active project are considered.
- Family type (a.k.a. type): a named variant inside a family that carries the type parameters compared here; the tool works at the type level.
- Family instance (placed instance): one placement of a family type in the model; its count is what makes a type or family undeletable.
- Category: the classification of a family (for example doors or windows); it is reported per row and is not a comparison key.
- Type parameters: the parameters visible and editable on a family type. A value may be text, a number, a yes/no state, or a reference to another element. Read-only and computed parameters are not compared.
- Parameter semantics: the comparison key is the parameter name and the compared value is the normalised value; yes/no values compare as their two states and references compare by the referenced element.
- Identity versus content: the type name and the free-text metadata (type mark, comments, description) identify or annotate a type rather than define it, so they are excluded from comparison.
- Instances block deletion: the model refuses to remove a type or family while placements of it exist, which is why the instance count is shown per row.
- Element selection and zoom: the tool drives the application's selection and "show element" behaviour so the user can see an instance in the model.
- Document scope: analysis is limited to the active project; the family editor and linked models are not analysed.

## Open questions
- Which exact parameter names belong in the excluded set, and in which language(s)? The original matches a fixed set of Russian and English names; should the reimplementation expose the set as an editable default, and where is that default stored?
- Should the excluded set and the near-duplicate tolerances (1–3 differing parameters, 20 reported pairs, 400 examined pairs per family) be user-editable at all, or fixed defaults?
- For duplicate types, the original keeps the alphabetically first name while families are resolved by instance count; should the two rules be made consistent, and which is preferred?
- Is the "contained copies" pass (one family's types a subset of another's) in scope, or should only fully identical families be reported?
- Tie-breaks for equal instance counts and equal-size record sets are defined but arbitrary; do users rely on them?
- Are system families, in-place families and annotation families in scope, or only loadable model families?
- Should instances in linked models, or "in use" for reasons other than placed instances, also block deletion?
- Should comparison respect the project's unit formatting and rounding, or keep the invariant numeric comparison described here?
- What retention and location does the analysis log need, and is it a user-facing feature or internal diagnostics only?
- Is there a scope control (a selection or a category subset) to add, or is "all families of the project" permanently fixed?
- Should a "reassign instances to the kept type, then delete" mode exist for in-use duplicate types, which the original never offers?
- What is the defined behaviour if the project is modified by another process between analysis and deletion?
