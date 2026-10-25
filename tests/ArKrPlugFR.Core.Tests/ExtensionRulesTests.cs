using System;
using ArKrPlugFR.Core.Analytical;
using ArKrPlugFR.Core.Geometry;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ExtensionRulesTests
    {
        private static BoundingBox Box(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            return new BoundingBox(new Point3(minX, minY, minZ), new Point3(maxX, maxY, maxZ), extremesAreOrdered: true);
        }

        [Fact]
        public void PlanarDistance_IsZeroAboveTheFootprintAndGrowsOutsideIt()
        {
            BoundingBox footprint = Box(0, 0, 3000, 1000, 1000, 3200);

            Assert.Equal(0, ExtensionRules.PlanarDistance(new Point3(500, 500, 0), footprint), 6);
            Assert.Equal(500, ExtensionRules.PlanarDistance(new Point3(1500, 500, 0), footprint), 6);
            Assert.Equal(Math.Sqrt(500 * 500 * 2), ExtensionRules.PlanarDistance(new Point3(1500, 1500, 0), footprint), 6);
        }

        [Fact]
        public void FindBestTarget_ChoosesTheLowestTargetAboveWithinTheReach()
        {
            var targets = new[]
            {
                new ExtensionTarget(Box(0, 0, 6000, 1000, 1000, 6200)),
                new ExtensionTarget(Box(0, 0, 4000, 1000, 1000, 4200)),
                new ExtensionTarget(Box(0, 0, 3000, 1000, 1000, 3200))
            };

            // The top of the member sits at 3500, so only the two higher targets qualify.
            int best = ExtensionRules.FindBestTarget(new Point3(500, 500, 3500), targets, 4572);

            Assert.Equal(1, best);
        }

        [Fact]
        public void FindBestTarget_IgnoresTargetsBelowAndTargetsTooFarAway()
        {
            var below = new[] { new ExtensionTarget(Box(0, 0, 1000, 1000, 1000, 1200)) };
            Assert.Equal(-1, ExtensionRules.FindBestTarget(new Point3(500, 500, 3500), below, 4572));

            var far = new[] { new ExtensionTarget(Box(9000, 0, 4000, 10000, 1000, 4200)) };
            Assert.Equal(-1, ExtensionRules.FindBestTarget(new Point3(500, 500, 3500), far, 4572));

            Assert.Equal(0, ExtensionRules.FindBestTarget(new Point3(500, 500, 3500), far, 9000));
        }

        [Fact]
        public void FindBestTarget_BreaksATieByTheNearerFootprint()
        {
            var targets = new[]
            {
                new ExtensionTarget(Box(3000, 0, 4000, 4000, 1000, 4200)),
                new ExtensionTarget(Box(0, 0, 4000, 1000, 1000, 4200))
            };

            int best = ExtensionRules.FindBestTarget(new Point3(500, 500, 3500), targets, 4572);

            Assert.Equal(1, best);
        }

        [Fact]
        public void FindBestTarget_RejectsBadArguments()
        {
            var targets = new[] { new ExtensionTarget(Box(0, 0, 4000, 1000, 1000, 4200)) };

            Assert.Throws<ArgumentNullException>(
                () => ExtensionRules.FindBestTarget(new Point3(0, 0, 0), null!, 1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => ExtensionRules.FindBestTarget(new Point3(0, 0, 0), targets, -1));
        }

        [Fact]
        public void ExtendedTop_KeepsThePlaceInPlanAndRaisesTheHeight()
        {
            Assert.Equal(
                new Point3(100, 200, 4000),
                ExtensionRules.ExtendedTop(new Point3(100, 200, 3500), 4000));
        }
    }
}
