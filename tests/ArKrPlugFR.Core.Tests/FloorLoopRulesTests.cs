using System.Collections.Generic;
using ArKrPlugFR.Core.Floors;
using ArKrPlugFR.Core.Geometry;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class FloorLoopRulesTests
    {
        [Fact]
        public void Classify_TwoDisjointSquares_TwoPartsWithoutOpenings()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { Square(0d, 0d, 10d), Square(20d, 0d, 10d) });

            Assert.Equal(2, plan.Parts.Count);
            Assert.Equal(0, plan.Parts[0].Outer);
            Assert.Equal(1, plan.Parts[1].Outer);
            Assert.Empty(plan.Parts[0].Holes);
            Assert.Empty(plan.Parts[1].Holes);
            Assert.False(plan.HasRejectedContours);
        }

        [Fact]
        public void Classify_SquareWithAHole_OnePartWithThatOpening()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { Square(0d, 0d, 30d), Square(10d, 10d, 10d) });

            Assert.Single(plan.Parts);
            Assert.Equal(0, plan.Parts[0].Outer);
            Assert.Equal(new[] { 1 }, plan.Parts[0].Holes);
        }

        [Fact]
        public void Classify_SquareWithTwoHoles_BothOpeningsOfThePart()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(
                new[] { Square(0d, 0d, 30d), Square(5d, 5d, 5d), Square(15d, 15d, 5d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1, 2 }, plan.Parts[0].Holes);
        }

        [Fact]
        public void Classify_HoleInTheArmOfAConcaveContour_StaysAnOpening()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { LShape(), Rect(1d, 1d, 2d, 2d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1 }, plan.Parts[0].Holes);
        }

        [Fact]
        public void Classify_HoleInTheNotchOfAConcaveContour_BecomesItsOwnPart()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { LShape(), Rect(6d, 6d, 2d, 2d) });

            Assert.Equal(2, plan.Parts.Count);
            Assert.Empty(plan.Parts[0].Holes);
            Assert.Empty(plan.Parts[1].Holes);
            Assert.False(plan.HasRejectedContours);
        }

        [Fact]
        public void Classify_ContourInsideAnOpening_ReportedAsNested()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(
                new[] { Square(0d, 0d, 30d), Square(10d, 10d, 10d), Square(12d, 12d, 6d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1 }, plan.Parts[0].Holes);
            Assert.Equal(new[] { 2 }, plan.Nested);
        }

        [Fact]
        public void Classify_SmallestContainerWins()
        {
            // The small square lies inside the medium one, which lies inside the outer one.
            // The medium square is the container, so the small one is inside an opening.
            FloorLoopPlan plan = FloorLoopRules.Classify(
                new[] { Square(0d, 0d, 40d), Square(10d, 10d, 20d), Square(12d, 12d, 8d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1 }, plan.Parts[0].Holes);
            Assert.Equal(new[] { 2 }, plan.Nested);
        }

        [Fact]
        public void Classify_DuplicateContours_OnePartAndOneDuplicate()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { Square(0d, 0d, 10d), Square(0d, 0d, 10d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1 }, plan.Duplicates);
            Assert.True(plan.HasRejectedContours);
        }

        [Fact]
        public void Classify_ThreeCongruentContours_OnePartAndTwoDuplicates()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(
                new[] { Square(0d, 0d, 10d), Square(0d, 0d, 10d), Square(0d, 0d, 10d) });

            Assert.Single(plan.Parts);
            Assert.Empty(plan.Parts[0].Holes);
            Assert.Equal(new[] { 1, 2 }, plan.Duplicates);
        }

        [Fact]
        public void Classify_DuplicateOfAHole_IsReportedAndTheHoleStays()
        {
            // The outer square holds one opening; the same opening is listed twice.
            FloorLoopPlan plan = FloorLoopRules.Classify(
                new[] { Square(0d, 0d, 30d), Square(10d, 10d, 10d), Square(10d, 10d, 10d) });

            Assert.Single(plan.Parts);
            Assert.Equal(new[] { 1 }, plan.Parts[0].Holes);
            Assert.Equal(new[] { 2 }, plan.Duplicates);
        }

        [Fact]
        public void Classify_DegenerateContours_ReportedWithTheirIndexes()
        {
            var tooShort = new FloorContour(new[] { new Point3(0d, 0d, 0d), new Point3(1d, 0d, 0d) });
            var collinear = new FloorContour(new[]
            {
                new Point3(0d, 0d, 0d),
                new Point3(5d, 0d, 0d),
                new Point3(10d, 0d, 0d)
            });

            FloorLoopPlan plan = FloorLoopRules.Classify(new[] { tooShort, collinear });

            Assert.Empty(plan.Parts);
            Assert.Equal(new[] { 0, 1 }, plan.Degenerate);
        }

        [Fact]
        public void Classify_EmptyList_EmptyPlan()
        {
            FloorLoopPlan plan = FloorLoopRules.Classify(new List<FloorContour>());

            Assert.Empty(plan.Parts);
            Assert.False(plan.HasRejectedContours);
        }

        [Fact]
        public void Contains_ReadsAConcaveContourCorrectly()
        {
            FloorContour contour = LShape();

            Assert.True(FloorLoopRules.Contains(contour, new Point3(2d, 2d, 0d)));
            Assert.False(FloorLoopRules.Contains(contour, new Point3(6d, 6d, 0d)));
        }

        [Fact]
        public void Contains_ReadsAPointOutsideEveryEdge()
        {
            FloorContour contour = Square(0d, 0d, 10d);

            Assert.True(FloorLoopRules.Contains(contour, new Point3(5d, 5d, 0d)));
            Assert.False(FloorLoopRules.Contains(contour, new Point3(30d, 30d, 0d)));
        }

        [Fact]
        public void Area_OfASquare_IsItsSideSquared()
        {
            Assert.Equal(100d, Square(0d, 0d, 10d).Area, 6);
            Assert.Equal(6d, Rect(0d, 0d, 3d, 2d).Area, 6);
        }

        [Fact]
        public void Contour_IsDegenerateUnderOneSquareMillimetre()
        {
            Assert.True(new FloorContour(new[] { new Point3(0d, 0d, 0d), new Point3(0.5d, 0d, 0d), new Point3(0d, 0.5d, 0d) }).IsDegenerate);
            Assert.False(Square(0d, 0d, 2d).IsDegenerate);
        }

        private static FloorContour Square(double x, double y, double size)
        {
            return Rect(x, y, size, size);
        }

        private static FloorContour Rect(double x, double y, double width, double height)
        {
            return new FloorContour(new[]
            {
                new Point3(x, y, 0d),
                new Point3(x + width, y, 0d),
                new Point3(x + width, y + height, 0d),
                new Point3(x, y + height, 0d)
            });
        }

        private static FloorContour LShape()
        {
            return new FloorContour(new[]
            {
                new Point3(0d, 0d, 0d),
                new Point3(10d, 0d, 0d),
                new Point3(10d, 4d, 0d),
                new Point3(4d, 4d, 0d),
                new Point3(4d, 10d, 0d),
                new Point3(0d, 10d, 0d)
            });
        }
    }
}
