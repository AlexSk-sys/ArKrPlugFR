using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Joins;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class JoinPairsTests
    {
        [Fact]
        public void JoinPair_NormalisesOrder()
        {
            var pair = new JoinPair(900L, 120L);

            Assert.Equal(120L, pair.First);
            Assert.Equal(900L, pair.Second);
            Assert.Equal(new JoinPair(120L, 900L), pair);
        }

        [Fact]
        public void JoinPair_RejectsSelfJoin()
        {
            Assert.Throws<ArgumentException>(() => new JoinPair(5L, 5L));
        }

        [Fact]
        public void Collect_ReturnsEachJoinOnce()
        {
            var map = new Dictionary<long, IReadOnlyCollection<long>>
            {
                [1L] = new[] { 2L, 3L },
                [2L] = new[] { 1L },
                [3L] = new[] { 1L },
            };

            var pairs = JoinPairs.Collect(map);

            Assert.Equal(2, pairs.Count);
            Assert.Equal(new JoinPair(1L, 2L), pairs[0]);
            Assert.Equal(new JoinPair(1L, 3L), pairs[1]);
        }

        [Fact]
        public void Collect_SkipsElementsOutsideScope()
        {
            var map = new Dictionary<long, IReadOnlyCollection<long>>
            {
                [1L] = new[] { 2L, 500L },
                [2L] = new[] { 1L },
            };

            var pairs = JoinPairs.Collect(map);

            Assert.Single(pairs);
            Assert.Equal(new JoinPair(1L, 2L), pairs[0]);
        }

        [Fact]
        public void Collect_IgnoresNullAndSelfReferences()
        {
            var map = new Dictionary<long, IReadOnlyCollection<long>>
            {
                [1L] = null!,
                [2L] = new[] { 2L, 3L },
                [3L] = new[] { 2L },
            };

            var pairs = JoinPairs.Collect(map);

            Assert.Single(pairs);
            Assert.Equal(new JoinPair(2L, 3L), pairs[0]);
        }

        [Fact]
        public void Collect_OfEmptyMapIsEmpty()
        {
            Assert.Empty(JoinPairs.Collect(new Dictionary<long, IReadOnlyCollection<long>>()));
        }

        [Fact]
        public void CountByElement_CountsBothEnds()
        {
            var map = new Dictionary<long, IReadOnlyCollection<long>>
            {
                [1L] = new[] { 2L, 3L },
                [2L] = new[] { 1L, 3L },
                [3L] = new[] { 1L, 2L },
            };

            var counts = JoinPairs.CountByElement(JoinPairs.Collect(map));

            Assert.Equal(2, counts[1L]);
            Assert.Equal(2, counts[2L]);
            Assert.Equal(2, counts[3L]);
        }

        [Fact]
        public void Collect_RejectsNullMap()
        {
            Assert.Throws<ArgumentNullException>(() => JoinPairs.Collect(null!));
        }
    }
}
