using System.Collections.Generic;

namespace ArKrPlugFR.Core.Worksets
{
    /// <summary>
    /// The remembered state of the tool: the ordered rules and whether missing worksets
    /// may be created. It is stored as one JSON file per user; a missing, unreadable or
    /// corrupt file falls back to these defaults.
    /// </summary>
    public sealed class WorksetSettings
    {
        /// <summary>Gets or sets a value indicating whether a run creates worksets that do not exist yet.</summary>
        public bool CreateMissingWorksets { get; set; } = true;

        /// <summary>Gets or sets the rule list, in the order the user sees it.</summary>
        public List<WorksetRule> Rules { get; set; } = new List<WorksetRule>();
    }
}
