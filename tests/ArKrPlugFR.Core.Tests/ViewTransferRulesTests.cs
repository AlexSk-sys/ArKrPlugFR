using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.ViewTransfer;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ViewTransferRulesTests
    {
        private static ViewCandidate View(
            long id,
            string name,
            string viewTypeName,
            bool isTemplate = false,
            bool isValid = true,
            bool isSheet = false,
            bool isSystemSchedule = false)
        {
            return new ViewCandidate(id, name, viewTypeName, isTemplate, isValid, isSheet, isSystemSchedule);
        }

        [Fact]
        public void Rules_AllowAViewTemplate()
        {
            Assert.True(ViewTransferRules.IsTransferable(View(1L, "Plan A", "FloorPlan", isTemplate: true)));
        }

        [Fact]
        public void Rules_AllowAnOrdinaryView()
        {
            Assert.True(ViewTransferRules.IsTransferable(View(1L, "Level 1", "FloorPlan")));
        }

        [Fact]
        public void Rules_AllowAnOrdinarySchedule()
        {
            Assert.True(ViewTransferRules.IsTransferable(View(1L, "Door Schedule", "Schedule")));
        }

        [Fact]
        public void Rules_RejectASheetByFlag()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Sheet 1", "DrawingSheet", isSheet: true)));
        }

        [Fact]
        public void Rules_RejectASheetByTypeName()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Sheet 1", "DrawingSheet")));
        }

        [Fact]
        public void Rules_RejectAPanelSchedule()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Panel", "PanelSchedule")));
        }

        [Fact]
        public void Rules_RejectAColumnSchedule()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Columns", "ColumnSchedule")));
        }

        [Fact]
        public void Rules_RejectASystemSchedule()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Revision", "Schedule", isSystemSchedule: true)));
        }

        [Fact]
        public void Rules_RejectAnInvalidElement()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Gone", "FloorPlan", isValid: false)));
        }

        [Fact]
        public void Rules_RejectInternalViewTypes()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Internal", "Internal")));
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Undefined", "Undefined")));
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Revit", "FirstRev")));
        }

        [Fact]
        public void Rules_IgnoreTheCaseOfTheViewTypeName()
        {
            Assert.False(ViewTransferRules.IsTransferable(View(1L, "Panel", "panelschedule")));
            Assert.True(ViewTransferRules.IsTransferable(View(1L, "Plan", "floorplan")));
        }

        [Fact]
        public void Rules_RejectNull()
        {
            Assert.Throws<ArgumentNullException>(() => ViewTransferRules.IsTransferable(null!));
        }

        [Fact]
        public void Catalog_GroupsPlanTypesTogether()
        {
            Assert.Equal(ViewGroupCatalog.PlansKey, GroupKey("FloorPlan"));
            Assert.Equal(ViewGroupCatalog.PlansKey, GroupKey("StructuralPlan"));
            Assert.Equal(ViewGroupCatalog.PlansKey, GroupKey("AreaPlan"));
        }

        [Fact]
        public void Catalog_GroupsCeilingPlansApart()
        {
            Assert.Equal(ViewGroupCatalog.CeilingPlansKey, GroupKey("CeilingPlan"));
        }

        [Fact]
        public void Catalog_GroupsWalkthroughsWith3DViews()
        {
            Assert.Equal(ViewGroupCatalog.ThreeDimensionalKey, GroupKey("ThreeD"));
            Assert.Equal(ViewGroupCatalog.ThreeDimensionalKey, GroupKey("Walkthrough"));
        }

        [Fact]
        public void Catalog_GroupsSectionsElevationsLegendsAndDetailsTogether()
        {
            Assert.Equal(ViewGroupCatalog.SectionsKey, GroupKey("Elevation"));
            Assert.Equal(ViewGroupCatalog.SectionsKey, GroupKey("Section"));
            Assert.Equal(ViewGroupCatalog.SectionsKey, GroupKey("Legend"));
            Assert.Equal(ViewGroupCatalog.SectionsKey, GroupKey("Detail"));
        }

        [Fact]
        public void Catalog_MapsTheRemainingKnownTypes()
        {
            Assert.Equal(ViewGroupCatalog.DraftingKey, GroupKey("DraftingView"));
            Assert.Equal(ViewGroupCatalog.EngineeringKey, GroupKey("EngineeringPlan"));
            Assert.Equal(ViewGroupCatalog.SchedulesKey, GroupKey("Schedule"));
        }

        [Fact]
        public void Catalog_HasNoMappingForAnUnknownType()
        {
            Assert.False(ViewGroupCatalog.TryGetGroupKey("CostReport", out string key));
            Assert.Equal(string.Empty, key);
        }

        [Fact]
        public void Naming_NormalizesWhitespace()
        {
            Assert.Equal("Plan A", ViewNaming.Normalize("  Plan A  "));
        }

        [Fact]
        public void Naming_FallsBackWhenTheNameIsEmpty()
        {
            Assert.Equal(ViewNaming.FallbackName, ViewNaming.Normalize("   "));
            Assert.Equal(ViewNaming.FallbackName, ViewNaming.Normalize(null));
        }

        [Fact]
        public void Naming_KeepsAFreeName()
        {
            Assert.Equal("Plan A", ViewNaming.MakeUnique("Plan A", Taken("Level 1", "Level 2")));
        }

        [Fact]
        public void Naming_AddsASuffixOnCollision()
        {
            Assert.Equal("Plan A (2)", ViewNaming.MakeUnique("Plan A", Taken("Plan A")));
        }

        [Fact]
        public void Naming_CountsUpPastSeveralCollisions()
        {
            Assert.Equal("Plan A (3)", ViewNaming.MakeUnique("Plan A", Taken("Plan A", "Plan A (2)")));
        }

        [Fact]
        public void Naming_IgnoresCase()
        {
            Assert.Equal("plan a (2)", ViewNaming.MakeUnique("plan a", Taken("PLAN A")));
        }

        [Fact]
        public void Naming_RejectsNullTakenNames()
        {
            Assert.Throws<ArgumentNullException>(() => ViewNaming.MakeUnique("Plan A", null!));
            Assert.Throws<ArgumentNullException>(() => ViewNaming.IsTaken("Plan A", null!));
        }

        [Fact]
        public void Planner_DropsViewsThatAreNotTransferable()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "Level 1", "FloorPlan"),
                View(2L, "Sheet 1", "DrawingSheet", isSheet: true),
                View(3L, "Gone", "FloorPlan", isValid: false),
            });

            Assert.Equal(1, plan.TotalCount);
            Assert.Equal("Level 1", Assert.Single(plan.Groups).Items[0].Name);
        }

        [Fact]
        public void Planner_KeepsScheduleTemplates()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "Doors", "Schedule", isTemplate: true),
            });

            TransferItem item = Assert.Single(Assert.Single(plan.Groups).Items);
            Assert.True(item.IsTemplate);
        }

        [Fact]
        public void Planner_SplitsUnknownViewTypesIntoTheirOwnGroups()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "Report A", "CostReport"),
                View(2L, "Report B", "CostReport"),
                View(3L, "Something", "SystemsAnalysisReport"),
            });

            Assert.Equal(2, plan.Groups.Count);
            Assert.Equal(2, plan.Groups.Single(group => group.FallbackViewTypeName == "CostReport").ItemCount);
            Assert.Equal(
                1,
                plan.Groups.Single(group => group.FallbackViewTypeName == "SystemsAnalysisReport").ItemCount);
        }

        [Fact]
        public void Planner_SortsItemsByNameInsideAGroup()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "beta", "FloorPlan"),
                View(2L, "Alpha", "FloorPlan"),
                View(3L, "gamma", "FloorPlan"),
            });

            Assert.Equal(new[] { "Alpha", "beta", "gamma" }, NamesOf(Assert.Single(plan.Groups)));
        }

        [Fact]
        public void Planner_PlansNamesThatAreUniqueInTheTarget()
        {
            TransferPlan plan = TransferPlanner.Create(
                new[] { View(1L, "Level 1", "FloorPlan") },
                new[] { "LEVEL 1" });

            Assert.Equal("Level 1 (2)", Assert.Single(Assert.Single(plan.Groups).Items).PlannedName);
        }

        [Fact]
        public void Planner_PlansUniqueNamesForTwoCopiesOfOneName()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "Level 1", "FloorPlan"),
                View(2L, "Level 1", "CeilingPlan"),
            });

            var planned = plan.Groups
                .SelectMany(group => group.Items)
                .Select(item => item.PlannedName)
                .ToList();

            Assert.Equal(2, planned.Distinct().Count());
        }

        [Fact]
        public void Planner_IgnoresAnElementIdTwice()
        {
            TransferPlan plan = TransferPlanner.Create(new[]
            {
                View(1L, "Level 1", "FloorPlan"),
                View(1L, "Level 1", "FloorPlan"),
            });

            Assert.Equal(1, plan.TotalCount);
        }

        [Fact]
        public void Planner_OfNothingIsEmpty()
        {
            Assert.True(TransferPlanner.Create(Array.Empty<ViewCandidate>()).IsEmpty);
        }

        [Fact]
        public void Planner_RejectsNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => TransferPlanner.Create(null!));
        }

        [Fact]
        public void Group_RejectsBothKeyAndFallback()
        {
            Assert.Throws<ArgumentException>(
                () => new TransferGroup("plans", "FloorPlan", Array.Empty<TransferItem>()));
        }

        [Fact]
        public void Group_RejectsNeitherKeyNorFallback()
        {
            Assert.Throws<ArgumentException>(() => new TransferGroup(null, null, Array.Empty<TransferItem>()));
        }

        private static string? GroupKey(string viewTypeName)
        {
            return ViewGroupCatalog.TryGetGroupKey(viewTypeName, out string key) ? key : null;
        }

        private static HashSet<string> Taken(params string[] names)
        {
            return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        }

        private static string[] NamesOf(TransferGroup group)
        {
            return group.Items.Select(item => item.Name).ToArray();
        }
    }
}
