using System.Collections.Generic;
using ArKrPlugFR.Core.Worksets;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class WorksetRulesTests
    {
        [Fact]
        public void SelectTarget_MatchesTheCategoryShownInRevit()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.Category, "Walls", "Architecture") },
                Facts(category: "Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_MatchesTheBuiltInCategoryName()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.Category, "OST_Walls", "Architecture") },
                Facts(category: "Стены", identifier: "OST_Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_MatchesAnyEntryOfACategoryList()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.Category, "Floors; Walls", "Architecture") },
                Facts(category: "Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_IgnoresCase()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.Category, "walls", "Architecture") },
                Facts(category: "WALLS"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_MatchesAFamilyPrefix()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.FamilyPrefix, "201_", "Concrete") },
                Facts(family: "201_Concrete rectangular"));

            Assert.Equal("Concrete", target);
        }

        [Fact]
        public void SelectTarget_MatchesAnyEntryOfAFamilyPrefixList()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.FamilyPrefix, "201_,202_, 204_", "Concrete") },
                Facts(family: "204_Pile cap"));

            Assert.Equal("Concrete", target);
        }

        [Fact]
        public void SelectTarget_LeavesAnUnrelatedFamilyAlone()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.FamilyPrefix, "201_", "Concrete") },
                Facts(family: "305_Topography"));

            Assert.Null(target);
        }

        [Fact]
        public void SelectTarget_MatchesATypePrefix()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.TypePrefix, "100_", "Architecture") },
                Facts(type: "100_Interior partition 200"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_DoesNotMatchATypePrefixWhenThereIsNoType()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.TypePrefix, "100_", "Architecture") },
                Facts());

            Assert.Null(target);
        }

        [Fact]
        public void SelectTarget_TakesTheWorksetFromTheParameterValue()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.ParameterValue, "WorksetNameForPlugin", "ignored") },
                Facts(parameters: new (string, string?)[] { ("WorksetNameForPlugin", "  Piles  ") }));

            Assert.Equal("Piles", target);
        }

        [Fact]
        public void SelectTarget_FallsThroughWhenTheParameterIsEmpty()
        {
            var rules = new[]
            {
                new WorksetRule(WorksetRuleKinds.ParameterValue, "WorksetName", "ignored"),
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "Architecture"),
            };

            string? target = WorksetRules.SelectTarget(rules, Facts(category: "Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_ReturnsNothingWhenOnlyAnEmptyParameterRuleApplies()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.ParameterValue, "WorksetName", "ignored") },
                Facts(parameters: new (string, string?)[] { ("WorksetName", "   ") }));

            Assert.Null(target);
        }

        [Fact]
        public void SelectTarget_KeepsTheOrderOfTheRules()
        {
            var rules = new[]
            {
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "First"),
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "Second"),
            };

            string? target = WorksetRules.SelectTarget(rules, Facts(category: "Walls"));

            Assert.Equal("First", target);
        }

        [Fact]
        public void SelectTarget_PrefersANarrowRulePlacedFirst()
        {
            var rules = new[]
            {
                new WorksetRule(WorksetRuleKinds.FamilyPrefix, "201_", "Concrete"),
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "Architecture"),
            };

            string? target = WorksetRules.SelectTarget(rules, Facts(category: "Walls", family: "201_Panel"));

            Assert.Equal("Concrete", target);
        }

        [Fact]
        public void SelectTarget_SkipsAnUnknownKind()
        {
            var rules = new[]
            {
                new WorksetRule("linked-file", "*.rvt", "Links"),
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "Architecture"),
            };

            string? target = WorksetRules.SelectTarget(rules, Facts(category: "Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void SelectTarget_IgnoresAnEmptyPattern()
        {
            string? target = WorksetRules.SelectTarget(
                new[] { new WorksetRule(WorksetRuleKinds.Category, "   ", "Architecture") },
                Facts(category: "Walls"));

            Assert.Null(target);
        }

        [Fact]
        public void SelectTarget_SkipsAMatchingRuleWithoutATarget()
        {
            var rules = new[]
            {
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "  "),
                new WorksetRule(WorksetRuleKinds.Category, "Walls", "Architecture"),
            };

            string? target = WorksetRules.SelectTarget(rules, Facts(category: "Walls"));

            Assert.Equal("Architecture", target);
        }

        [Fact]
        public void ParameterNames_ReturnsTheDistinctNamesOfUsableParameterRules()
        {
            var rules = new[]
            {
                new WorksetRule(WorksetRuleKinds.ParameterValue, "WorksetName", string.Empty),
                new WorksetRule(WorksetRuleKinds.ParameterValue, "  worksetname  ", string.Empty),
                new WorksetRule(WorksetRuleKinds.ParameterValue, "Section", string.Empty),
                new WorksetRule(WorksetRuleKinds.Category, "Section", "Misc"),
                new WorksetRule(WorksetRuleKinds.ParameterValue, "   ", string.Empty),
            };

            IReadOnlyList<string> names = WorksetRules.ParameterNames(rules);

            Assert.Equal(new[] { "WorksetName", "Section" }, names);
        }

        [Fact]
        public void Rule_IsUnusableWithoutAPattern()
        {
            Assert.False(new WorksetRule(WorksetRuleKinds.Category, " ", "Architecture").IsUsable);
        }

        [Fact]
        public void Rule_IsUnusableWithoutATargetUnlessItReadsOneFromAParameter()
        {
            Assert.False(new WorksetRule(WorksetRuleKinds.Category, "Walls", string.Empty).IsUsable);
            Assert.True(new WorksetRule(WorksetRuleKinds.ParameterValue, "WorksetName", string.Empty).IsUsable);
        }

        [Fact]
        public void Rule_IsUnusableWithAnUnknownKind()
        {
            Assert.False(new WorksetRule("linked-file", "*.rvt", "Links").IsUsable);
        }

        [Fact]
        public void Rule_IsUsableWithDefaultValuesFromSettings()
        {
            var stored = new WorksetRule();

            Assert.Equal(WorksetRuleKinds.Category, stored.Kind);
            Assert.False(stored.IsUsable);
        }

        [Fact]
        public void Normalize_TrimsAValueAndTreatsSpacesAsAbsent()
        {
            Assert.Equal("Walls", WorksetRules.Normalize("  Walls  "));
            Assert.Null(WorksetRules.Normalize("   "));
            Assert.Null(WorksetRules.Normalize(null));
        }

        private static WorksetElementFacts Facts(
            string? category = null,
            string? identifier = null,
            string? family = null,
            string? type = null,
            (string Name, string? Value)[]? parameters = null)
        {
            var values = new Dictionary<string, string?>();

            if (parameters != null)
            {
                foreach ((string name, string? value) in parameters)
                {
                    values[name] = value;
                }
            }

            return new WorksetElementFacts(category, identifier, family, type, values);
        }
    }
}
