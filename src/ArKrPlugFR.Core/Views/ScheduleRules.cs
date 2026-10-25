using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Sheets;

namespace ArKrPlugFR.Core.Views
{
    /// <summary>
    /// Which schedules may be copied at all, and which names read like a schedule.
    /// <para>
    /// A project keeps some schedules for itself: the revision schedules that hang off
    /// the title block are served by the platform and cannot be duplicated. Their names
    /// give them away — they start with the word the platform uses, or they carry a
    /// stamp word — so the rule can be decided without opening a schedule.
    /// </para>
    /// </summary>
    public static class ScheduleRules
    {
        /// <summary>The word the platform puts in front of its own revision schedules.</summary>
        public const string RevisionPrefix = "Revision";

        /// <summary>
        /// Words that mark a name as a schedule rather than a view; the list carries the
        /// two languages of the tool and no name of a specific organisation.
        /// </summary>
        public static readonly IReadOnlyList<string> ScheduleWords = new[]
        {
            "schedule",
            "key",
            "table",
            "list",
            "ведомость",
            "спецификация",
            "таблица",
            "перечень"
        };

        /// <summary>
        /// Tells whether a schedule has to be left alone: an empty name, a revision
        /// schedule, or one that carries a stamp word.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <returns><c>true</c> when the schedule must not be copied.</returns>
        public static bool IsServedByName(string? name)
        {
            if (name == null || name.Trim().Length == 0)
            {
                return true;
            }

            string trimmed = name.Trim();

            if (trimmed.StartsWith(RevisionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return StampKeywords.LooksLikeStamp(new[] { trimmed });
        }

        /// <summary>Tells whether a name reads like a schedule.</summary>
        /// <param name="name">Name to read.</param>
        /// <returns><c>true</c> when one of the schedule words occurs in the name.</returns>
        public static bool LooksLikeSchedule(string? name)
        {
            if (name == null || name.Length == 0)
            {
                return false;
            }

            foreach (string word in ScheduleWords)
            {
                if (name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
