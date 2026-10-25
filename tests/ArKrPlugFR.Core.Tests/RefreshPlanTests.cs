using System.Collections.Generic;
using ArKrPlugFR.Core.Annotations;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class RefreshPlanTests
    {
        private static readonly IReadOnlyList<TagRecord> Tags = new[]
        {
            new TagRecord(11, new long[] { 101 }, orphaned: false),
            new TagRecord(12, new long[] { 102 }, orphaned: false),
            new TagRecord(13, new long[] { 103 }, orphaned: true),
            new TagRecord(14, null, orphaned: true)
        };

        [Fact]
        public void Build_RefreshesTheWholeViewWhenNothingIsSelected()
        {
            RefreshPlan plan = RefreshPlanner.Build(Tags, new List<long>());

            Assert.Equal(new long[] { 11, 12 }, plan.Tags);
            Assert.Equal(2, plan.Orphaned);
        }

        [Fact]
        public void Build_ReadsANullSelectionAsTheWholeView()
        {
            RefreshPlan plan = RefreshPlanner.Build(Tags, null);

            Assert.Equal(new long[] { 11, 12 }, plan.Tags);
        }

        [Fact]
        public void Build_NarrowsToTheTagsOfTheSelectedElements()
        {
            RefreshPlan plan = RefreshPlanner.Build(Tags, new List<long> { 102 });

            Assert.Equal(new long[] { 12 }, plan.Tags);
            Assert.Equal(0, plan.Orphaned);
        }

        [Fact]
        public void Build_AlsoReadsTheSelectionOfTheTagItself()
        {
            RefreshPlan plan = RefreshPlanner.Build(Tags, new List<long> { 11 });

            Assert.Equal(new long[] { 11 }, plan.Tags);
        }

        [Fact]
        public void Build_CountsOnlyTheOrphansInScope()
        {
            // The selected element 103 is tagged by an orphaned tag: it is considered,
            // so it is counted; the other orphan is out of scope and is not.
            RefreshPlan plan = RefreshPlanner.Build(Tags, new List<long> { 103 });

            Assert.Empty(plan.Tags);
            Assert.Equal(1, plan.Orphaned);
        }

        [Fact]
        public void Build_ReturnsNothingWhenTheSelectionTagsNothing()
        {
            RefreshPlan plan = RefreshPlanner.Build(Tags, new List<long> { 999 });

            Assert.Empty(plan.Tags);
            Assert.Equal(0, plan.Orphaned);
        }

        [Fact]
        public void Build_OrdersTheIdsAndDropsRepetitions()
        {
            var tags = new[]
            {
                new TagRecord(9, new long[] { 1 }, orphaned: false),
                new TagRecord(3, new long[] { 2 }, orphaned: false),
                new TagRecord(9, new long[] { 2 }, orphaned: false)
            };

            RefreshPlan plan = RefreshPlanner.Build(tags, null);

            Assert.Equal(new long[] { 3, 9 }, plan.Tags);
        }

        [Fact]
        public void Build_ReturnsAnEmptyPlanForAViewWithoutTags()
        {
            RefreshPlan plan = RefreshPlanner.Build(new List<TagRecord>(), null);

            Assert.Empty(plan.Tags);
            Assert.Equal(0, plan.Orphaned);
        }
    }
}
