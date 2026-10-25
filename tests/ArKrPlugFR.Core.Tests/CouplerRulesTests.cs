using System.Collections.Generic;
using ArKrPlugFR.Core.Couplers;
using ArKrPlugFR.Core.Geometry;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class CouplerRulesTests
    {
        [Fact]
        public void DistanceTo_APointOnTheSegment_IsZero()
        {
            var segment = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));

            Assert.Equal(0d, segment.DistanceTo(new Point3(40d, 0d, 0d)), 6);
        }

        [Fact]
        public void DistanceTo_APointBesideTheSegment_IsThePerpendicularDistance()
        {
            var segment = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));

            Assert.Equal(30d, segment.DistanceTo(new Point3(40d, 30d, 0d)), 6);
        }

        [Fact]
        public void DistanceTo_APointPastAnEnd_IsTheDistanceToThatEnd()
        {
            var segment = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));

            Assert.Equal(50d, segment.DistanceTo(new Point3(150d, 0d, 0d)), 6);
        }

        [Fact]
        public void DistanceTo_ASegmentWithoutLength_IsTheDistanceToItsPoint()
        {
            var segment = new Segment3(new Point3(10d, 10d, 10d), new Point3(10d, 10d, 10d));

            Assert.Equal(5d, segment.DistanceTo(new Point3(15d, 10d, 10d)), 6);
        }

        [Fact]
        public void Length_OfASegment_IsTheDistanceBetweenItsEnds()
        {
            Assert.Equal(5d, new Segment3(new Point3(0d, 0d, 0d), new Point3(3d, 4d, 0d)).Length, 6);
        }

        [Fact]
        public void FindNearest_PicksTheCentreClosestToTheAxis()
        {
            var axis = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));
            var centres = new List<Point3>
            {
                new Point3(40d, 300d, 0d),
                new Point3(50d, 20d, 0d),
                new Point3(60d, 100d, 0d),
            };

            Assert.Equal(1, CouplerRules.FindNearest(axis, centres, 200d));
        }

        [Fact]
        public void FindNearest_IgnoresCentresFartherThanTheTolerance()
        {
            var axis = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));
            var centres = new List<Point3> { new Point3(40d, 250d, 0d) };

            Assert.Equal(-1, CouplerRules.FindNearest(axis, centres, 200d));
        }

        [Fact]
        public void FindNearest_KeepsTheFirstOfTwoEquals()
        {
            var axis = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));
            var centres = new List<Point3>
            {
                new Point3(20d, 50d, 0d),
                new Point3(80d, 50d, 0d),
            };

            Assert.Equal(0, CouplerRules.FindNearest(axis, centres, 200d));
        }

        [Fact]
        public void FindNearest_WithNoCentres_ReturnsMinusOne()
        {
            var axis = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));

            Assert.Equal(-1, CouplerRules.FindNearest(axis, new List<Point3>(), 200d));
        }

        [Fact]
        public void FindNearest_CountsACentreAtTheExactTolerance()
        {
            var axis = new Segment3(new Point3(0d, 0d, 0d), new Point3(100d, 0d, 0d));
            var centres = new List<Point3> { new Point3(50d, 200d, 0d) };

            Assert.Equal(0, CouplerRules.FindNearest(axis, centres, 200d));
        }

        [Fact]
        public void Tolerance_IsUsableWithinItsRange()
        {
            Assert.True(CouplerRules.IsUsableTolerance(1d));
            Assert.True(CouplerRules.IsUsableTolerance(2000d));
            Assert.False(CouplerRules.IsUsableTolerance(0d));
            Assert.False(CouplerRules.IsUsableTolerance(2001d));
        }

        [Fact]
        public void Settings_StartWithTheDefaultToleranceAndParameterNames()
        {
            var settings = new CouplerSettings();

            Assert.Equal(CouplerRules.DefaultToleranceMillimeters, settings.ToleranceMillimeters);
            Assert.Equal(CouplerSettings.DefaultMarkParameter, settings.MarkParameter);
            Assert.Equal(CouplerSettings.DefaultCategoryParameter, settings.CategoryParameter);
        }
    }
}
