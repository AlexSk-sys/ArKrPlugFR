using System;

namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>A single log record.</summary>
    public sealed class LogEntry
    {
        /// <summary>Creates a log record.</summary>
        /// <param name="level">Severity of the record.</param>
        /// <param name="message">Text of the record.</param>
        /// <param name="timestamp">Moment the record was produced; the current time is used when omitted.</param>
        /// <param name="source">Name of the component that produced the record.</param>
        /// <param name="exception">Optional exception attached to the record.</param>
        public LogEntry(
            LogLevel level,
            string message,
            DateTimeOffset? timestamp = null,
            string? source = null,
            Exception? exception = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message must not be empty.", nameof(message));
            }

            Level = level;
            Message = message;
            Timestamp = timestamp ?? DateTimeOffset.Now;
            Source = source;
            Exception = exception;
        }

        /// <summary>Gets the severity of the record.</summary>
        public LogLevel Level { get; }

        /// <summary>Gets the text of the record.</summary>
        public string Message { get; }

        /// <summary>Gets the moment the record was produced.</summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>Gets the name of the component that produced the record, if known.</summary>
        public string? Source { get; }

        /// <summary>Gets the exception attached to the record, if any.</summary>
        public Exception? Exception { get; }

        /// <summary>Renders the record as a single line, without the exception.</summary>
        public override string ToString()
        {
            string source = string.IsNullOrWhiteSpace(Source) ? string.Empty : " [" + Source + "]";
            return Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + Level.ToString().ToUpperInvariant() + source + " " + Message;
        }
    }
}
