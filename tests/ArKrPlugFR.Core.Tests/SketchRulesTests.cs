using ArKrPlugFR.Core.Sketches;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SketchRulesTests
    {
        [Fact]
        public void Plan_CapsTheRunAtTheLimit()
        {
            Assert.Equal(3, SketchRules.Plan(3, 25));
            Assert.Equal(25, SketchRules.Plan(40, 25));
            Assert.Equal(25, SketchRules.Plan(25, 25));
        }

        [Fact]
        public void Plan_OfNothingIsNothing()
        {
            Assert.Equal(0, SketchRules.Plan(0, 25));
            Assert.Equal(0, SketchRules.Plan(-4, 25));
            Assert.Equal(0, SketchRules.Plan(5, 0));
        }

        [Fact]
        public void ViewName_NamesTheShapeAndItsPlaceInTheRun()
        {
            Assert.Equal("3. A500C-21", SketchRules.ViewName(" A500C-21 ", 3));
            Assert.Equal("1. Bar", SketchRules.ViewName(null, 1));
            Assert.Equal("2. Bar", SketchRules.ViewName("   ", 2));
        }

        [Fact]
        public void Millimeters_KeepsTheNumberReadable()
        {
            Assert.Equal("12", SketchRules.Millimeters(12));
            Assert.Equal("12.5", SketchRules.Millimeters(12.5));
            Assert.Equal("1200", SketchRules.Millimeters(1200));
            Assert.Equal("0.001", SketchRules.Millimeters(0.001));
        }
    }
}
