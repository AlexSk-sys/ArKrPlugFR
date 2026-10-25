using ArKrPlugFR.Core.RebarMarks;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class FillRulesTests
    {
        [Fact]
        public void ShouldFill_FillsAnEmptyBar()
        {
            Assert.True(FillRules.ShouldFill("Км-1", string.Empty, onlyEmpty: true));
        }

        [Fact]
        public void ShouldFill_KeepsABarWithAValueWhenAsked()
        {
            Assert.False(FillRules.ShouldFill("Км-1", "Км-2", onlyEmpty: true));
        }

        [Fact]
        public void ShouldFill_OverwritesWhenTheUserAsks()
        {
            Assert.True(FillRules.ShouldFill("Км-1", "Км-2", onlyEmpty: false));
        }

        [Fact]
        public void ShouldFill_WithAnEmptySource_FillsNothing()
        {
            Assert.False(FillRules.ShouldFill(string.Empty, string.Empty, onlyEmpty: false));
            Assert.False(FillRules.ShouldFill("   ", "Км-2", onlyEmpty: false));
            Assert.False(FillRules.ShouldFill(null, string.Empty, onlyEmpty: false));
        }
    }
}
