using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Views
{
    /// <summary>
    /// What a template, or the element a template would be applied to, is.
    /// </summary>
    public enum TemplateKind
    {
        /// <summary>No template at all: the "No template" entry of the picker.</summary>
        None,

        /// <summary>An ordinary view template, applied to views and drafting views.</summary>
        View,

        /// <summary>A legend template, applied to legends.</summary>
        Legend,

        /// <summary>A schedule template, applied to schedules.</summary>
        Schedule,

        /// <summary>A template whose view type says nothing, which fits nothing.</summary>
        Undefined
    }

    /// <summary>
    /// Which template may be applied to which copy, and how the picker is searched.
    /// <para>
    /// A schedule template cannot shape a plan and a legend template cannot shape a
    /// schedule, so the chosen template is checked against every kind of ticked
    /// element before the run; a template that fits nothing is not applied.
    /// </para>
    /// </summary>
    public static class TemplateRules
    {
        /// <summary>
        /// Tells whether a template kind may be applied to a copied element kind.
        /// </summary>
        /// <param name="template">Kind of the chosen template.</param>
        /// <param name="target">Kind of the element the template would be applied to.</param>
        /// <returns><c>true</c> when the template fits the element.</returns>
        public static bool Fits(TemplateKind template, TemplateKind target)
        {
            if (template == TemplateKind.None)
            {
                return true;
            }

            if (template == TemplateKind.Undefined || target == TemplateKind.Undefined)
            {
                return false;
            }

            if (template == TemplateKind.Schedule)
            {
                return target == TemplateKind.Schedule;
            }

            if (template == TemplateKind.Legend)
            {
                return target == TemplateKind.Legend;
            }

            return target == TemplateKind.View;
        }

        /// <summary>Splits a search field into terms on spaces, commas and semicolons.</summary>
        /// <param name="searchText">Text the user typed; may be <c>null</c>.</param>
        /// <returns>The terms to look for, in the order they were typed.</returns>
        public static IReadOnlyList<string> Terms(string? searchText)
        {
            var terms = new List<string>();

            if (searchText == null)
            {
                return terms;
            }

            foreach (string part in searchText.Split(
                new[] { ' ', ',', ';' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = part.Trim();

                if (trimmed.Length > 0)
                {
                    terms.Add(trimmed);
                }
            }

            return terms;
        }

        /// <summary>
        /// Tells whether any term of the search occurs in any of the given texts; an
        /// empty search matches everything.
        /// </summary>
        /// <param name="searchText">Text the user typed; may be <c>null</c>.</param>
        /// <param name="texts">Names and descriptions of one candidate.</param>
        /// <returns><c>true</c> when the candidate stays in the list.</returns>
        public static bool Matches(string? searchText, IEnumerable<string?> texts)
        {
            if (texts == null)
            {
                throw new ArgumentNullException(nameof(texts));
            }

            IReadOnlyList<string> terms = Terms(searchText);

            if (terms.Count == 0)
            {
                return true;
            }

            foreach (string? text in texts)
            {
                if (text == null || text.Length == 0)
                {
                    continue;
                }

                foreach (string term in terms)
                {
                    if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
