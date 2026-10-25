namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>
    /// Destination of log records. Implementations must be safe to call from
    /// several threads, because Revit can run an event handler while a command
    /// is still finishing.
    /// </summary>
    public interface ILogSink
    {
        /// <summary>Writes a single record.</summary>
        void Write(LogEntry entry);
    }
}
