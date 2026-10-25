using System;
using ArKrPlugFR.Core.Duplication;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ParameterValueNormalizerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Normalize_TreatsMissingValueAsEmpty(string? value)
        {
            Assert.Equal(string.Empty, ParameterValueNormalizer.Normalize(value));
        }

        [Fact]
        public void Normalize_TrimsAndCollapsesWhitespace()
        {
            Assert.Equal("600 X 400", ParameterValueNormalizer.Normalize("  600 \t  X   400 "));
        }

        [Fact]
        public void Normalize_IgnoresInsignificantTrailingZeros()
        {
            Assert.Equal(ParameterValueNormalizer.Normalize("1.0"), ParameterValueNormalizer.Normalize("1"));
            Assert.Equal(ParameterValueNormalizer.Normalize("1.500"), ParameterValueNormalizer.Normalize("1.5"));
        }

        [Fact]
        public void Normalize_ReadsADecimalComma()
        {
            Assert.Equal(ParameterValueNormalizer.Normalize("1.5"), ParameterValueNormalizer.Normalize("1,5"));
        }

        [Fact]
        public void Normalize_ReadsACommaAsTheDecimalSeparator()
        {
            // A single comma with no dot is read as a decimal comma, the convention
            // in Russian-language projects. The same text can also mean a thousands
            // separator; that ambiguity is resolved in favour of the decimal point
            // and is the documented behaviour of the tool.
            Assert.Equal(ParameterValueNormalizer.Normalize("1.234"), ParameterValueNormalizer.Normalize("1,234"));
        }

        [Fact]
        public void Normalize_LeavesGroupedDigitsAlone()
        {
            // Spaces inside a number cannot be read as a number without guessing, so
            // such a value is compared as text and stays distinct from "1234".
            Assert.NotEqual(ParameterValueNormalizer.Normalize("1 234"), ParameterValueNormalizer.Normalize("1234"));
        }

        [Fact]
        public void Normalize_ComparesTextCaseInsensitively()
        {
            Assert.Equal(ParameterValueNormalizer.Normalize("Concrete"), ParameterValueNormalizer.Normalize("concrete"));
        }

        [Fact]
        public void Normalize_KeepsDifferentTextApart()
        {
            Assert.NotEqual(ParameterValueNormalizer.Normalize("Concrete"), ParameterValueNormalizer.Normalize("Masonry"));
        }

        [Fact]
        public void Normalize_RejectsAnOutOfRangePrecision()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ParameterValueNormalizer.Normalize("1", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ParameterValueNormalizer.Normalize("1", 18));
        }
    }
}
