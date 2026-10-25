using System;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class LengthTests
    {
        [Fact]
        public void FromFeet_ConvertsToMillimeters()
        {
            Length length = Length.FromFeet(1d);

            Assert.Equal(304.8d, length.Millimeters, 9);
        }

        [Fact]
        public void FromMillimeters_RoundTripsThroughFeet()
        {
            Length original = Length.FromMillimeters(150d);

            Assert.Equal(150d, Length.FromFeet(original.Feet).Millimeters, 9);
        }

        [Fact]
        public void FromMeters_ConvertsToInches()
        {
            Length length = Length.FromMeters(1d);

            Assert.Equal(39.37007874d, length.Inches, 6);
        }

        [Fact]
        public void Operators_ComposeLengths()
        {
            Length sum = Length.FromMillimeters(1000d) + Length.FromMillimeters(500d);
            Length difference = sum - Length.FromMillimeters(200d);

            Assert.Equal(1300d, difference.Millimeters, 9);
            Assert.True(Length.FromMillimeters(-5d) < Length.Zero);
            Assert.Equal(5d, Length.Abs(Length.FromMillimeters(-5d)).Millimeters, 9);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void FromMillimeters_RejectsNonFiniteValues(double value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Length.FromMillimeters(value));
        }

        [Fact]
        public void Equity_HoldsForEqualLengths()
        {
            Assert.True(Length.FromMillimeters(10d) == Length.FromMeters(0.01d));
            Assert.False(Length.FromMillimeters(10d) == Length.FromMillimeters(10.001d));
        }
    }
}
