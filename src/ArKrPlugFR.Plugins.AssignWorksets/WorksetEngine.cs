using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Worksets;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// Walks the model once, decides the workset of every element from the user's rules
    /// and writes the decision in one undoable transaction.
    /// <para>
    /// The decision itself is made in <see cref="WorksetRules"/>; this class only reads
    /// the facts out of the Revit model, creates the worksets the rules ask for, and
    /// moves the elements. A document that is not workshared, an element the platform
    /// refuses and a workset that cannot be created are reported, never guessed at.
    /// </para>
    /// </summary>
    internal static class WorksetEngine
    {
        /// <summary>How many failure lines are kept for the report; the rest are only counted.</summary>
        private const int FailureLimit = 10;

        /// <summary>Runs the assignment.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="rules">Usable rules, in the order the user sees them.</param>
        /// <param name="createMissing">Whether worksets that do not exist yet may be created.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static WorksetOutcome Run(
            Document document,
            IReadOnlyList<WorksetRule> rules,
            bool createMissing,
            WorksetText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new WorksetOutcome();

            if (!WorksetSupport.IsWorkshared(document))
            {
                outcome.Stopped = text.NotWorkshared;

                return outcome;
            }

            Dictionary<string, WorksetId> worksets = WorksetSupport.ReadUserWorksetMap(document);
            IReadOnlyList<string> parameterNames = WorksetRules.ParameterNames(rules);
            var planned = new List<PlannedAssignment>();

            foreach (Element element in Collect(document))
            {
                outcome.Considered++;

                string? target = WorksetRules.SelectTarget(rules, ReadFacts(document, element, parameterNames));

                if (target == null)
                {
                    outcome.NoMatch++;

                    continue;
                }

                if (worksets.TryGetValue(target, out WorksetId existing))
                {
                    if (existing != null && existing == element.WorksetId)
                    {
                        outcome.AlreadyInTarget++;

                        continue;
                    }
                }
                else if (!createMissing)
                {
                    outcome.WorksetMissing++;

                    continue;
                }

                planned.Add(new PlannedAssignment(element, target));
            }

            if (planned.Count > 0)
            {
                int createdBefore = outcome.CreatedWorksets;

                bool committed = DocumentEdit.Run(document, text.TransactionName, () => Apply(document, planned, worksets, outcome, text));

                if (!committed)
                {
                    // The rollback took back the worksets this run had created on the way,
                    // so the report must not claim them.
                    outcome.CreatedWorksets = createdBefore;
                }

                log.Info(
                    "Worksets: read " + outcome.Considered + ", assigned " + outcome.Assigned +
                    ", already in place " + outcome.AlreadyInTarget + ", no match " + outcome.NoMatch +
                    ", created " + outcome.CreatedWorksets + ", refused " + outcome.Failed + ".",
                    nameof(WorksetEngine));
            }

            return outcome;
        }

        private static bool Apply(
            Document document,
            IReadOnlyList<PlannedAssignment> planned,
            Dictionary<string, WorksetId> worksets,
            WorksetOutcome outcome,
            WorksetText text)
        {
            bool changed = false;

            foreach (PlannedAssignment assignment in planned)
            {
                if (!TryResolveWorkset(document, assignment.Target, worksets, outcome, text))
                {
                    continue;
                }

                WorksetId worksetId = worksets[assignment.Target];

                if (worksetId == assignment.Element.WorksetId)
                {
                    outcome.AlreadyInTarget++;

                    continue;
                }

                try
                {
                    if (WorksetSupport.TrySetWorkset(assignment.Element, worksetId))
                    {
                        outcome.Assigned++;
                        changed = true;
                    }
                    else
                    {
                        outcome.Failed++;
                        AddFailure(outcome, text.AssignmentRefused(ElementDescription.Describe(assignment.Element)));
                    }
                }
                catch (Exception exception)
                {
                    outcome.Failed++;
                    AddFailure(outcome, text.AssignmentFailed(ElementDescription.Describe(assignment.Element), exception.Message));
                }
            }

            return changed;
        }

        private static bool TryResolveWorkset(
            Document document,
            string target,
            Dictionary<string, WorksetId> worksets,
            WorksetOutcome outcome,
            WorksetText text)
        {
            if (worksets.ContainsKey(target))
            {
                return true;
            }

            if (!WorksetTable.IsWorksetNameUnique(document, target))
            {
                // The name is taken by a workset the map did not hold — most likely one
                // created while this run was prepared. Read the table again before giving up.
                foreach (KeyValuePair<string, WorksetId> pair in WorksetSupport.ReadUserWorksetMap(document))
                {
                    worksets[pair.Key] = pair.Value;
                }

                if (worksets.ContainsKey(target))
                {
                    return true;
                }

                outcome.Failed++;
                AddFailure(outcome, text.NotUnique(target));

                return false;
            }

            try
            {
                worksets[target] = Workset.Create(document, target).Id;
                outcome.CreatedWorksets++;

                return true;
            }
            catch (Exception exception)
            {
                outcome.Failed++;
                AddFailure(outcome, text.CreateFailed(target, exception.Message));

                return false;
            }
        }

        private static IEnumerable<Element> Collect(Document document)
        {
            foreach (Element element in new FilteredElementCollector(document).WhereElementIsNotElementType())
            {
                if (element.Category == null || element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM) == null)
                {
                    continue;
                }

                yield return element;
            }
        }

        private static WorksetElementFacts ReadFacts(
            Document document,
            Element element,
            IReadOnlyList<string> parameterNames)
        {
            ElementType? type = document.GetElement(element.GetTypeId()) as ElementType;
            string? family = (element as FamilyInstance)?.Symbol?.Family?.Name;

            Dictionary<string, string?>? values = null;

            if (parameterNames.Count > 0)
            {
                values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

                foreach (string name in parameterNames)
                {
                    values[name] = ReadParameterValue(element, type, name);
                }
            }

            return new WorksetElementFacts(
                element.Category?.Name,
                BuiltInCategoryName(element.Category),
                family,
                type?.Name,
                values);
        }

        private static string? ReadParameterValue(Element element, ElementType? type, string name)
        {
            Parameter? parameter = element.LookupParameter(name) ?? type?.LookupParameter(name);

            // Only a text value is a workset name; anything else counts as absent, so the
            // element falls through to the next rule instead of failing.
            string? value = parameter?.AsString();

            return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
        }

        private static string? BuiltInCategoryName(Category? category)
        {
            if (category == null)
            {
                return null;
            }

            string name = ((BuiltInCategory)ElementIdCompat.GetValue(category.Id)).ToString();

            return name.StartsWith("OST_", StringComparison.Ordinal) ? name : null;
        }

        private static void AddFailure(WorksetOutcome outcome, string line)
        {
            if (outcome.Failures.Count < FailureLimit)
            {
                outcome.Failures.Add(line);
            }
        }

        private sealed class PlannedAssignment
        {
            internal PlannedAssignment(Element element, string target)
            {
                Element = element;
                Target = target;
            }

            internal Element Element { get; }

            internal string Target { get; }
        }
    }
}
