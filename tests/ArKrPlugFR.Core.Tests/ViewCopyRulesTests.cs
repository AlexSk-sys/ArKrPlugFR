using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Views;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ViewCopyRulesTests
    {
        [Fact]
        public void NextCopyName_AppendsAnUnderscoreAndTheNumber()
        {
            Assert.Equal("Plan_1", ViewCopyNames.NextCopyName("Plan", 1, _ => false));
            Assert.Equal("Plan_3", ViewCopyNames.NextCopyName("Plan", 3, _ => false));
        }

        [Fact]
        public void NextCopyName_KeepsCountingWhileTheNameIsTaken()
        {
            var taken = new HashSet<string> { "Plan_1", "Plan_2" };

            Assert.Equal("Plan_3", ViewCopyNames.NextCopyName("Plan", 1, taken.Contains));
        }

        [Fact]
        public void NextCopyName_StripsCharactersTheDocumentRefuses()
        {
            Assert.Equal("План1_1", ViewCopyNames.NextCopyName("План[1]", 1, _ => false));
        }

        [Fact]
        public void NextCopyName_ReadsANumberBelowOneAsOne()
        {
            Assert.Equal("Plan_1", ViewCopyNames.NextCopyName("Plan", 0, _ => false));
            Assert.Equal("Plan_1", ViewCopyNames.NextCopyName("Plan", -5, _ => false));
        }

        [Fact]
        public void NextCopyName_ReturnsNothingForAnEmptySourceName()
        {
            Assert.Equal(string.Empty, ViewCopyNames.NextCopyName(null, 1, _ => false));
            Assert.Equal(string.Empty, ViewCopyNames.NextCopyName("[]*", 1, _ => false));
        }

        [Fact]
        public void NextCopyName_RejectsAMissingUniquenessRule()
        {
            Assert.Throws<ArgumentNullException>(() => ViewCopyNames.NextCopyName("Plan", 1, null!));
        }

        [Theory]
        [InlineData("12", "12")]
        [InlineData("007", "7")]
        [InlineData("1a2b", "12")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void Normalize_KeepsDigitsOnly(string? typed, string expected)
        {
            Assert.Equal(expected, CopyCounter.Normalize(typed));
        }

        [Fact]
        public void Normalize_SnapsBackToTheCap()
        {
            Assert.Equal("99", CopyCounter.Normalize("150"));

            // A number too large to hold is still too large for the run.
            Assert.Equal("99", CopyCounter.Normalize("99999999999999999999"));
        }

        [Theory]
        [InlineData("5", 5)]
        [InlineData(null, 1)]
        [InlineData("", 1)]
        [InlineData("0", 1)]
        [InlineData("150", 99)]
        public void ValueOf_NeverLeavesTheAllowedRange(string? text, int expected)
        {
            Assert.Equal(expected, CopyCounter.ValueOf(text));
        }

        [Fact]
        public void IncreaseAndDecrease_StopAtTheEdges()
        {
            Assert.Equal("2", CopyCounter.Increase("1"));
            Assert.Equal("99", CopyCounter.Increase("99"));
            Assert.Equal("1", CopyCounter.Decrease("1"));
            Assert.Equal("1", CopyCounter.Decrease(null));
            Assert.Equal("1", CopyCounter.DefaultText);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Revision Schedule")]
        [InlineData("Штамп АР")]
        public void IsServedByName_LeavesThePlatformsOwnSchedulesAlone(string? name)
        {
            Assert.True(ScheduleRules.IsServedByName(name));
        }

        [Theory]
        [InlineData("Ведомость расхода стали")]
        [InlineData("Ключевая спецификация")]
        public void IsServedByName_KeepsOrdinarySchedules(string name)
        {
            Assert.False(ScheduleRules.IsServedByName(name));
        }

        [Theory]
        [InlineData("Ведомость расхода стали")]
        [InlineData("Sheet list")]
        [InlineData("Key schedule")]
        public void LooksLikeSchedule_RecognisesScheduleWording(string name)
        {
            Assert.True(ScheduleRules.LooksLikeSchedule(name));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("План 1 этажа")]
        [InlineData("Разрез 2-2")]
        public void LooksLikeSchedule_LeavesOtherNamesAlone(string? name)
        {
            Assert.False(ScheduleRules.LooksLikeSchedule(name));
        }

        [Theory]
        [InlineData(TemplateKind.None, TemplateKind.View, true)]
        [InlineData(TemplateKind.None, TemplateKind.Schedule, true)]
        [InlineData(TemplateKind.View, TemplateKind.View, true)]
        [InlineData(TemplateKind.View, TemplateKind.Schedule, false)]
        [InlineData(TemplateKind.View, TemplateKind.Legend, false)]
        [InlineData(TemplateKind.Schedule, TemplateKind.Schedule, true)]
        [InlineData(TemplateKind.Schedule, TemplateKind.View, false)]
        [InlineData(TemplateKind.Legend, TemplateKind.Legend, true)]
        [InlineData(TemplateKind.Legend, TemplateKind.View, false)]
        [InlineData(TemplateKind.Undefined, TemplateKind.View, false)]
        [InlineData(TemplateKind.View, TemplateKind.Undefined, false)]
        public void Fits_MatchesTheTemplateKindToTheElementKind(
            TemplateKind template,
            TemplateKind target,
            bool expected)
        {
            Assert.Equal(expected, TemplateRules.Fits(template, target));
        }

        [Fact]
        public void Terms_SplitsOnSpacesCommasAndSemicolons()
        {
            Assert.Equal(new[] { "план", "АР" }, TemplateRules.Terms("  план, АР ;; "));
            Assert.Empty(TemplateRules.Terms(null));
            Assert.Empty(TemplateRules.Terms("   "));
        }

        [Fact]
        public void Matches_KeepsACandidateWhenAnyTermOccurs()
        {
            Assert.True(TemplateRules.Matches("план АР", new[] { "Шаблон АР", "Поэтажные планы" }));
            Assert.False(TemplateRules.Matches("план АР", new[] { "Разрез", "Ведомость" }));
        }

        [Fact]
        public void Matches_KeepsEverythingWhenTheSearchIsEmpty()
        {
            Assert.True(TemplateRules.Matches(null, new[] { "Разрез" }));
            Assert.True(TemplateRules.Matches("  ", new[] { "Разрез" }));
        }
    }
}
