using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Sheets;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SheetRulesTests
    {
        private static SheetFacts Sheet(long id, string number, string name, bool placeholder = false, bool alive = true)
        {
            return new SheetFacts(id, number, name, placeholder, alive);
        }

        [Fact]
        public void IsDuplicable_AcceptsALiveSheetWithANumber()
        {
            Assert.True(SheetRules.IsDuplicable(Sheet(1L, "A-01", "Ground floor")));
        }

        [Fact]
        public void IsDuplicable_RejectsAPlaceholderOfASheetList()
        {
            // A placeholder is a row of a sheet list: there is no content to copy.
            Assert.False(SheetRules.IsDuplicable(Sheet(1L, "A-01", "Ground floor", placeholder: true)));
        }

        [Fact]
        public void IsDuplicable_RejectsASheetThatNoLongerResolves()
        {
            Assert.False(SheetRules.IsDuplicable(Sheet(1L, "A-01", "Ground floor", alive: false)));
        }

        [Fact]
        public void IsDuplicable_RejectsAnEmptyNumber()
        {
            Assert.False(SheetRules.IsDuplicable(Sheet(1L, "   ", "Ground floor")));
        }

        [Fact]
        public void IsDuplicable_RejectsNull()
        {
            Assert.False(SheetRules.IsDuplicable(null));
        }

        [Fact]
        public void SelectDuplicable_KeepsOnlyTheSheetsThatMayBeCopied()
        {
            IReadOnlyList<SheetFacts> selected = SheetRules.SelectDuplicable(new[]
            {
                Sheet(1L, "A-01", "Ground floor"),
                Sheet(2L, "A-02", "Placeholder", placeholder: true),
                Sheet(3L, "A-03", "First floor"),
            });

            Assert.Equal(2, selected.Count);
            Assert.Equal(1L, selected[0].Id);
            Assert.Equal(3L, selected[1].Id);
        }

        [Fact]
        public void SelectDuplicable_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => SheetRules.SelectDuplicable(null!));
        }

        [Fact]
        public void NaturalCompare_SortsEmbeddedNumbersNumerically()
        {
            // A-1 must come before A-10; a plain string comparison would say the opposite.
            Assert.True(SheetRules.NaturalCompare("A-1", "A-10") < 0);
            Assert.True(SheetRules.NaturalCompare("A-2", "A-10") < 0);
            Assert.True(SheetRules.NaturalCompare("A-10", "A-2") > 0);
        }

        [Fact]
        public void NaturalCompare_IgnoresCaseAndLeadingZeros()
        {
            Assert.Equal(0, SheetRules.NaturalCompare("a-1", "A-01"));
            Assert.Equal(0, SheetRules.NaturalCompare("A-1", "a-1"));
        }

        [Fact]
        public void NaturalCompare_ComparesLetterRunsLetterByLetter()
        {
            Assert.True(SheetRules.NaturalCompare("A-1", "B-1") < 0);
            Assert.True(SheetRules.NaturalCompare("AK-1", "AK-2") < 0);
        }

        [Fact]
        public void NaturalCompare_TreatsNullAsEmpty()
        {
            Assert.Equal(0, SheetRules.NaturalCompare(null, string.Empty));
            Assert.True(SheetRules.NaturalCompare(null, "1") < 0);
        }

        [Fact]
        public void FindHighestSuffix_IsZeroWhenTheProjectHasNoCopy()
        {
            Assert.Equal(0, SheetNumbering.FindHighestSuffix("A-01", new[] { "A-01", "A-02", "B-01" }));
        }

        [Fact]
        public void FindHighestSuffix_TakesTheHighestOfSeveral()
        {
            Assert.Equal(
                7,
                SheetNumbering.FindHighestSuffix("A-01", new[] { "A-01", "A-01_1", "A-01_7", "A-01_3" }));
        }

        [Fact]
        public void FindHighestSuffix_IgnoresNumbersThatOnlyStartWithTheSameText()
        {
            // "A-011" extends the digits of the source itself, it is not a copy of it.
            Assert.Equal(
                0,
                SheetNumbering.FindHighestSuffix("A-01", new[] { "A-011", "A-01x" }));
        }

        [Fact]
        public void FindHighestSuffix_IgnoresANonNumericSuffix()
        {
            Assert.Equal(
                0,
                SheetNumbering.FindHighestSuffix("A-01", new[] { "A-01_final" }));
        }

        [Fact]
        public void GenerateNumbers_NumbersConsecutivelyAfterTheHighestExistingSuffix()
        {
            IReadOnlyList<string> numbers = SheetNumbering.GenerateNumbers(
                "A-01",
                new[] { "A-01", "A-01_1", "A-01_2" },
                2);

            Assert.Equal(new[] { "A-01_3", "A-01_4" }, numbers);
        }

        [Fact]
        public void GenerateNumbers_StartsAtOneForAProjectWithoutCopies()
        {
            IReadOnlyList<string> numbers = SheetNumbering.GenerateNumbers("A-01", new[] { "A-01" }, 3);

            Assert.Equal(new[] { "A-01_1", "A-01_2", "A-01_3" }, numbers);
        }

        [Fact]
        public void GenerateNumbers_OfZeroCopiesIsEmpty()
        {
            Assert.Empty(SheetNumbering.GenerateNumbers("A-01", new[] { "A-01" }, 0));
        }

        [Fact]
        public void GenerateNumbers_RejectsANegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => SheetNumbering.GenerateNumbers("A-01", new[] { "A-01" }, -1));
        }

        [Fact]
        public void WithSuffix_AppendsTheNumber()
        {
            Assert.Equal("A-01_5", SheetNumbering.WithSuffix("A-01", 5));
        }

        [Fact]
        public void WithSuffix_RejectsASuffixBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SheetNumbering.WithSuffix("A-01", 0));
        }

        [Fact]
        public void StripDisallowed_RemovesTheRefusedCharactersOnly()
        {
            Assert.Equal("Plan A-1 (existing)", SheetCopyNames.StripDisallowed("Plan A-1 (existing)"));
            Assert.Equal("AB1", SheetCopyNames.StripDisallowed("A{B}1"));
            Assert.Equal("CDD", SheetCopyNames.StripDisallowed("C?D:D"));
            Assert.Equal(string.Empty, SheetCopyNames.StripDisallowed(null));
        }

        [Fact]
        public void MarkedSheetName_AppendsTheCopyNumber()
        {
            Assert.Equal("Ground floor (2)", SheetCopyNames.MarkedSheetName("Ground floor", 2));
        }

        [Fact]
        public void MarkedSheetName_StripsDisallowedCharacters()
        {
            Assert.Equal("Room A (1)", SheetCopyNames.MarkedSheetName("Room {A}", 1));
        }

        [Fact]
        public void NextFreeName_ReturnsTheDesiredNameWhenNothingHasIt()
        {
            Assert.Equal(
                "Plan",
                SheetCopyNames.NextFreeName("Plan", name => name == "Other"));
        }

        [Fact]
        public void NextFreeName_IncreasesTheTailUntilTheNameIsFree()
        {
            var taken = new HashSet<string> { "Plan", "Plan (2)", "Plan (3)" };

            Assert.Equal(
                "Plan (4)",
                SheetCopyNames.NextFreeName("Plan", taken.Contains));
        }

        [Fact]
        public void NextFreeName_OfAnEmptyNameStaysEmpty()
        {
            Assert.Equal(
                string.Empty,
                SheetCopyNames.NextFreeName("{}", name => false));
        }

        [Fact]
        public void Plan_DuplicatesOrdinaryViewsWithoutDetailing()
        {
            SheetCopyPlan plan = SheetViewPlanner.Plan(
                new[] { new SourceViewFact("Ground floor", SheetViewKind.View) },
                new SheetCopyOptions());

            PlannedView planned = Assert.Single(plan.Views);
            Assert.Equal(SheetViewAction.Duplicate, planned.Action);
            Assert.Equal(SheetViewDuplicateOption.Plain, planned.Option);
        }

        [Fact]
        public void Plan_ReusesLegendsAndNeverDuplicatesThem()
        {
            SheetCopyPlan plan = SheetViewPlanner.Plan(
                new[] { new SourceViewFact("Keys", SheetViewKind.Legend) },
                new SheetCopyOptions());

            PlannedView planned = Assert.Single(plan.Views);
            Assert.Equal(SheetViewAction.Reuse, planned.Action);
            Assert.Equal(SheetViewDuplicateOption.None, planned.Option);
        }

        [Fact]
        public void Plan_NeverTouchesSchedules()
        {
            SheetCopyPlan plan = SheetViewPlanner.Plan(
                new[] { new SourceViewFact("Door schedule", SheetViewKind.Schedule) },
                new SheetCopyOptions());

            PlannedView planned = Assert.Single(plan.Views);
            Assert.Equal(SheetViewAction.None, planned.Action);
        }

        [Fact]
        public void Plan_StopsCarryingContentWhenTheSwitchIsOff()
        {
            var off = new SheetCopyOptions(
                copyViews: false,
                copyLegends: false,
                copyText: false,
                copyImages: false,
                copyAnnotations: false,
                copyParameters: false);

            SheetCopyPlan plan = SheetViewPlanner.Plan(
                new[]
                {
                    new SourceViewFact("Ground floor", SheetViewKind.View),
                    new SourceViewFact("Keys", SheetViewKind.Legend),
                },
                off);

            Assert.All(plan.Views, planned => Assert.Equal(SheetViewAction.None, planned.Action));
        }

        [Fact]
        public void Plan_OfNoViewsIsEmpty()
        {
            SheetCopyPlan plan = SheetViewPlanner.Plan(null, new SheetCopyOptions());

            Assert.Empty(plan.Views);
        }

        [Fact]
        public void Plan_RejectsMissingOptions()
        {
            Assert.Throws<ArgumentNullException>(() => SheetViewPlanner.Plan(null, null!));
        }

        [Theory]
        [InlineData("Sheet Number", true)]
        [InlineData("SheetNumber", true)]
        [InlineData("Номер листа", true)]
        [InlineData("Guide Grid", true)]
        [InlineData("Направляющая сетка", true)]
        [InlineData("Comments", false)]
        [InlineData("Факт. площадь", false)]
        public void ShouldSkip_JudgesTheNameByItsWording(string parameterName, bool expected)
        {
            Assert.Equal(expected, ParameterCopyRules.ShouldSkip(parameterName));
        }

        [Fact]
        public void ShouldSkip_TreatsAMissingNameAsSkippable()
        {
            Assert.True(ParameterCopyRules.ShouldSkip(null));
            Assert.True(ParameterCopyRules.ShouldSkip("   "));
        }

        [Theory]
        [InlineData("Guide Grid", true)]
        [InlineData("Направляющая сетка", true)]
        [InlineData("Sheet Number", false)]
        [InlineData("Comments", false)]
        public void IsGuideGridReference_JudgesTheNameByItsWording(string parameterName, bool expected)
        {
            Assert.Equal(expected, ParameterCopyRules.IsGuideGridReference(parameterName));
        }
    }
}
