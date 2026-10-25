using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>
    /// Writes records to a daily text file under a directory owned by the
    /// application, and removes files older than the retention period.
    /// <para>
    /// A tool that changes a model must leave a trace: when an engineer asks why
    /// an element moved, the answer has to be on disk.
    /// </para>
    /// </summary>
    public sealed class FileLogSink : ILogSink
    {
        private const int DefaultRetentionDays = 14;

        private readonly string _directory;
        private readonly int _retentionDays;
        private readonly object _sync = new object();
        private readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        private DateTime _lastCleanupDate = DateTime.MinValue;

        /// <summary>Creates a sink that writes into the given directory.</summary>
        /// <param name="directory">Directory for the log files; created when missing.</param>
        /// <param name="retentionDays">Number of daily files to keep; must be positive.</param>
        public FileLogSink(string directory, int retentionDays = DefaultRetentionDays)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("The directory must not be empty.", nameof(directory));
            }

            if (retentionDays <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retentionDays), retentionDays, "The retention period must be positive.");
            }

            _directory = directory;
            _retentionDays = retentionDays;
        }

        /// <inheritdoc />
        public void Write(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            lock (_sync)
            {
                Directory.CreateDirectory(_directory);
                CleanupIfNeeded(entry.Timestamp.Date);

                string path = GetPath(entry.Timestamp);
                var builder = new StringBuilder(entry.ToString());

                if (entry.Exception != null)
                {
                    builder.AppendLine();
                    builder.Append("    ").Append(entry.Exception);
                }

                builder.AppendLine();
                File.AppendAllText(path, builder.ToString(), _encoding);
            }
        }

        /// <summary>Returns the file a record with the given timestamp is written to.</summary>
        public string GetPath(DateTimeOffset timestamp)
        {
            string name = "ArKrPlugFR-" + timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".log";
            return Path.Combine(_directory, name);
        }

        private void CleanupIfNeeded(DateTime today)
        {
            if (_lastCleanupDate == today)
            {
                return;
            }

            _lastCleanupDate = today;

            try
            {
                DateTime threshold = today.AddDays(-_retentionDays);
                var files = Directory
                    .EnumerateFiles(_directory, "ArKrPlugFR-*.log")
                    .Select(path => new FileInfo(path))
                    .Where(info => info.LastWriteTime.Date < threshold)
                    .ToList();

                foreach (FileInfo file in files)
                {
                    file.Delete();
                }
            }
            catch (IOException)
            {
                // Cleanup is best effort; a locked file is retried on the next run.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
