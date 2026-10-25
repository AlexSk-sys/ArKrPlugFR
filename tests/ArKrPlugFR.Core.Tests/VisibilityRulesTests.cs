using System;
using ArKrPlugFR.Core.Visibility;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class VisibilityRulesTests
    {
        [Theory]
        [InlineData(ModelViewKind.ThreeDimensional, true)]
        [InlineData(ModelViewKind.FloorPlan, true)]
        [InlineData(ModelViewKind.CeilingPlan, true)]
        [InlineData(ModelViewKind.Section, true)]
        [InlineData(ModelViewKind.Elevation, true)]
        [InlineData(ModelViewKind.Other, false)]
        public void IsModelView_OnlyAcceptsTheViewsAModelIsLookedAtIn(ModelViewKind kind, bool expected)
        {
            Assert.Equal(expected, VisibilityRules.IsModelView(kind));
        }

        [Fact]
        public void Decide_HidingSkipsWhatIsHiddenAndRefusesWhatCannotBeHidden()
        {
            Assert.Equal(
                VisibilityDecision.Change,
                VisibilityRules.Decide(VisibilityAction.Hide, false, true));

            Assert.Equal(
                VisibilityDecision.AlreadyInPlace,
                VisibilityRules.Decide(VisibilityAction.Hide, true, true));

            Assert.Equal(
                VisibilityDecision.CannotChange,
                VisibilityRules.Decide(VisibilityAction.Hide, false, false));
        }

        [Fact]
        public void Decide_ShowingIgnoresWhetherAnElementCouldBeHidden()
        {
            Assert.Equal(
                VisibilityDecision.Change,
                VisibilityRules.Decide(VisibilityAction.Show, true, false));

            Assert.Equal(
                VisibilityDecision.AlreadyInPlace,
                VisibilityRules.Decide(VisibilityAction.Show, false, false));
        }

        [Fact]
        public void Opposite_TurnsTheActionAround()
        {
            Assert.Equal(VisibilityAction.Show, VisibilityRules.Opposite(VisibilityAction.Hide));
            Assert.Equal(VisibilityAction.Hide, VisibilityRules.Opposite(VisibilityAction.Show));
        }
    }
}
