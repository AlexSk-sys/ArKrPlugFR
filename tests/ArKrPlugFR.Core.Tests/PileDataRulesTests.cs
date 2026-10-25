using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Piles;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class PileDataRulesTests
    {
        [Fact]
        public void Order_TopDown_CountsFromTheLargestYDown()
        {
            IReadOnlyList<int> order = PileNumbering.Order(
                new[] { P(0d, 0d), P(0d, 1000d), P(500d, 1000d) },
                topDown: true);

            Assert.Equal(new[] { 1, 2, 0 }, new List<int>(order));
        }

        [Fact]
        public void Order_BottomUp_CountsFromTheSmallestYUp()
        {
            IReadOnlyList<int> order = PileNumbering.Order(
                new[] { P(0d, 1000d), P(0d, 0d) },
                topDown: false);

            Assert.Equal(new[] { 1, 0 }, new List<int>(order));
        }

        [Fact]
        public void Order_CountsLeftToRightWithinARow()
        {
            IReadOnlyList<int> order = PileNumbering.Order(
                new[] { P(900d, 500d), P(100d, 500d) },
                topDown: true);

            Assert.Equal(new[] { 1, 0 }, new List<int>(order));
        }

        [Fact]
        public void Order_OfPilesAtTheSamePlace_KeepsTheReadingOrder()
        {
            IReadOnlyList<int> order = PileNumbering.Order(
                new[] { P(500d, 500d), P(500d, 500d) },
                topDown: true);

            Assert.Equal(new[] { 0, 1 }, new List<int>(order));
        }

        [Fact]
        public void Order_WithNoPiles_IsEmpty()
        {
            Assert.Empty(PileNumbering.Order(new List<Point3>(), topDown: true));
        }

        [Fact]
        public void CutHeight_MeasuresThePartAboveTheSlabBottom()
        {
            double? cut = CutHeightRules.CutHeight(PileBox(0d, 0d, 3000d), SlabBox(-1000d, 5000d, 2500d));

            Assert.Equal(500d, cut);
        }

        [Fact]
        public void CutHeight_IsCappedAtThePileItself()
        {
            var pile = new BoundingBox(
                new Point3(-150d, -150d, 1500d),
                new Point3(150d, 150d, 3000d),
                extremesAreOrdered: true);

            double? cut = CutHeightRules.CutHeight(pile, SlabBox(-1000d, 5000d, 1000d));

            Assert.Equal(1500d, cut);
        }

        [Fact]
        public void CutHeight_WithoutOverlapInPlan_IsNothing()
        {
            double? cut = CutHeightRules.CutHeight(PileBox(0d, 0d, 3000d), SlabBox(5000d, 5000d, 2500d));

            Assert.Null(cut);
        }

        [Fact]
        public void CutHeight_WithTheSlabAboveThePileTop_IsNothing()
        {
            double? cut = CutHeightRules.CutHeight(PileBox(0d, 0d, 3000d), SlabBox(-1000d, 5000d, 3000d));

            Assert.Null(cut);
        }

        private static Point3 P(double x, double y)
        {
            return new Point3(x, y, 0d);
        }

        private static BoundingBox PileBox(double x, double y, double top)
        {
            return new BoundingBox(
                new Point3(x - 150d, y - 150d, top - 6000d),
                new Point3(x + 150d, y + 150d, top),
                extremesAreOrdered: true);
        }

        private static BoundingBox SlabBox(double minX, double maxX, double bottom)
        {
            return new BoundingBox(
                new Point3(minX, -2000d, bottom),
                new Point3(maxX, 2000d, bottom + 500d),
                extremesAreOrdered: true);
        }
    }
}
