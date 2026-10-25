using System;
using ArKrPlugFR.Core.Schedules;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ColumnVisibilityTests
    {
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("0", true)]
        [InlineData("0.00", true)]
        [InlineData("0,00", true)]
        [InlineData("-0", true)]
        [InlineData("0.001", false)]
        [InlineData("12", false)]
        [InlineData("А500С", false)]
        [InlineData("-3", false)]
        public void IsBlankOrZero_TellsNothingFromSomething(string? text, bool expected)
        {
            Assert.Equal(expected, ColumnVisibility.IsBlankOrZero(text));
        }

        [Fact]
        public void IsEmpty_IsTrueOnlyWhenEveryCellTellsNothing()
        {
            Assert.True(ColumnVisibility.IsEmpty(new string?[] { null, "", "0", "0.00" }));
            Assert.False(ColumnVisibility.IsEmpty(new string?[] { "", "0", "12" }));
            Assert.False(ColumnVisibility.IsEmpty(new string?[] { "А500С" }));
        }

        [Fact]
        public void IsEmpty_OfNoCellsIsFalse()
        {
            Assert.False(ColumnVisibility.IsEmpty(Array.Empty<string?>()));
        }

        [Fact]
        public void IsEmpty_RejectsANullSequence()
        {
            Assert.Throws<ArgumentNullException>(() => ColumnVisibility.IsEmpty(null!));
        }

        [Fact]
        public void Decide_HidesWhatTellsNothingAndShowsWhatCarriesValues()
        {
            Assert.Equal(ColumnAction.Hide, ColumnVisibility.Decide(isHidden: false, hasValues: false));
            Assert.Equal(ColumnAction.Leave, ColumnVisibility.Decide(isHidden: true, hasValues: false));
            Assert.Equal(ColumnAction.Show, ColumnVisibility.Decide(isHidden: true, hasValues: true));
            Assert.Equal(ColumnAction.Leave, ColumnVisibility.Decide(isHidden: false, hasValues: true));
        }
    }
}
