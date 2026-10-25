using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Marks;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class MarkTallyTests
    {
        [Fact]
        public void Count_GroupsEqualMarksRegardlessOfCaseAndSpaces()
        {
            IReadOnlyDictionary<string, int> counts = MarkTally.Count(
                new[] { "КМ-1", " км-1 ", "Км-1", "КМ-2" });

            Assert.Equal(3, MarkTally.Lookup(counts, "км-1"));
            Assert.Equal(1, MarkTally.Lookup(counts, " КМ-2 "));
            Assert.Equal(2, counts.Count);
        }

        [Fact]
        public void Count_LeavesOutEmptyMarks()
        {
            IReadOnlyDictionary<string, int> counts = MarkTally.Count(
                new string?[] { null, "", "   ", "КЖ-3" });

            Assert.Single(counts);
            Assert.Equal(1, MarkTally.Lookup(counts, "КЖ-3"));
        }

        [Fact]
        public void Lookup_ReturnsZeroForAnUnknownOrEmptyMark()
        {
            IReadOnlyDictionary<string, int> counts = MarkTally.Count(new[] { "КМ-1" });

            Assert.Equal(0, MarkTally.Lookup(counts, "КМ-9"));
            Assert.Equal(0, MarkTally.Lookup(counts, null));
            Assert.Equal(0, MarkTally.Lookup(counts, "  "));
        }

        [Fact]
        public void TryKey_TrimsAndRejectsWhatIsLeftEmpty()
        {
            Assert.True(MarkTally.TryKey("  кж-7 ", out string key));
            Assert.Equal("кж-7", key);

            Assert.False(MarkTally.TryKey("   ", out string empty));
            Assert.Equal(string.Empty, empty);
        }

        [Fact]
        public void Count_RejectsANullSequence()
        {
            Assert.Throws<ArgumentNullException>(() => MarkTally.Count(null!));
        }
    }
}
