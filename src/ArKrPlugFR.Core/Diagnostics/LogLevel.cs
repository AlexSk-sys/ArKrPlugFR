namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>Severity of a log entry.</summary>
    public enum LogLevel
    {
        /// <summary>Diagnostic detail used while developing a plugin.</summary>
        Debug = 0,

        /// <summary>Normal progress of a tool.</summary>
        Information = 1,

        /// <summary>Something unexpected that did not stop the tool.</summary>
        Warning = 2,

        /// <summary>The tool failed or skipped work it was asked to do.</summary>
        Error = 3,
    }
}
