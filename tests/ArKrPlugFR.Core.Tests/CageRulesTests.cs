using System;
using ArKrPlugFR.Core.Cages;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class CageRulesTests
    {
        [Fact]
        public void Positions_CoversTheLengthAndAddsTheBarAtTheStart()
        {
            // 10 m at 2 m: five whole steps cover the length, plus the bar at the start.
            Assert.Equal(6, CageRules.Positions(10, 2));

            // A length that is not a whole number of steps rounds up.
            Assert.Equal(6, CageRules.Positions(10, 2.1));
            Assert.Equal(7, CageRules.Positions(10, 1.9));
        }

        [Fact]
        public void Positions_ReturnsZeroForAnUnusableStepOrLength()
        {
            Assert.Equal(0, CageRules.Positions(10, 0));
            Assert.Equal(0, CageRules.Positions(10, -1));
            Assert.Equal(0, CageRules.Positions(-1, 1));
        }

        [Fact]
        public void Positions_OfAZeroLengthIsOne()
        {
            Assert.Equal(1, CageRules.Positions(0, 1));
        }

        [Fact]
        public void LongitudinalMetres_MultipliesTheLengthByTheBars()
        {
            Assert.Equal(30, CageRules.LongitudinalMetres(10, 3), 6);
            Assert.Equal(0, CageRules.LongitudinalMetres(10, 0), 6);
            Assert.Equal(0, CageRules.LongitudinalMetres(0, 3), 6);
            Assert.Equal(0, CageRules.LongitudinalMetres(10, -2), 6);
        }

        [Fact]
        public void InclinedMetres_CountsTwoBarsPerPosition()
        {
            Assert.Equal(24, CageRules.InclinedMetres(6, 2), 6);
            Assert.Equal(0, CageRules.InclinedMetres(0, 2), 6);
            Assert.Equal(0, CageRules.InclinedMetres(6, 0), 6);
        }

        [Fact]
        public void TransverseMetres_AddsEveryDistinctLengthOncePerPosition()
        {
            Assert.Equal(21, CageRules.TransverseMetres(3, new[] { 4.0, 2.0, 1.0 }), 6);

            // A length that repeats in the list is not the caller's job to deduplicate,
            // but an unusable one is left out.
            Assert.Equal(6, CageRules.TransverseMetres(3, new[] { 2.0, 0, -5 }), 6);
            Assert.Equal(0, CageRules.TransverseMetres(0, new[] { 2.0 }), 6);
        }

        [Fact]
        public void TransverseMetres_RejectsANullSequence()
        {
            Assert.Throws<ArgumentNullException>(() => CageRules.TransverseMetres(1, null!));
        }

        [Fact]
        public void Round_KeepsTwoDecimalsAndRoundsHalfAwayFromZero()
        {
            Assert.Equal(12.35, CageRules.Round(12.345), 6);
            Assert.Equal(12.34, CageRules.Round(12.3449), 6);
            Assert.Equal(0.01, CageRules.Round(0.005), 6);
        }
    }
}
