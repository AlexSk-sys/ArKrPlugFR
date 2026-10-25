using System.Collections.Generic;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Keeps the report short: an engine remembers the first few lines about what it left
    /// out and counts the rest, so a run over a whole project still reads like a report.
    /// </summary>
    internal static class TransferNotes
    {
        /// <summary>How many lines about one kind are kept.</summary>
        internal const int LineLimit = 10;

        /// <summary>Adds a line when there is still room for it.</summary>
        /// <param name="lines">Lines kept by the run.</param>
        /// <param name="line">Line to add.</param>
        internal static void Add(List<string> lines, string line)
        {
            if (lines.Count < LineLimit)
            {
                lines.Add(line);
            }
        }
    }
}
