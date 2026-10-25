# Count structures by mark — functional specification

## Purpose
Write into every rebar bar the number of bearing structures that carry the same mark as
the structure the bar belongs to. A column marked "Км-1" that stands four times in the
project makes `Кол_Конструкций` of its bars read 4; schedules and drawings then show how
many structures each bar serves without counting by hand.

## Inputs
- An open project document.
- The kind of structures to count, chosen in the window: columns (structural and
  architectural), walls, floors, beams, or all four at once.
- Three parameter names, read from the tool's settings file; each has a default of the
  design and can be changed without recompiling:
  - the mark of a structure: `Марка`;
  - the mark of the structure a bar belongs to: `Метка основы`;
  - the number written into a bar: `Кол_Конструкций`.

## Behaviour
1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Collect the structures of the chosen kinds and read their mark parameter. A structure
   without a usable mark — missing parameter, or an empty value — does not take part in
   the count.
3. Count the structures per mark. Marks are compared after trimming, without case:
   "КМ-1", " км-1 " and "Км-1" are one mark.
4. Collect every rebar bar of the project.
5. For every bar:
   - read its structure mark; an empty one means the bar is counted as skipped;
   - look the mark up in the count; a mark that no structure carries means skipped;
   - find the number parameter on the bar; a missing or read-only parameter means
     skipped;
   - otherwise plan the write. The value is fitted to the parameter's storage: an
     integer parameter receives a whole number, a text parameter its text, a number
     parameter the same value.
6. In one transaction, write the planned numbers. A bar the platform refuses is reported
   with its id and the reason, and the run continues.
7. Report: how many structures were counted, how many distinct marks they carry, how many
   bars were read, how many were updated, how many were skipped for each of the reasons
   above, and how many were refused, with the first few refusals listed one by one.
8. Change nothing else: no element is created, moved or deleted; only the number
   parameter of bars changes.

## Results and messages
- No active document: error, nothing changed.
- No structure of the chosen kinds, or none with a usable mark: "No structures with a mark
  of the chosen kind were found." — nothing changed.
- No bars in the project: "The project holds no rebar bars." — nothing changed.
- Nothing to write because every bar was skipped: the report says so and lists the
  reasons.
- Success: "Structures: {0} in {1} mark(s); bars: {2}; updated: {3}; without a structure
  mark: {4}; without a count for their mark: {5}; without the number parameter: {6};
  refused: {7}." — informational.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures
- A structure with an empty mark: not counted, and no bar is updated through it.
- Two marks differing only in case or spaces: one mark.
- A bar whose structure mark is empty or unknown: skipped and counted, never written zero.
- A bar without the number parameter, or with it read-only: skipped and counted.
- A parameter that cannot store the number (for example an element id): skipped.
- A bar the platform refuses to write: reported with its id and the reason, the run
  continues.
- Bars inside systems (RebarInSystem) are out of scope of this release and are not read.
- Running the tool again writes the same numbers and reports nothing refused; it is safe
  to run repeatedly.

## UI surfaces
- A tool window: the kind of structures as one list of five choices, a hint, a run button
  and a close button; the result is shown in the window and in the shared result window.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon
  the tool sits in the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved
- Categories of columns, walls, floors and beams, and their elements.
- The mark parameter of a structure and the structure mark parameter of a bar, read by
  name; the number parameter of a bar, written with the value fitted to its storage type.
- Rebar elements as the bars of the project.
- Transactions: the whole run is one undoable step and is rolled back when nothing was
  written.

## Scope of this release
Implemented: everything described above — the four kinds plus all, the mark tally, the
parameter names with their defaults, the skip reasons, the value fitted to the parameter's
storage, one transaction, the bilingual window and report.

Not implemented: bars inside rebar systems; multi-value parameters; any change to the
structures themselves; and reading marks from linked models (the tool counts structures of
the current project).
