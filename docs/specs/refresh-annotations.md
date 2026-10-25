# Refresh annotations — functional specification

## Purpose
Make the tags of the active view recompute their text. After the model changes, the
platform sometimes leaves a tag showing what it showed before, until something in the
view is edited. The tool nudges every tag of the active view — moves its head by a
millimetre and back, so the platform recomputes the text and redraws the annotation —
and then refreshes the view. Every tag ends exactly where it started; nothing about the
project's data changes.

## Inputs
- An open project document with an active view.
- The current selection, optionally: when elements are selected, only the tags of those
  elements are refreshed. An empty selection means every tag of the active view.
- The active view: tags of other views are left alone.

## Behaviour
1. On start, require an active project document and an active view. Without them, report
   an error and change nothing.
2. Collect the independent tags of the active view (titles in elevation, tags on section
   views, callouts and the like). Room and space tags are left alone in this release.
3. Read the selection. With an empty selection every tag of the view is planned; with a
   selection, a tag is planned when the tag itself or one of the elements it tags is
   selected.
4. Orphaned tags — tags whose element is gone — are not planned; they are counted and
   reported.
5. In one transaction, for every planned tag: read its head position, move the head by
   1 mm, and move it back. The move forces the platform to recompute the annotation; the
   position after the run is the position before it, to the last decimal.
6. Regenerate the document inside the same transaction, so the recomputed annotations are
   committed as one undoable step. When nothing was nudged, no transaction is kept.
7. Refresh the active view, so the new text reaches the screen without a manual reload.
8. Report: how many tags the view holds, how many were refreshed, how many were orphaned,
   and how many were refused, with the first few refusals listed one by one.
9. Change nothing else: the project's data, the tags' text and positions, the selection and
   the view's own settings stay exactly as they were.

## Results and messages
- No active document or no active view: "Open a view before running this tool." — error,
  nothing changed.
- The active view holds no tags: "The active view has no tags to refresh." — nothing
  changed.
- The selection matches no tag: "None of the selected elements carries a tag in the active
  view." — nothing changed.
- Success: "Refreshed {0} of {1} tag(s) in view '{2}'; orphaned: {3}; refused: {4}." —
  informational.
- Every message is provided in Russian and English; the result window carries a title, a
  status line and a close button, and closes on Escape.

## Edge cases and failures
- A tag whose head cannot be moved — a pinned tag, or one inside a locked group: the
  refusal is reported with the tag's id and the run continues with the rest.
- An orphaned tag: counted, not nudged, no error.
- A view the platform refuses to refresh (for example a view that is not currently
  displayable): the refresh is skipped, the nudges that were made stand, and the run
  still reports what it did.
- A schedule or another view kind that cannot hold tags: it simply has none, which is
  reported as an empty view.
- Re-running the tool with nothing selected refreshes every tag again; the tool is safe
  to run repeatedly.

## UI surfaces
- No window: the tool runs from the ribbon on the active view and the current selection,
  and reports through the shared result window (caption, status line, close button,
  Escape to close).
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the annotations panel.
- All text is provided in Russian and English.

## Revit concepts involved
- The active view of the document and its own view-specific elements.
- Independent tags: their head position, their host element, and whether they are
  orphaned.
- Regeneration: an edit inside a transaction that makes the platform recompute the
  affected elements.
- The undoable transaction: the whole run is one step and is rolled back when nothing
  was nudged.
- Refreshing the active view through the user interface document.

## Scope of this release
Implemented: everything described above — the tags of the active view, the optional
selection, the orphan count, the nudge by a millimetre and back, one transaction, the
regeneration, the view refresh, and the bilingual report.

Not implemented: room and space tags, dimensions and text notes (independent tags only);
tags of views other than the active one; nudging through a project parameter (the tool
needs no parameter of any template, unlike the approach this tool replaces); and any
change to the text of a tag, which is computed by the platform.
