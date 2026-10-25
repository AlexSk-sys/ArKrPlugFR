# Assign elements to worksets — functional specification

## Purpose
Put the model elements of a workshared project into worksets by rules the user writes
once. Modelling leaves elements in the default workset or in the wrong one; sorting them
by hand means clicking through element properties one by one. This tool reads the rules,
finds the workset of every element itself, creates the worksets that do not exist yet and
moves the elements in one undoable step.

## Inputs
- An open project with worksharing enabled. A project that is not workshared stops the
  run with a message and nothing changes.
- An ordered list of rules. A rule has a kind, a pattern and a workset:
  - **Category** — one or more category names; a name is the one shown in the interface
    or the built-in one such as `OST_Walls`.
  - **Family prefix** — one or more prefixes the family name starts with.
  - **Type prefix** — one or more prefixes the type name starts with.
  - **Parameter value** — the name of a text parameter whose value becomes the workset
    name; the workset field of the rule is not used.
- Several values inside a pattern are separated by commas or semicolons.
- The switch that allows creating worksets that do not exist yet; on by default.

## Behaviour
1. Require a workshared document. Without one, report it and change nothing.
2. Keep only the usable rules: a known kind, a non-empty pattern, and — except for the
   parameter kind — a non-empty workset. When no usable rule is left, report it and
   change nothing.
3. Read the user worksets of the document once.
4. Walk the model: every instance element that has a category and the workset parameter
   is a candidate; element types are never touched, and elements of linked models are not
   part of the current document and are never seen.
5. Read the facts of each candidate — category name and built-in name, family name, type
   name, and the values of the parameters the rules name (the instance parameter first,
   then the parameter of its type) — and ask the rules for the workset. The first usable
   matching rule wins. A parameter rule matches only when the parameter carries a value;
   an empty value falls through to the rules below it.
6. An element that matches no rule, and an element whose workset may not be created and
   does not exist, are counted and left alone.
7. In one transaction: create the worksets the planned elements need — unless the switch
   is off — and write the workset parameter of every element that is not already in its
   target workset. An element or a workset the platform refuses is reported with its id
   and the reason, and the run continues.
8. When nothing was moved, the transaction is rolled back and leaves no entry in the undo
   list, so a second run that finds everything in place changes nothing.
9. Report: how many elements were read, assigned, already in their workset, without a
   matching rule, missing their workset, and refused; plus how many worksets were
   created, with the first few refusals listed one by one.
10. Change nothing else: no workset is renamed or deleted, no element is created or
    deleted, and no view workset is touched.

## Results and messages
- Not workshared: "This project is not workshared; the tool needs a project with
  worksharing enabled." — nothing changed.
- No usable rules: "Add at least one rule: the match must not be empty, and a workset is
  needed everywhere except in a rule that reads it from a parameter." — nothing changed.
- Success: "Read {0} element(s); assigned: {1}; already in place: {2}; no matching rule:
  {3}; workset missing: {4}; refused: {5}." and, when applicable, "Worksets created:
  {0}." — informational.
- Every message is provided in Russian and English; the window carries a title, a
  subtitle, the rule list and a close button, and closes on Escape.

## Edge cases and failures
- A document without worksharing: reported, nothing changed.
- An unknown kind in the settings file, an empty pattern, a workset field left empty for
  a matching kind: the rule is skipped; when nothing usable is left the run says so.
- A parameter rule whose parameter is empty, missing, or not a text parameter: the
  element falls through to the next rule instead of failing.
- A pattern with several values: any one of them matches.
- Names are matched without case, both for rules and for worksets.
- A workset name that is already taken but whose workset was not seen: the table is read
  again; when the workset still cannot be resolved the name is reported and skipped.
- An element the platform refuses to move — a checked-out copy, a pinned element, a
  read-only workset parameter: reported with its id, the run continues with the rest.
- Hundreds of elements: one transaction, one undoable step.

## UI surfaces
- A tool window: the ordered rule list (kind, match, workset, a remove button per row),
  a button that adds a rule, the switch that allows creating missing worksets, a hint
  that explains what each kind matches, a report area and the run and close buttons. The
  rules are remembered between sessions.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the worksets panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Worksharing: `Document.IsWorkshared`, the user worksets read with
  `FilteredWorksetCollector`, a new workset created with `Workset.Create`, and the unique
  workset name checked with `WorksetTable.IsWorksetNameUnique`.
- Reassignment: the workset of an element is written through its workset parameter
  (`BuiltInParameter.ELEM_PARTITION_PARAM`), which is the documented way to move an
  element between worksets.
- Element facts: the category and its built-in name, the family of a family instance,
  the name of its type, and parameter lookup — the instance parameter first, the type
  parameter second.
- Transactions: the whole run is one undoable step and is rolled back when nothing moved.

## Scope of this release
Implemented: everything described above — the four rule kinds with ordered precedence,
the creation of missing worksets, the counts and the failure list, the remembered rules,
the bilingual window, and the workset logic covered by unit tests without Revit.

Not implemented: rules that derive worksets from the names of linked Revit files or from
the parts of that name; a single worksets for every imported DWG/DXF and CAD link; XML
settings files (settings are JSON under the user's profile here); and editing a rule by
double-clicking a table cell — every field is edited in place instead.
