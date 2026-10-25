using System;
using System.IO;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class LogTests
    {
        [Fact]
        public void Log_WritesToEverySink()
        {
            var first = new RingBufferLogSink(10);
            var second = new RingBufferLogSink(10);
            var log = new Log(first, second);

            log.Info("started", "UnjoinElements");

            Assert.Single(first.Snapshot());
            Assert.Single(second.Snapshot());
            Assert.Equal("UnjoinElements", first.Snapshot()[0].Source);
        }

        [Fact]
        public void Log_IgnoresFailingSinks()
        {
            var log = new Log(new ThrowingSink());
            var healthy = new RingBufferLogSink(10);
            log.Add(healthy);

            log.Error("still written", "test");

            Assert.Single(healthy.Snapshot());
        }

        [Fact]
        public void LogEntry_RequiresMessage()
        {
            Assert.Throws<ArgumentException>(() => new LogEntry(LogLevel.Information, " "));
        }

        [Fact]
        public void LogEntry_ToString_ContainsLevelAndSource()
        {
            var entry = new LogEntry(LogLevel.Warning, "something odd", new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), "Tool");

            string text = entry.ToString();

            Assert.Contains("2026-01-02 03:04:05", text);
            Assert.Contains("WARNING", text);
            Assert.Contains("[Tool]", text);
            Assert.Contains("something odd", text);
        }

        [Fact]
        public void RingBuffer_DropsOldestRecords()
        {
            var sink = new RingBufferLogSink(3);
            var log = new Log(sink);

            for (int i = 1; i <= 5; i++)
            {
                log.Info("record " + i);
            }

            var snapshot = sink.Snapshot();

            Assert.Equal(3, snapshot.Count);
            Assert.Equal("record 3", snapshot[0].Message);
            Assert.Equal("record 5", snapshot[2].Message);
        }

        [Fact]
        public void FileLogSink_AppendsToDailyFileAndKeepsRetention()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ArKrPlugFR-tests-" + Guid.NewGuid().ToString("N"));

            try
            {
                var sink = new FileLogSink(directory, retentionDays: 2);
                var today = new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero);

                sink.Write(new LogEntry(LogLevel.Information, "first", today, "test"));
                sink.Write(new LogEntry(LogLevel.Error, "second", today, "test", new InvalidOperationException("boom")));

                string expected = Path.Combine(directory, "ArKrPlugFR-20260520.log");
                Assert.True(File.Exists(expected));
                Assert.Equal(expected, sink.GetPath(today));

                string content = File.ReadAllText(expected);
                Assert.Contains("first", content);
                Assert.Contains("second", content);
                Assert.Contains("boom", content);
                Assert.Equal(2, content.Split('\n').Count(line => line.Contains("first") || line.Contains("second")));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        [Fact]
        public void FileLogSink_RemovesFilesOlderThanRetention()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ArKrPlugFR-tests-" + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(directory);
                string stale = Path.Combine(directory, "ArKrPlugFR-20200101.log");
                File.WriteAllText(stale, "old");
                File.SetLastWriteTime(stale, new DateTime(2020, 1, 1));

                var sink = new FileLogSink(directory, retentionDays: 3);
                sink.Write(new LogEntry(LogLevel.Information, "fresh", new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero)));

                Assert.False(File.Exists(stale));
                Assert.Single(Directory.GetFiles(directory, "ArKrPlugFR-*.log"));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void FileLogSink_RequiresDirectory(string directory)
        {
            Assert.Throws<ArgumentException>(() => new FileLogSink(directory));
        }

        [Fact]
        public void FileLogSink_RequiresPositiveRetention()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FileLogSink(Path.GetTempPath(), 0));
        }

        private sealed class ThrowingSink : ILogSink
        {
            public void Write(LogEntry entry)
            {
                throw new InvalidOperationException("sink is broken");
            }
        }
    }
}
