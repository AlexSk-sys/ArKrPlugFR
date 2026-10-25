using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>
    /// Keeps the most recent records in memory so a window can show what a tool
    /// just did without reading the log file back.
    /// </summary>
    public sealed class RingBufferLogSink : ILogSink
    {
        private readonly Queue<LogEntry> _entries;
        private readonly object _sync = new object();

        /// <summary>Creates a sink that keeps the given number of records.</summary>
        /// <param name="capacity">Maximum number of records to keep; must be positive.</param>
        public RingBufferLogSink(int capacity = 200)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "The capacity must be positive.");
            }

            _entries = new Queue<LogEntry>(capacity);
            Capacity = capacity;
        }

        /// <summary>Gets the maximum number of records kept.</summary>
        public int Capacity { get; }

        /// <inheritdoc />
        public void Write(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            lock (_sync)
            {
                while (_entries.Count >= Capacity)
                {
                    _entries.Dequeue();
                }

                _entries.Enqueue(entry);
            }
        }

        /// <summary>Returns a copy of the records, oldest first.</summary>
        public IReadOnlyList<LogEntry> Snapshot()
        {
            lock (_sync)
            {
                return new List<LogEntry>(_entries);
            }
        }

        /// <summary>Removes every record and returns the ones that were kept.</summary>
        public IReadOnlyList<LogEntry> Drain()
        {
            lock (_sync)
            {
                var result = new List<LogEntry>(_entries);
                _entries.Clear();
                return result;
            }
        }
    }
}
