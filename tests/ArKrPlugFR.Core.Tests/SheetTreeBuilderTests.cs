using System.Collections.Generic;
using ArKrPlugFR.Core.Browser;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SheetTreeBuilderTests
    {
        [Fact]
        public void Build_NestsTheFoldersOfTheScheme()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "Architecture", "Plans" }, "A-101", "Ground floor", "A-101"),
                    Sheet(new[] { "Architecture" }, "A-001", "Title", "A-001"),
                    Sheet(new[] { "Structure" }, "K-001", "Title", "K-001"),
                },
                descending: false);

            Assert.Equal(2, tree.Groups.Count);
            Assert.Equal("Architecture", tree.Groups[0].Name);
            Assert.Single(tree.Groups[0].Groups);
            Assert.Equal("Plans", tree.Groups[0].Groups[0].Name);
            Assert.Equal(2, tree.Groups[0].SheetCount);
            Assert.Equal(1, tree.Groups[0].Groups[0].SheetCount);
            Assert.Equal(1, tree.Groups[1].SheetCount);
        }

        [Fact]
        public void Build_CountsTheSheetsAndGroupsOfTheTree()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "A", "A1" }, "S-001", "One", "1"),
                    Sheet(new[] { "A" }, "S-002", "Two", "2"),
                    Sheet(new string[0], "S-003", "Three", "3"),
                },
                descending: false);

            Assert.Equal(3, tree.SheetCount);
            Assert.Equal(2, tree.GroupCount);
        }

        [Fact]
        public void Build_KeepsASheetOfTheRootWithoutAGroup()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[] { Sheet(new string[0], "A-001", "Title", "A-001") },
                descending: false);

            Assert.Empty(tree.Groups);
            Assert.Single(tree.Sheets);
            Assert.Equal("A-001", tree.Sheets[0].Number);
        }

        [Fact]
        public void Build_OrdersTheGroupsByName()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "Walls" }, "A-001", "One", "A"),
                    Sheet(new[] { "Floors" }, "A-002", "Two", "B"),
                    Sheet(new[] { "Columns" }, "A-003", "Three", "C"),
                },
                descending: false);

            Assert.Equal("Columns", tree.Groups[0].Name);
            Assert.Equal("Floors", tree.Groups[1].Name);
            Assert.Equal("Walls", tree.Groups[2].Name);
        }

        [Fact]
        public void Build_OrdersTheSheetsOfAGroupByTheSortingValue()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "G" }, "A-003", "Third", "3"),
                    Sheet(new[] { "G" }, "A-001", "First", "1"),
                    Sheet(new[] { "G" }, "A-002", "Second", "2"),
                },
                descending: false);

            Assert.Equal("A-001", tree.Groups[0].Sheets[0].Number);
            Assert.Equal("A-002", tree.Groups[0].Sheets[1].Number);
            Assert.Equal("A-003", tree.Groups[0].Sheets[2].Number);
        }

        [Fact]
        public void Build_OrdersDescendingWhenTheSchemeSaysSo()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "G" }, "A-001", "First", "1"),
                    Sheet(new[] { "G" }, "A-002", "Second", "2"),
                },
                descending: true);

            Assert.Equal("A-002", tree.Groups[0].Sheets[0].Number);
            Assert.Equal("A-001", tree.Groups[0].Sheets[1].Number);
        }

        [Fact]
        public void Build_OrdersSheetsWithTheSameSortingValueByNumber()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[]
                {
                    Sheet(new[] { "G" }, "A-002", "Second", string.Empty),
                    Sheet(new[] { "G" }, "A-001", "First", string.Empty),
                },
                descending: false);

            Assert.Equal("A-001", tree.Groups[0].Sheets[0].Number);
            Assert.Equal("A-002", tree.Groups[0].Sheets[1].Number);
        }

        [Fact]
        public void Build_IgnoresEmptyFoldersOfTheScheme()
        {
            SheetTree tree = SheetTreeBuilder.Build(
                new[] { Sheet(new[] { " ", "Real" }, "A-001", "One", "A-001") },
                descending: false);

            Assert.Single(tree.Groups);
            Assert.Equal("Real", tree.Groups[0].Name);
        }

        [Fact]
        public void Build_WithNoSheets_ReturnsAnEmptyTree()
        {
            SheetTree tree = SheetTreeBuilder.Build(new List<SheetFact>(), descending: false);

            Assert.Empty(tree.Groups);
            Assert.Empty(tree.Sheets);
        }

        [Fact]
        public void Join_WritesThePathOfASheet()
        {
            string path = SheetPath.Join(new[] { "Architecture", "Plans" }, "A-101 \u2014 Ground floor");

            Assert.Equal("Architecture \u2192 Plans \u2192 A-101 \u2014 Ground floor", path);
        }

        [Fact]
        public void Join_WithoutFolders_WritesTheLabelAlone()
        {
            Assert.Equal("A-001 \u2014 Title", SheetPath.Join(new string[0], "A-001 \u2014 Title"));
        }

        [Fact]
        public void Join_SkipsEmptyFolders()
        {
            Assert.Equal("Real \u2192 A-001 \u2014 Title", SheetPath.Join(new[] { " ", "Real" }, "A-001 \u2014 Title"));
        }

        [Fact]
        public void Label_JoinsTheNumberAndTheName()
        {
            Assert.Equal("A-101 \u2014 Ground floor", Sheet(new string[0], "A-101", "Ground floor", "A-101").Label);
        }

        private static SheetFact Sheet(string[] folders, string number, string name, string sortKey)
        {
            return new SheetFact(folders, number, name, sortKey);
        }
    }
}
