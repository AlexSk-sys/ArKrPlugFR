using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Duplication;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class PurgePlanTests
    {
        private static IReadOnlyDictionary<long, int> Counts(params (long Id, int Count)[] counts)
        {
            var map = new Dictionary<long, int>();

            foreach ((long id, int count) in counts)
            {
                map[id] = count;
            }

            return map;
        }

        [Fact]
        public void Build_AllowsAnItemWithNoInstances()
        {
            PurgePlan plan = PurgePlan.Build(new[] { 1L }, Counts((1L, 0)));

            Assert.Equal(new[] { 1L }, plan.Doomed);
            Assert.Empty(plan.Blocked);
        }

        [Fact]
        public void Build_BlocksAnItemThatIsUsed()
        {
            PurgePlan plan = PurgePlan.Build(new[] { 1L }, Counts((1L, 3)));

            Assert.Empty(plan.Doomed);
            Assert.Equal(new[] { 1L }, plan.Blocked);
        }

        [Fact]
        public void Build_BlocksAnItemWhoseUsageIsUnknown()
        {
            // An id that is not in the count map has not been seen as an instance,
            // but the absence of evidence is not proof that it is unused, so it is
            // kept.
            PurgePlan plan = PurgePlan.Build(new[] { 7L }, Counts((1L, 0)));

            Assert.Empty(plan.Doomed);
            Assert.Equal(new[] { 7L }, plan.Blocked);
        }

        [Fact]
        public void Build_SeparatesTheTwoGroups()
        {
            PurgePlan plan = PurgePlan.Build(
                new[] { 1L, 2L, 3L },
                Counts((1L, 0), (2L, 4), (3L, 0)));

            Assert.Equal(new[] { 1L, 3L }, plan.Doomed);
            Assert.Equal(new[] { 2L }, plan.Blocked);
        }

        [Fact]
        public void Build_IgnoresDuplicates()
        {
            PurgePlan plan = PurgePlan.Build(new[] { 1L, 1L, 1L }, Counts((1L, 0)));

            Assert.Single(plan.Doomed);
        }

        [Fact]
        public void Build_OfNothingIsNothing()
        {
            PurgePlan plan = PurgePlan.Build(Array.Empty<long>(), Counts((1L, 0)));

            Assert.Empty(plan.Doomed);
            Assert.Empty(plan.Blocked);
        }

        [Fact]
        public void Build_RejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => PurgePlan.Build(null!, Counts()));
            Assert.Throws<ArgumentNullException>(() => PurgePlan.Build(Array.Empty<long>(), null!));
        }
    }
}
