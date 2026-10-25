using System;
using ArKrPlugFR.Core.Alignment;
using ArKrPlugFR.Core.Geometry;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class AlignmentRulesTests
    {
        [Fact]
        public void VerticalOffset_MeasuresAtTheCrossingOfTheTwoLines()
        {
            // Two lines crossing at their middles: the target at 3000, the element at 1000.
            var target = new Segment3(new Point3(0, 0, 3000), new Point3(10000, 0, 3000));
            var current = new Segment3(new Point3(5000, -5000, 1000), new Point3(5000, 5000, 1000));

            Assert.Equal(2000, AlignmentRules.VerticalOffset(target, current), 6);
        }

        [Fact]
        public void VerticalOffset_MeasuresAlongTheSlopeOfBothLines()
        {
            // The target rises from 0 to 10000 over its length; the crossing sits at its middle.
            var target = new Segment3(new Point3(0, 0, 0), new Point3(10000, 0, 10000));
            var current = new Segment3(new Point3(5000, -5000, 1000), new Point3(5000, 5000, 1000));

            Assert.Equal(4000, AlignmentRules.VerticalOffset(target, current), 6);
        }

        [Fact]
        public void VerticalOffset_FallsBackToTheMidpointsWhenTheLinesDoNotCross()
        {
            // Parallel lines: the midpoints are 1000 and 2500 high.
            var target = new Segment3(new Point3(0, 0, 1000), new Point3(10000, 0, 1000));
            var current = new Segment3(new Point3(0, 5000, 2500), new Point3(10000, 5000, 2500));

            Assert.Equal(-1500, AlignmentRules.VerticalOffset(target, current), 6);

            // Lines that would cross only beyond their ends.
            var shortTarget = new Segment3(new Point3(0, 0, 1000), new Point3(1000, 0, 1000));
            var shortCurrent = new Segment3(new Point3(5000, -5000, 2000), new Point3(5000, 5000, 2000));

            Assert.Equal(-1000, AlignmentRules.VerticalOffset(shortTarget, shortCurrent), 6);
        }

        [Fact]
        public void VerticalOffset_OfAVerticalLineUsesTheMidpoints()
        {
            var target = new Segment3(new Point3(0, 0, 5000), new Point3(0, 0, 9000));
            var current = new Segment3(new Point3(0, 0, 1000), new Point3(0, 0, 3000));

            Assert.Equal(5000, AlignmentRules.VerticalOffset(target, current), 6);
        }

        [Fact]
        public void ProjectOntoLine_ReturnsTheNearestPointOfTheLine()
        {
            var line = new Segment3(new Point3(0, 0, 0), new Point3(10000, 0, 0));

            Point3 projected = AlignmentRules.ProjectOntoLine(new Point3(3000, 2000, 500), line);

            Assert.Equal(3000, projected.X, 6);
            Assert.Equal(0, projected.Y, 6);
            Assert.Equal(0, projected.Z, 6);
        }

        [Fact]
        public void ProjectOntoLine_ClampsToTheEndsOfTheLine()
        {
            var line = new Segment3(new Point3(0, 0, 0), new Point3(10000, 0, 0));

            Assert.Equal(0, AlignmentRules.ProjectOntoLine(new Point3(-5000, 100, 0), line).X, 6);
            Assert.Equal(10000, AlignmentRules.ProjectOntoLine(new Point3(15000, 100, 0), line).X, 6);
        }

        [Fact]
        public void ProjectOntoLine_OfADegenerateLineReturnsItsStart()
        {
            var line = new Segment3(new Point3(100, 200, 300), new Point3(100, 200, 300));

            Assert.Equal(new Point3(100, 200, 300), AlignmentRules.ProjectOntoLine(new Point3(0, 0, 0), line));
        }

        [Fact]
        public void HasEndpointAt_FindsBothEndsWithinTheTolerance()
        {
            var segment = new Segment3(new Point3(0, 0, 0), new Point3(1000, 0, 0));

            Assert.True(AlignmentRules.HasEndpointAt(segment, new Point3(0.4, 0, 0), 1));
            Assert.True(AlignmentRules.HasEndpointAt(segment, new Point3(1000, 0, 0), 1));
            Assert.False(AlignmentRules.HasEndpointAt(segment, new Point3(500, 0, 0), 1));
        }

        [Fact]
        public void HasEndpointAt_RejectsANegativeTolerance()
        {
            var segment = new Segment3(new Point3(0, 0, 0), new Point3(1000, 0, 0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => AlignmentRules.HasEndpointAt(segment, new Point3(0, 0, 0), -1));
        }
    }
}
