using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>
    /// Logger used by the whole add-in. It forwards records to every registered
    /// sink and never throws: a broken sink must not break a running tool.
    /// </summary>
    public sealed class Log
    {
        private readonly List<ILogSink> _sinks = new List<ILogSink>();
        private readonly object _sync = new object();

        /// <summary>Creates a logger with no sinks; records are then discarded.</summary>
        public Log()
        {
        }

        /// <summary>Creates a logger that forwards records to the given sinks.</summary>
        public Log(params ILogSink[] sinks)
        {
            if (sinks == null)
            {
                throw new ArgumentNullException(nameof(sinks));
            }

            foreach (ILogSink sink in sinks)
            {
                Add(sink);
            }
        }

        /// <summary>Adds a sink. The same instance is only added once.</summary>
        public void Add(ILogSink sink)
        {
            if (sink == null)
            {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_sync)
            {
                if (!_sinks.Contains(sink))
                {
                    _sinks.Add(sink);
                }
            }
        }

        /// <summary>Removes a sink.</summary>
        public void Remove(ILogSink sink)
        {
            lock (_sync)
            {
                _sinks.Remove(sink);
            }
        }

        /// <summary>Writes an informational record.</summary>
        public void Info(string message, string? source = null) => Write(LogLevel.Information, message, source, null);

        /// <summary>Writes a warning record.</summary>
        public void Warn(string message, string? source = null, Exception? exception = null) =>
            Write(LogLevel.Warning, message, source, exception);

        /// <summary>Writes an error record.</summary>
        public void Error(string message, string? source = null, Exception? exception = null) =>
            Write(LogLevel.Error, message, source, exception);

        /// <summary>Writes a record of the given level.</summary>
        public void Write(LogLevel level, string message, string? source = null, Exception? exception = null)
        {
            var entry = new LogEntry(level, message, DateTimeOffset.Now, source, exception);
            ILogSink[] snapshot;

            lock (_sync)
            {
                snapshot = _sinks.ToArray();
            }

            foreach (ILogSink sink in snapshot)
            {
                try
                {
                    sink.Write(entry);
                }
                catch (Exception)
                {
                    // Logging must never be the reason a tool fails.
                }
            }
        }
    }
}
