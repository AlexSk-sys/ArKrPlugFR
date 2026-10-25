using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Joins;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class UnjoinCandidatesTests
    {
        private const long Wall = 100L;
        private const long Column = 200L;
        private const long Beam = 300L;

        private static BoundingBox Box(double min, double max)
        {
            return new BoundingBox(new Point3(min, min, min), new Point3(max, max, max));
        }

        private static IReadOnlyDictionary<long, BoundingBox> Walls(params long[] ids)
        {
            var boxes = new Dictionary<long, BoundingBox>();

            foreach (long id in ids)
            {
                boxes[id] = Box(0d, 1000d);
            }

            return boxes;
        }

        [Fact]
        public void Select_KeepsJoinedOverlappingNeighbour()
        {
            var candidates = new[] { new UnjoinCandidate(Wall, Column, Box(900d, 1100d), isJoined: true) };

            IReadOnlyList<JoinPair> pairs = UnjoinCandidates.Select(Walls(Wall), candidates);

            Assert.Single(pairs);
            Assert.Equal(new JoinPair(Wall, Column), pairs[0]);
        }

        [Fact]
        public void Select_SkipsNeighbourThatIsNotJoined()
        {
            var candidates = new[] { new UnjoinCandidate(Wall, Column, Box(900d, 1100d), isJoined: false) };

            Assert.Empty(UnjoinCandidates.Select(Walls(Wall), candidates));
        }

        [Fact]
        public void Select_SkipsNeighbourThatCannotReachTheWall()
        {
            var candidates = new[] { new UnjoinCandidate(Wall, Column, Box(5000d, 6000d), isJoined: true) };

            Assert.Empty(UnjoinCandidates.Select(Walls(Wall), candidates));
        }

        [Fact]
        public void Select_UsesToleranceToReachANearbyNeighbour()
        {
            var candidates = new[] { new UnjoinCandidate(Wall, Beam, Box(1100d, 1200d), isJoined: true) };

            Assert.Empty(UnjoinCandidates.Select(Walls(Wall), candidates));

            IReadOnlyList<JoinPair> pairs = UnjoinCandidates.Select(
                Walls(Wall),
                candidates,
                Length.FromMillimeters(UnjoinSearchOptions.DefaultToleranceMillimeters));

            Assert.Single(pairs);
            Assert.Equal(new JoinPair(Wall, Beam), pairs[0]);
        }

        [Fact]
        public void Select_IgnoresWallsThatAreNotInTheScope()
        {
            var candidates = new[] { new UnjoinCandidate(999L, Column, Box(0d, 10d), isJoined: true) };

            Assert.Empty(UnjoinCandidates.Select(Walls(Wall), candidates));
        }

        [Fact]
        public void Select_ReportsEachJoinOnceWhenBothSidesAreSupplied()
        {
            var candidates = new[]
            {
                new UnjoinCandidate(Wall, Column, Box(900d, 1100d), isJoined: true),
                new UnjoinCandidate(Column, Wall, Box(900d, 1100d), isJoined: true),
            };

            Assert.Single(UnjoinCandidates.Select(Walls(Wall, Column), candidates));
        }

        [Fact]
        public void Select_OrdersPairsByElementId()
        {
            var candidates = new[]
            {
                new UnjoinCandidate(Wall, Beam, Box(900d, 1100d), isJoined: true),
                new UnjoinCandidate(Wall, Column, Box(900d, 1100d), isJoined: true),
            };

            IReadOnlyList<JoinPair> pairs = UnjoinCandidates.Select(Walls(Wall), candidates);

            Assert.Equal(2, pairs.Count);
            Assert.Equal(new JoinPair(Wall, Column), pairs[0]);
            Assert.Equal(new JoinPair(Wall, Beam), pairs[1]);
        }

        [Fact]
        public void Select_OfNothingIsEmpty()
        {
            Assert.Empty(UnjoinCandidates.Select(Walls(Wall), Array.Empty<UnjoinCandidate>()));
        }

        [Fact]
        public void Select_RejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(
                () => UnjoinCandidates.Select(null!, Array.Empty<UnjoinCandidate>()));

            Assert.Throws<ArgumentNullException>(
                () => UnjoinCandidates.Select(Walls(Wall), null!));
        }

        [Fact]
        public void Select_RejectsNegativeTolerance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => UnjoinCandidates.Select(Walls(Wall), Array.Empty<UnjoinCandidate>(), Length.FromMillimeters(-1d)));
        }

        [Fact]
        public void Candidate_RejectsSelfReference()
        {
            Assert.Throws<ArgumentException>(() => new UnjoinCandidate(Wall, Wall, Box(0d, 1d), isJoined: true));
        }

        [Fact]
        public void Options_DefaultMarginIsHalfAFoot()
        {
            var options = new UnjoinSearchOptions();

            Assert.Equal(152.4d, options.SearchTolerance.Millimeters, 6);
            Assert.Equal(Length.MillimetersPerFoot / 2d, options.SearchTolerance.Millimeters, 6);
        }

        [Fact]
        public void Options_RejectNegativeMargin()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnjoinSearchOptions(Length.FromMillimeters(-1d)));
        }
    }
}
