# Select elements by family type — functional specification

## Purpose
Finding every instance of a family type usually means walking the project browser into the
category, into the family, and picking the types one by one. This tool lists the types of the
project as one tree — category, family, type — with a search field and a scope, and selects the
instances of the ticked types in one press. It only reads the project.

## Inputs
- An open project with an active view.
- The scope: all families, the types placed in the model, or the types visible in the current
  view. Each scope shows how many types it holds. On a sheet the view scope is not available and
  is not offered.
- A search query; an empty query shows every type of the scope.
- Ticked types; ticking a category or a family ticks everything inside it.

## Behaviour
1. Read the types of every scope when the window opens: the family types of the project, those
   with at least one instance, and those with an instance visible in the active view.
2. Build the tree from the facts of the chosen scope and the query: categories, families and types
   sorted by name, every level counting the types below it. A type without a category sits under
   the label "No category".
3. The query is matched without case against the family name and the type name, as a piece of a
   name rather than a pattern; categories and families without a match disappear from the tree.
4. Ticks survive a change of scope or query: a ticked type stays ticked when it appears again.
5. On the run, collect the instances of every ticked type in the scope, make them the selection and
   report: types ticked, elements selected, and types that hold no instance in this scope.
6. Change nothing else: no transaction is opened, nothing is created, deleted or written to.

## Results and messages
- Nothing ticked: "Tick at least one type." — or, when the scope holds no types at all, "The scope
  holds no types." — nothing is selected.
- Success: "Types ticked: {0}; elements selected: {1}; types without instances: {2}." —
  informational.
- Every message is provided in Russian and English; the window carries a title, a subtitle, a scope
  list with counts, a search field, the tree with tick boxes, a report area and the run and close
  buttons, and closes on Escape.

## Edge cases and failures
- A sheet is active: the view scope is not offered and the window says why.
- A type without a category: shown under the label.
- The same type name in two families: two rows.
- A ticked type without instances in the scope: counted; nothing is selected for it.
- A large project: the types of the three scopes are read once, when the window opens.
- The tool never writes, so no undo entry is created even when the selection ends up empty.

## UI surfaces
- A tool window: the scope list with the count of types of each scope, the search field, the tree
  with tick boxes — a parent ticks everything under it and shows the in-between state — the buttons
  that tick and untick everything, the report area and the run and close buttons.
- The ribbon button carries the tool caption and a bilingual description; the tool sits in the
  families panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Family types: the `FamilySymbol` elements of the project, their family, their category and their
  names.
- Instances: elements that carry a type (`GetTypeId`), collected from the project or from one view.
- The selection: `Selection.SetElementIds`; the tool opens no transaction.
- The active view: a sheet is not a model view, so the view scope needs a model view.

## Scope of this release
Implemented: everything described above — the three scopes with counts, the search, the tree with
ticks that survive a rebuild, the selection of the instances and the report.

Not implemented: selecting instances as rows are clicked while browsing; grouping by anything but
category and family; and searching inside parameter values — the query reads names only.
