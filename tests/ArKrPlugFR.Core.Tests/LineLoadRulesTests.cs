using ArKrPlugFR.Core.Loads;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class LineLoadRulesTests
    {
        [Fact]
        public void IsUsable_RefusesALoadWithNoForceAndNoMoment()
        {
            Assert.False(LineLoadRules.IsUsable(0, 0));
            Assert.True(LineLoadRules.IsUsable(1.5, 0));
            Assert.True(LineLoadRules.IsUsable(0, 0.4));
            Assert.True(LineLoadRules.IsUsable(-1.5, 0));
        }

        [Fact]
        public void Force_PointsDownForAPositiveValue()
        {
            Assert.Equal(new LoadVector(0, 0, -1.5), LineLoadRules.Force(1.5));
            Assert.Equal(new LoadVector(0, 0, 1.5), LineLoadRules.Force(-1.5));
        }

        [Fact]
        public void Moment_KeepsItsSignOnTheVerticalAxis()
        {
            Assert.Equal(new LoadVector(0, 0, 0.4), LineLoadRules.Moment(0.4));
        }

        [Fact]
        public void EndVectors_RepeatForAUniformLoadAndTaperToZeroOtherwise()
        {
            Assert.Equal(new LoadVector(0, 0, -1.5), LineLoadRules.EndForce(1.5, true));
            Assert.Equal(new LoadVector(0, 0, 0), LineLoadRules.EndForce(1.5, false));
            Assert.Equal(new LoadVector(0, 0, 0.4), LineLoadRules.EndMoment(0.4, true));
            Assert.Equal(new LoadVector(0, 0, 0), LineLoadRules.EndMoment(0.4, false));
        }
    }
}
