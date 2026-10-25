using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Alignment;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class AlignPlannerTests
    {
        private static readonly GridLine Vertical = new GridLine(
            new Point3(0d, 0d, 0d),
            new Point3(0d, 1d, 0d));

        private static readonly GridLine Horizontal = new GridLine(
            new Point3(0d, 0d, 0d),
            new Point3(1d, 0d, 0d));

        [Fact]
        public void Distance_MeasuresAcrossTheLine()
        {
            Assert.Equal(0d, GridSnap.Distance(new Point3(0d, 500d, 0d), Vertical), 6);
            Assert.Equal(3d, GridSnap.Distance(new Point3(3d, 500d, 0d), Vertical), 6);
        }

        [Fact]
        public void Foot_ProjectsOntoTheLineAndKeepsTheHeight()
        {
            Point3 foot = GridSnap.Foot(new Point3(7d, 300d, 1000d), Vertical);

            Assert.Equal(0d, foot.X, 6);
            Assert.Equal(300d, foot.Y, 6);
            Assert.Equal(1000d, foot.Z, 6);
        }

        [Fact]
        public void Intersection_FindsTheCrossingOfTwoPerpendicularLines()
        {
            Point3? crossing = GridSnap.Intersection(
                new GridLine(new Point3(100d, 0d, 0d), new Point3(0d, 1d, 0d)),
                new GridLine(new Point3(0d, 200d, 0d), new Point3(1d, 0d, 0d)));

            Assert.NotNull(crossing);
            Assert.Equal(100d, crossing!.Value.X, 6);
            Assert.Equal(200d, crossing.Value.Y, 6);
        }

        [Fact]
        public void Intersection_IsNullForParallelLines()
        {
            Point3? crossing = GridSnap.Intersection(
                Vertical,
                new GridLine(new Point3(300d, 0d, 0d), new Point3(0d, -1d, 0d)));

            Assert.Null(crossing);
        }

        [Fact]
        public void Plan_LandsTheColumnOnTheCrossingOfTwoLines()
        {
            AlignTarget target = AlignPlanner.Plan(
                new Point3(3d, 4d, 1500d),
                new[] { Vertical, Horizontal },
                Length.FromMillimeters(5d));

            Assert.Equal(2, target.Lines);
            Assert.True(target.Moved);
            Assert.Equal(0d, target.Position.X, 6);
            Assert.Equal(0d, target.Position.Y, 6);
            Assert.Equal(1500d, target.Position.Z, 6);
        }

        [Fact]
        public void Plan_UsesTheFootOfASingleLine()
        {
            AlignTarget target = AlignPlanner.Plan(
                new Point3(4d, 800d, 0d),
                new[] { Vertical },
                Length.FromMillimeters(5d));

            Assert.Equal(1, target.Lines);
            Assert.True(target.Moved);
            Assert.Equal(0d, target.Position.X, 6);
            Assert.Equal(800d, target.Position.Y, 6);
        }

        [Fact]
        public void Plan_LeavesAColumnWithoutALineInReachAlone()
        {
            AlignTarget target = AlignPlanner.Plan(
                new Point3(50d, 800d, 0d),
                new[] { Vertical, Horizontal },
                Length.FromMillimeters(5d));

            Assert.Equal(0, target.Lines);
            Assert.False(target.Moved);
            Assert.Equal(50d, target.Position.X, 6);
        }

        [Fact]
        public void Plan_CountsAColumnAlreadyOnTheLineAsAligned()
        {
            AlignTarget target = AlignPlanner.Plan(
                new Point3(0d, 800d, 0d),
                new[] { Vertical },
                Length.FromMillimeters(5d));

            Assert.Equal(1, target.Lines);
            Assert.False(target.Moved);
        }

        [Fact]
        public void Plan_UsesOnlyTheNearestOfParallelLines()
        {
            var nearby = new GridLine(new Point3(1d, 0d, 0d), new Point3(0d, 1d, 0d));

            AlignTarget target = AlignPlanner.Plan(
                new Point3(4d, 800d, 0d),
                new[] { Vertical, nearby },
                Length.FromMillimeters(5d));

            Assert.Equal(1, target.Lines);
            Assert.Equal(1d, target.Position.X, 6);
        }

        [Fact]
        public void Plan_WorksWithAnAngledGrid()
        {
            // A grid running at forty-five degrees through the origin; a column three
            // millimetres to its left lands on the foot of the perpendicular.
            var angled = new GridLine(new Point3(0d, 0d, 0d), new Point3(1d, 1d, 0d));

            AlignTarget target = AlignPlanner.Plan(
                new Point3(0d, 6d, 0d),
                new[] { angled },
                Length.FromMillimeters(5d));

            Assert.Equal(1, target.Lines);
            Assert.True(target.Moved);
            Assert.Equal(3d, target.Position.X, 6);
            Assert.Equal(3d, target.Position.Y, 6);
        }

        [Fact]
        public void Plan_RejectsANegativeCapture()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AlignPlanner.Plan(
                Point3.Origin,
                new List<GridLine> { Vertical },
                Length.FromMillimeters(-1d)));
        }
    }
}
