using System;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class BoundingBoxTests
    {
        private static BoundingBox Box(double size = 1000d)
        {
            return new BoundingBox(new Point3(0d, 0d, 0d), new Point3(size, size, size));
        }

        [Fact]
        public void Constructor_OrdersCornersRegardlessOfInputOrder()
        {
            var box = new BoundingBox(new Point3(300d, 200d, 100d), new Point3(0d, 0d, 0d));

            Assert.Equal(new Point3(0d, 0d, 0d), box.Min);
            Assert.Equal(new Point3(300d, 200d, 100d), box.Max);
        }

        [Fact]
        public void Size_ReportsEachAxisInMillimetres()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(300d, 400d, 500d));

            Assert.Equal(300d, box.Width.Millimeters);
            Assert.Equal(400d, box.Depth.Millimeters);
            Assert.Equal(500d, box.Height.Millimeters);
            Assert.Equal(60_000_000d, box.Volume);
        }

        [Fact]
        public void Center_IsHalfwayOnEveryAxis()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(200d, 400d, 600d));

            Assert.Equal(new Point3(100d, 200d, 300d), box.Center);
        }

        [Fact]
        public void Inflate_GrowsEverySide()
        {
            BoundingBox inflated = Box().Inflate(Length.FromMillimeters(50d));

            Assert.Equal(new Point3(-50d, -50d, -50d), inflated.Min);
            Assert.Equal(new Point3(1050d, 1050d, 1050d), inflated.Max);
        }

        [Fact]
        public void Inflate_RejectsNegativeAmount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Box().Inflate(Length.FromMillimeters(-1d)));
        }

        [Fact]
        public void Intersects_IsTrueForOverlappingBoxes()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(50d, 50d, 50d), new Point3(200d, 200d, 200d));

            Assert.True(left.Intersects(right));
            Assert.True(right.Intersects(left));
        }

        [Fact]
        public void Intersects_IsTrueWhenBoxesOnlyTouch()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(100d, 0d, 0d), new Point3(200d, 100d, 100d));

            Assert.True(left.Intersects(right));
        }

        [Fact]
        public void Intersects_IsFalseForBoxesApartOnOneAxis()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(101d, 0d, 0d), new Point3(200d, 100d, 100d));

            Assert.False(left.Intersects(right));
        }

        [Fact]
        public void Intersects_UsesToleranceToBridgeAGap()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(140d, 0d, 0d), new Point3(200d, 100d, 100d));

            Assert.False(left.Intersects(right));
            Assert.True(left.Intersects(right, Length.FromMillimeters(40d)));
        }

        [Fact]
        public void Intersects_RejectsNegativeTolerance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Box().Intersects(Box(), Length.FromMillimeters(-1d)));
        }

        [Fact]
        public void Contains_ChecksEveryAxis()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));

            Assert.True(box.Contains(new Point3(50d, 50d, 50d)));
            Assert.True(box.Contains(new Point3(0d, 0d, 0d)));
            Assert.False(box.Contains(new Point3(101d, 50d, 50d)));
        }

        [Fact]
        public void Intersect_ReturnsThePartTwoBoxesShare()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(50d, 0d, 0d), new Point3(200d, 150d, 100d));

            BoundingBox? shared = left.Intersect(right);

            Assert.NotNull(shared);
            Assert.Equal(new Point3(50d, 0d, 0d), shared!.Value.Min);
            Assert.Equal(new Point3(100d, 100d, 100d), shared.Value.Max);
        }

        [Fact]
        public void Intersect_IsNullForBoxesApartOnOneAxis()
        {
            var left = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));
            var right = new BoundingBox(new Point3(101d, 0d, 0d), new Point3(200d, 100d, 100d));
            var touching = new BoundingBox(new Point3(100d, 0d, 0d), new Point3(200d, 100d, 100d));

            Assert.Null(left.Intersect(right));

            // A shared face holds no volume, so it is not a crossing either.
            Assert.Null(left.Intersect(touching));
        }

        [Fact]
        public void Equality_ComparesBothCorners()
        {
            Assert.Equal(Box(), Box());
            Assert.True(Box() == Box());
            Assert.NotEqual(Box(1000d), Box(2000d));
        }

        [Fact]
        public void Point3_DistanceIsEuclidean()
        {
            var origin = new Point3(0d, 0d, 0d);
            var point = new Point3(300d, 400d, 0d);

            Assert.Equal(500d, origin.DistanceTo(point), 6);
        }

        [Fact]
        public void Point3_RejectsNonFiniteCoordinates()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point3(double.NaN, 0d, 0d));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point3(0d, double.PositiveInfinity, 0d));
        }
    }
}
