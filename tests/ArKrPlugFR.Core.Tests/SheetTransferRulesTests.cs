using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.SheetTransfer;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SheetTransferRulesTests
    {
        private static SheetCandidate Sheet(
            long id,
            string number,
            string name,
            bool isPlaceholder = false,
            params ViewCandidate[] views)
        {
            return new SheetCandidate(id, number, name, isPlaceholder, views);
        }

        [Fact]
        public void IsTransferable_AcceptsARegularSheet()
        {
            Assert.True(SheetTransferRules.IsTransferable(Sheet(1L, "A-01", "Plan")));
        }

        [Fact]
        public void IsTransferable_RejectsAPlaceholderSheet()
        {
            Assert.False(SheetTransferRules.IsTransferable(Sheet(1L, "A-01", "List", isPlaceholder: true)));
        }

        [Theory]
        [InlineData((string?)null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsTransferable_RejectsAMissingNumber(string? number)
        {
            Assert.False(SheetTransferRules.IsTransferable(new SheetCandidate(1L, number!, "Plan", false, Array.Empty<ViewCandidate>())));
        }

        [Fact]
        public void IsTransferable_RejectsNull()
        {
            Assert.False(SheetTransferRules.IsTransferable(null));
        }

        [Fact]
        public void IsTransferableView_AcceptsAPlacedView()
        {
            Assert.True(SheetTransferRules.IsTransferableView(new ViewCandidate(1L, "Plan", isTemplate: false)));
        }

        [Fact]
        public void IsTransferableView_RejectsATemplateAndNull()
        {
            Assert.False(SheetTransferRules.IsTransferableView(new ViewCandidate(1L, "Template", isTemplate: true)));
            Assert.False(SheetTransferRules.IsTransferableView(null));
        }

        [Theory]
        [InlineData("A-1", "A-10", -1)]
        [InlineData("A-10", "A-2", 1)]
        [InlineData("A-2", "A-10", -1)]
        [InlineData("Sheet 9", "Sheet 10", -1)]
        [InlineData("a-2", "A-10", -1)]
        [InlineData("A-01", "A-1", 0)]
        [InlineData("B-1", "A-2", 1)]
        [InlineData("", "A-1", -1)]
        [InlineData("A-1", "A-1", 0)]
        public void CompareSheetNumbers_OrdersNaturally(string left, string right, int expectedSign)
        {
            int comparison = SheetTransferRules.CompareSheetNumbers(left, right);

            Assert.Equal(Math.Sign(expectedSign), Math.Sign(comparison));
        }

        [Fact]
        public void CompareSheetNumbers_AcceptsNull()
        {
            Assert.True(SheetTransferRules.CompareSheetNumbers(null, "A-1") < 0);
            Assert.Equal(0, SheetTransferRules.CompareSheetNumbers(null, null));
        }

        [Fact]
        public void SelectTransferable_FiltersAndOrdersNaturally()
        {
            IReadOnlyList<SheetCandidate> result = SheetTransferRules.SelectTransferable(new[]
            {
                Sheet(2L, "A-10", "Tenth"),
                Sheet(1L, "A-1", "First"),
                Sheet(3L, "A-2", "List", isPlaceholder: true),
                Sheet(4L, "  ", "Broken"),
            });

            Assert.Equal(new[] { 1L, 2L }, result.Select(sheet => sheet.Id).ToArray());
        }

        [Fact]
        public void SelectTransferable_KeepsADuplicateIdOnce()
        {
            SheetCandidate sheet = Sheet(1L, "A-1", "First");

            IReadOnlyList<SheetCandidate> result = SheetTransferRules.SelectTransferable(new[] { sheet, sheet });

            Assert.Single(result);
        }

        [Fact]
        public void SelectTransferable_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => SheetTransferRules.SelectTransferable(null!));
        }

        [Fact]
        public void SanitizeName_StripsForbiddenCharactersAndTrims()
        {
            Assert.Equal("Plan 1 copy", SheetTransferRules.SanitizeName(" Plan [1] {copy} "));
            Assert.Equal(string.Empty, SheetTransferRules.SanitizeName(null));
        }

        [Fact]
        public void MarkSheetName_AppendsTheMarkerOnce()
        {
            Assert.Equal("Plan (copy)", SheetTransferRules.MarkSheetName("Plan", "(copy)"));
            Assert.Equal("Plan (COPY)", SheetTransferRules.MarkSheetName("Plan (COPY)", "(copy)"));
        }

        [Fact]
        public void MarkSheetName_WithoutMarkerOnlyCleans()
        {
            Assert.Equal("Plan 1", SheetTransferRules.MarkSheetName(" Plan [1] ", string.Empty));
        }

        [Fact]
        public void MakeUniqueNumber_KeepsAFreeNumber()
        {
            Assert.Equal("A-01", SheetTransferRules.MakeUniqueNumber("A-01", Array.Empty<string>()));
        }

        [Fact]
        public void MakeUniqueNumber_AppendsTheLowestFreeSuffix()
        {
            var taken = new List<string> { "A-01", "A-01_1", "A-01_3" };

            Assert.Equal("A-01_2", SheetTransferRules.MakeUniqueNumber("A-01", taken));
        }

        [Fact]
        public void MakeUniqueNumber_IgnoresLetterCase()
        {
            Assert.Equal("A-01_1", SheetTransferRules.MakeUniqueNumber("A-01", new[] { "a-01" }));
        }

        [Fact]
        public void MakeUniqueNumber_CleansTheNumberFirst()
        {
            Assert.Equal("A01", SheetTransferRules.MakeUniqueNumber("A[01]", Array.Empty<string>()));
        }

        [Fact]
        public void MakeUniqueNumber_RejectsNullTaken()
        {
            Assert.Throws<ArgumentNullException>(() => SheetTransferRules.MakeUniqueNumber("A-01", null!));
        }

        [Fact]
        public void MakeUniqueViewName_AppendsMarkerAndGrowsTheTail()
        {
            Assert.Equal("Detail (copy)", SheetTransferRules.MakeUniqueViewName("Detail", "(copy)", Array.Empty<string>()));

            var taken = new List<string> { "Detail (copy)", "Detail (copy) 2" };

            Assert.Equal("Detail (copy) 3", SheetTransferRules.MakeUniqueViewName("Detail", "(copy)", taken));
        }

        [Fact]
        public void Build_OrdersSheetsByNumberNaturally()
        {
            var selected = new[]
            {
                Sheet(2L, "A-10", "Tenth"),
                Sheet(1L, "A-01", "First"),
            };

            TransferPlan plan = TransferPlan.Build(selected, new[] { "A-01" }, "(copy)");

            Assert.Equal(2, plan.PlannedCount);
            Assert.Equal("A-01_1", plan.Items[0].Number);
            Assert.Equal("First (copy)", plan.Items[0].Name);
            Assert.Equal("A-10", plan.Items[1].Number);
            Assert.Equal("Tenth (copy)", plan.Items[1].Name);
        }

        [Fact]
        public void Build_CollidesWithNumbersPlannedEarlierInItself()
        {
            var selected = new[]
            {
                Sheet(1L, "A-01", "One"),
                Sheet(2L, "A-01", "Two"),
            };

            TransferPlan plan = TransferPlan.Build(selected, Array.Empty<string>(), "(copy)");

            Assert.Equal("A-01", plan.Items[0].Number);
            Assert.Equal("A-01_1", plan.Items[1].Number);
        }

        [Fact]
        public void Build_SkipsPlaceholdersAndDuplicates()
        {
            SheetCandidate sheet = Sheet(1L, "A-01", "Plan");

            var selected = new[]
            {
                sheet,
                sheet,
                Sheet(2L, "A-02", "List", isPlaceholder: true),
            };

            TransferPlan plan = TransferPlan.Build(selected, Array.Empty<string>(), "(copy)");

            Assert.Equal(1, plan.PlannedCount);
            Assert.Equal(1L, plan.Items[0].SourceSheetId);
        }

        [Fact]
        public void Build_CountsOnlyTransferableViews()
        {
            var views = new[]
            {
                new ViewCandidate(10L, "Plan", isTemplate: false),
                new ViewCandidate(11L, "Template", isTemplate: true),
            };

            TransferPlan plan = TransferPlan.Build(new[] { Sheet(1L, "A-01", "Plan", false, views) }, Array.Empty<string>(), "(copy)");

            Assert.Equal(1, plan.Items[0].ViewCount);
        }

        [Fact]
        public void Build_OfNothingIsNothing()
        {
            TransferPlan plan = TransferPlan.Build(Array.Empty<SheetCandidate>(), new[] { "A-01" }, "(copy)");

            Assert.Equal(0, plan.PlannedCount);
            Assert.Empty(plan.Items);
        }

        [Fact]
        public void Build_RejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => TransferPlan.Build(null!, Array.Empty<string>(), "(copy)"));
            Assert.Throws<ArgumentNullException>(() => TransferPlan.Build(Array.Empty<SheetCandidate>(), null!, "(copy)"));
            Assert.Throws<ArgumentNullException>(() => TransferPlan.Build(Array.Empty<SheetCandidate>(), Array.Empty<string>(), null!));
        }

        [Fact]
        public void Build_GivesASheetItsCopyCount()
        {
            var selected = new[]
            {
                Sheet(1L, "A-01", "First"),
                Sheet(2L, "A-10", "Tenth"),
            };

            var counts = new Dictionary<long, int> { { 1L, 3 }, { 2L, 1 } };

            TransferPlan plan = TransferPlan.Build(selected, Array.Empty<string>(), "(copy)", counts);

            Assert.Equal(4, plan.PlannedCount);

            // Every copy of one sheet takes the next free number, and several copies
            // of the same sheet also carry their copy number in the name.
            Assert.Equal("A-01", plan.Items[0].Number);
            Assert.Equal("First (copy) (1)", plan.Items[0].Name);
            Assert.Equal("A-01_1", plan.Items[1].Number);
            Assert.Equal("First (copy) (3)", plan.Items[2].Name);
            Assert.Equal("A-01_2", plan.Items[2].Number);
            Assert.Equal("A-10", plan.Items[3].Number);
            Assert.Equal("Tenth (copy)", plan.Items[3].Name);
        }

        [Fact]
        public void Build_ReadsACountBelowOneAsOneCopy()
        {
            var selected = new[] { Sheet(1L, "A-01", "First") };

            var counts = new Dictionary<long, int> { { 1L, 0 } };

            TransferPlan plan = TransferPlan.Build(selected, Array.Empty<string>(), "(copy)", counts);

            Assert.Equal(1, plan.PlannedCount);
            Assert.Equal("First (copy)", plan.Items[0].Name);
        }
    }
}
