# Rebar cage metres — functional specification

## Purpose

Work out how many running metres of bar a rebar cage holds and write the number into the
cage. A cage is placed along layout lines; the longitudinal bars follow those lines, while
the inclined and transverse bars repeat along the cage at their own steps. Adding the three
groups up by hand is what the tool does in one action.

## Inputs

- An open project document.
- The layout lines of the cage, picked in the model: any curve element, several at a time.
- One cage instance, picked in the model: a family instance that carries the cage
  parameters and, as its sub-components, the bars it is built from.
- Parameter names, read from the tool's settings file; each has a default of the design and
  can be changed without recompiling:
  - the number of longitudinal bars of the cage: `Колво_горизонт`;
  - the step of the inclined bars: `Шаг наклонных`;
  - the step of the transverse bars: `Шаг поперечных`;
  - the length of a bar, read on every sub-component: `ADSK_Длина арматуры`;
  - the flags that tell the three groups of sub-components apart:
    `Продольный стержень`, `Вертикальный стержень`, `Поперечный стержень`;
  - the parameter that receives the calculated length: `Длина ПМ расчетная`.

## Behaviour

1. On start, require an active project document. Without one, report an error and change
   nothing.
2. Ask for the layout lines and for the cage. A cancelled pick leaves the previous choice in
   place and writes nothing.
3. Sum the lengths of the picked lines: that is the cage length in metres.
4. Read the cage's own parameters: the number of longitudinal bars, the inclined step and
   the transverse step. A missing or unusable value is reported and the group it belongs to
   contributes nothing.
5. Read the bars of the cage: every sub-component of the cage that is a family instance is
   sorted into one of the three groups by its flags; a sub-component that carries no flag is
   counted as unclassified, and a bar whose length cannot be read simply adds nothing to its
   group.
   - the longitudinal group contributes only its presence; its lengths are not used,
     because the longitudinal bars follow the picked lines;
   - the inclined group contributes the longest of its bar lengths;
   - the transverse group contributes its distinct bar lengths, one entry per length.
6. Compute, in metres:
   - longitudinal = cage length × number of longitudinal bars;
   - positions of the inclined bars = the smallest whole number of steps that covers the
     cage length, plus one — `ceil(length / step) + 1`; the same rule gives the positions of
     the transverse bars;
   - inclined = positions of the inclined bars × 2 × the inclined bar length;
   - transverse = positions of the transverse bars × the sum of the distinct transverse bar
     lengths;
   - total = longitudinal + inclined + transverse, rounded to two decimals.
7. In one transaction, write the total into the cage's calculated-length parameter when the
   parameter is a number and is writable; a parameter of another kind, a read-only one or a
   missing one is reported, and the calculation is still shown.
8. Report: the cage family, the number of lines and their total length, the three groups
   with their numbers and lengths, the total, and every warning (missing parameter, zero
   step, no inclined length, no transverse length, unclassified sub-components).
9. Change nothing else: no element is created, moved or deleted; only the calculated-length
   parameter of the cage changes, and only when the user has not switched the write off.

## Results and messages

- No active document: error, nothing changed.
- No lines picked, or no cage picked: the action reports what is missing and changes
  nothing.
- Missing cage parameter: the report names it and the group it belongs to is left out of
  the total.
- Missing bar lengths: the report says so and the group is left out of the total.
- Success: the report shows the three groups and the total, and states that the total was
  written into the parameter.
- Every message is provided in Russian and English; the tool window carries a title, a
  subtitle and a close button, and closes on Escape.

## Edge cases and failures

- A step of zero or a missing step: no positions are computed for that group, and the report
  says which step was unusable.
- A cage length of zero: one position is computed for a usable step, as the formula
  prescribes.
- Sub-components whose length is unusable: skipped and counted.
- Sub-components that carry two flags: the first group in the order longitudinal, inclined,
  transverse wins, so a bar is never counted twice.
- A calculated-length parameter of a kind that cannot hold a number: reported, nothing
  written.
- Running the tool again with the same inputs writes the same number; it is safe to run
  repeatedly.

## UI surfaces

- A tool window: two pick buttons with the picked state under them, the calculated-length
  parameter as a field, a switch that turns the write off, a run button, a report area and a
  close button.
- The ribbon button carries the tool caption and a bilingual description. On the ribbon the
  tool sits in the rebar panel.
- All text is provided in Russian and English.

## Revit concepts involved

- Curve elements picked in the model; their lengths are summed.
- A family instance as the cage, its instance and type parameters read by name, and its
  sub-components (`GetSubComponentIds`) as the bars it is built from.
- Length parameters are read in the document's units and converted to metres; a length stored
  in an integer parameter is read as millimetres, because an integer carries no unit of its
  own. The parameter written into is fitted to its storage type (a number parameter receives
  the number, a text parameter its text).
- Transactions: the run is one undoable step, and nothing is written when the total is not
  written.

## Scope of this release

Implemented: everything described above — the picked lines and cage, the three groups with
their flags and lengths, the position rule with rounding up, the three sums, the total, the
write into the calculated-length parameter, the warnings, one transaction, the bilingual
window and report.

Not implemented: the reference's auxiliary count parameters (they fed the report only);
cages built without sub-components; reading bars from linked models; any change to the
geometry of the cage or of the bars.
