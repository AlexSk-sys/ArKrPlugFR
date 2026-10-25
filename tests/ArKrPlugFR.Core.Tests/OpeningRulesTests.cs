using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class OpeningRulesTests
    {
        [Fact]
        public void InWallFrame_ReadsACrossingAlongAStraightWall()
        {
            var crossing = new BoundingBox(
                new Point3(100d, 200d, 1000d),
                new Point3(500d, 300d, 1200d));

            OpeningRectangle rectangle = OpeningRules.InWallFrame(
                crossing,
                new Point3(0d, 0d, 0d),
                new Point3(1d, 0d, 0d));

            Assert.Equal(100d, rectangle.AlongMin);
            Assert.Equal(500d, rectangle.AlongMax);
            Assert.Equal(1000d, rectangle.HeightMin);
            Assert.Equal(1200d, rectangle.HeightMax);
            Assert.Equal(400d, rectangle.Width.Millimeters, 3);
            Assert.Equal(200d, rectangle.Height.Millimeters, 3);
        }

        [Fact]
        public void InWallFrame_MeasuresAlongTheWallHoweverItIsTurned()
        {
            var crossing = new BoundingBox(
                new Point3(100d, 200d, 0d),
                new Point3(500d, 300d, 200d));

            // The same box read against a wall running up the Y axis: what was the
            // depth of the box is now its width along the wall.
            OpeningRectangle rectangle = OpeningRules.InWallFrame(
                crossing,
                new Point3(0d, 0d, 0d),
                new Point3(0d, 1d, 0d));

            Assert.Equal(200d, rectangle.AlongMin);
            Assert.Equal(300d, rectangle.AlongMax);
            Assert.Equal(100d, rectangle.Width.Millimeters, 3);
        }

        [Fact]
        public void InWallFrame_GrowsWhenTheServiceMeetsTheWallAtAnAngle()
        {
            var crossing = new BoundingBox(
                new Point3(0d, 0d, 0d),
                new Point3(100d, 100d, 100d));

            // A square crossing read against a wall running at forty-five degrees: the
            // opening has to cover the diagonal the service really covers.
            OpeningRectangle rectangle = OpeningRules.InWallFrame(
                crossing,
                new Point3(0d, 0d, 0d),
                new Point3(1d, 1d, 0d));

            Assert.Equal(0d, rectangle.AlongMin, 3);
            Assert.Equal(Math.Sqrt(20000d), rectangle.AlongMax, 3);
            Assert.True(rectangle.Width.Millimeters > 141d);
        }

        [Fact]
        public void InWallFrame_ReadsADirectionThatIsNotAUnitVector()
        {
            var crossing = new BoundingBox(
                new Point3(0d, 0d, 0d),
                new Point3(300d, 400d, 0d));

            OpeningRectangle rectangle = OpeningRules.InWallFrame(
                crossing,
                new Point3(0d, 0d, 0d),
                new Point3(3d, 4d, 0d));

            // The direction is normalised, so the box reaches exactly its own length
            // along the wall.
            Assert.Equal(500d, rectangle.AlongMax, 3);
        }

        [Fact]
        public void InWallFrame_MeasuresHeightAboveTheBasePoint()
        {
            var crossing = new BoundingBox(
                new Point3(0d, 0d, 3000d),
                new Point3(400d, 100d, 3200d));

            // The wall base stands at 1900 mm, so the crossing is read 1100 mm above it.
            OpeningRectangle rectangle = OpeningRules.InWallFrame(
                crossing,
                new Point3(0d, 0d, 1900d),
                new Point3(1d, 0d, 0d));

            Assert.Equal(1100d, rectangle.HeightMin, 3);
            Assert.Equal(1300d, rectangle.HeightMax, 3);
        }

        [Fact]
        public void InWallFrame_RejectsADirectionWithoutLength()
        {
            var crossing = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(10d, 10d, 10d));

            Assert.Throws<ArgumentException>(() =>
                OpeningRules.InWallFrame(crossing, new Point3(0d, 0d, 0d), new Point3(0d, 0d, 0d)));
        }

        [Fact]
        public void WithClearance_GrowsTheCrossingOnEverySide()
        {
            var crossing = new OpeningRectangle(100d, 500d, 1000d, 1200d);

            OpeningRectangle opening = OpeningRules.WithClearance(crossing, OpeningRules.DefaultClearance);

            Assert.Equal(50d, opening.AlongMin, 3);
            Assert.Equal(550d, opening.AlongMax, 3);
            Assert.Equal(950d, opening.HeightMin, 3);
            Assert.Equal(1250d, opening.HeightMax, 3);

            // Five hundred by three hundred: the crossing of four hundred by two hundred
            // plus fifty millimetres on each of the four sides.
            Assert.Equal(500d, opening.Width.Millimeters, 3);
            Assert.Equal(300d, opening.Height.Millimeters, 3);
        }

        [Fact]
        public void WithClearance_ReadsANegativeClearanceAsNone()
        {
            var crossing = new OpeningRectangle(0d, 100d, 0d, 100d);

            OpeningRectangle opening = OpeningRules.WithClearance(crossing, Length.FromMillimeters(-40d));

            Assert.Equal(100d, opening.Width.Millimeters, 3);
        }

        [Fact]
        public void IsUsable_NeedsBothEdges()
        {
            Assert.False(OpeningRules.IsUsable(new OpeningRectangle(0d, 400d, 0d, 5d), OpeningRules.MinimumEdge));
            Assert.False(OpeningRules.IsUsable(new OpeningRectangle(0d, 0d, 0d, 0d), OpeningRules.MinimumEdge));
            Assert.True(OpeningRules.IsUsable(new OpeningRectangle(0d, 400d, 0d, 200d), OpeningRules.MinimumEdge));
        }

        [Fact]
        public void Defaults_MatchTheSpecification()
        {
            Assert.Equal(50d, OpeningRules.DefaultClearance.Millimeters, 3);
            Assert.Equal(50d, OpeningRules.DefaultClearanceMillimeters, 3);
        }
    }
}
