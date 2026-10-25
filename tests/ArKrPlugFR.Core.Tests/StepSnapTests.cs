using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class StepSnapTests
    {
        private static SnapSettings Settings(double stepMm = 100d, double toleranceMm = 30d, double? originMm = null)
        {
            return new SnapSettings(
                Length.FromMillimeters(stepMm),
                Length.FromMillimeters(toleranceMm),
                originMm.HasValue ? Length.FromMillimeters(originMm.Value) : (Length?)null);
        }

        [Fact]
        public void Snap_MovesValueInsideTolerance()
        {
            Assert.Equal(2400d, StepSnap.Snap(2420d, Settings()), 9);
        }

        [Fact]
        public void Snap_KeepsValueOutsideTolerance()
        {
            Assert.Equal(2250d, StepSnap.Snap(2250d, Settings()), 9);
        }

        [Fact]
        public void Snap_HandlesNegativeValues()
        {
            Assert.Equal(-1100d, StepSnap.Snap(-1130d, Settings()), 9);
            Assert.Equal(-1140d, StepSnap.Snap(-1140d, Settings()), 9);
        }

        [Fact]
        public void Snap_IsIdempotent()
        {
            SnapSettings settings = Settings();
            double once = StepSnap.Snap(1230d, settings);

            Assert.Equal(1200d, once, 9);
            Assert.Equal(once, StepSnap.Snap(once, settings), 9);
        }

        [Fact]
        public void Snap_WithOrigin_ShiftsTheGrid()
        {
            Assert.Equal(2450d, StepSnap.Snap(2470d, Settings(originMm: 50d)), 9);
        }

        [Fact]
        public void Snap_WithOrigin_KeepsFreeValue()
        {
            // The grid runs through 50, 150, ... 2450; 2410 is 40 mm away from
            // the nearest grid line, which exceeds the 30 mm tolerance.
            Assert.Equal(2410d, StepSnap.Snap(2410d, Settings(originMm: 50d)), 9);
        }

        [Fact]
        public void IsOnGrid_IsTrueOnlyForExactGridValues()
        {
            SnapSettings settings = Settings();

            Assert.True(StepSnap.IsOnGrid(2400d, settings));
            Assert.False(StepSnap.IsOnGrid(2420d, settings));
            Assert.False(StepSnap.IsOnGrid(2250d, settings));
        }

        [Fact]
        public void IsOnGrid_RespectsOrigin()
        {
            SnapSettings settings = Settings(originMm: 50d);

            Assert.True(StepSnap.IsOnGrid(2450d, settings));
            Assert.False(StepSnap.IsOnGrid(2400d, settings));
        }

        [Fact]
        public void Constructor_RejectsNonPositiveStep()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new SnapSettings(Length.Zero, Length.FromMillimeters(10d)));
        }

        [Fact]
        public void Constructor_RejectsNegativeTolerance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new SnapSettings(Length.FromMillimeters(100d), Length.FromMillimeters(-1d)));
        }

        [Fact]
        public void Snap_RejectsNullSettings()
        {
            Assert.Throws<ArgumentNullException>(() => StepSnap.Snap(1d, null!));
        }
    }
}
