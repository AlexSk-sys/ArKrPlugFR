using System.Collections.Generic;
using ArKrPlugFR.Core.Compare;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ParameterComparisonTests
    {
        [Fact]
        public void Build_ListsTheNamesOfBothSides()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?> { ["Марка"] = "Км-1", ["Только у источника"] = "x" },
                new Dictionary<string, string?> { ["Марка"] = "Км-2", ["Только у приёмника"] = "y" });

            Assert.Equal(3, rows.Count);
            Assert.Equal("Марка", rows[0].Name);
        }

        [Fact]
        public void Build_OrdersTheRowsByName()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?> { ["Я"] = "1", ["А"] = "2", ["М"] = "3" },
                new Dictionary<string, string?>());

            Assert.Equal(new[] { "А", "М", "Я" }, System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(rows, row => row.Name)));
        }

        [Fact]
        public void Build_MarksTheRowsWhoseValuesDiffer()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?> { ["Марка"] = "Км-1", ["Этаж"] = "3" },
                new Dictionary<string, string?> { ["Марка"] = "Км-2", ["Этаж"] = "3" });

            Assert.True(rows[0].Differs);
            Assert.False(rows[1].Differs);
        }

        [Fact]
        public void Build_IgnoresSurroundingSpacesWhenComparing()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?> { ["Марка"] = " Км-1 " },
                new Dictionary<string, string?> { ["Марка"] = "Км-1" });

            Assert.False(rows[0].Differs);
        }

        [Fact]
        public void Build_ReadsAnAbsentValueAsEmpty()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?> { ["Марка"] = null },
                new Dictionary<string, string?> { ["Марка"] = "Км-2" });

            Assert.Equal(string.Empty, rows[0].Source);
            Assert.Equal("Км-2", rows[0].Target);
            Assert.True(rows[0].Differs);
        }

        [Fact]
        public void Build_WithTwoEmptySides_HasNoDifference()
        {
            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(
                new Dictionary<string, string?>(),
                new Dictionary<string, string?>());

            Assert.Empty(rows);
        }

        [Fact]
        public void Build_OfTheSameValues_HasNoDifferences()
        {
            var values = new Dictionary<string, string?> { ["Марка"] = "Км-1", ["Этаж"] = "3" };

            IReadOnlyList<ParameterRow> rows = ParameterComparison.Build(values, values);

            Assert.All(rows, row => Assert.False(row.Differs));
        }
    }
}
