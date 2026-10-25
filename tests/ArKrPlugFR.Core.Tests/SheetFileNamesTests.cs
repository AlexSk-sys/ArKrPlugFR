using System;
using System.Linq;
using ArKrPlugFR.Core.Export;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SheetFileNamesTests
    {
        [Fact]
        public void ForSheet_JoinsTheNumberAndTheNameWithTheSeparator()
        {
            Assert.Equal("АР-101 - План этажа", SheetFileNames.ForSheet("АР-101", "План этажа"));
        }

        [Fact]
        public void ForSheet_HandlesAMissingNumberOrName()
        {
            Assert.Equal("АР-101", SheetFileNames.ForSheet("АР-101", "  "));
            Assert.Equal("План этажа", SheetFileNames.ForSheet(null, "План этажа"));
            Assert.Equal(SheetFileNames.Fallback, SheetFileNames.ForSheet(null, null));
            Assert.Equal(SheetFileNames.Fallback, SheetFileNames.ForSheet("   ", ""));
        }

        [Fact]
        public void Sanitize_ReplacesWhatAFileNameCannotHold()
        {
            string clean = SheetFileNames.Sanitize("План 1:100 * (финал)");

            Assert.DoesNotContain(":", clean);
            Assert.DoesNotContain("*", clean);
            Assert.Equal("План 1-100 - (финал)", clean);
        }

        [Fact]
        public void Sanitize_CollapsesSpacesAndTrimsTheEnds()
        {
            Assert.Equal("План этажа", SheetFileNames.Sanitize("  План   этажа  "));
            Assert.Equal("План", SheetFileNames.Sanitize("...План..."));
        }

        [Fact]
        public void Sanitize_ReturnsEmptyForNothingUsable()
        {
            Assert.Equal(string.Empty, SheetFileNames.Sanitize(null));
            Assert.Equal(string.Empty, SheetFileNames.Sanitize("   "));
        }

        [Fact]
        public void ForSheet_CapsALongName()
        {
            string name = new string('А', 200);
            string produced = SheetFileNames.ForSheet("1", name);

            Assert.True(produced.Length <= SheetFileNames.MaximumLength);
        }

        [Fact]
        public void Duplicates_FindsWhatWouldOverwriteWhat()
        {
            var names = new[] { "АР-1 - План", "АР-2 - Разрез", "ар-1 - план", "АР-2 - Разрез" };

            var duplicates = SheetFileNames.Duplicates(names);

            Assert.Equal(2, duplicates.Count);
            Assert.Contains("ар-1 - план", duplicates);
            Assert.Contains("АР-2 - Разрез", duplicates);
        }

        [Fact]
        public void Duplicates_OfDistinctNamesIsEmpty()
        {
            Assert.Empty(SheetFileNames.Duplicates(new[] { "А", "Б", "В" }));
        }

        [Fact]
        public void Duplicates_RejectsANullSequence()
        {
            Assert.Throws<ArgumentNullException>(() => SheetFileNames.Duplicates(null!));
        }
    }
}
