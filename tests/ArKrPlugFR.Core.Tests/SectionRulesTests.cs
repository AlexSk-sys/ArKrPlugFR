using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Sections;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class SectionRulesTests
    {
        [Theory]
        [InlineData("Колонна 400х600 мм", "400×600 мм")]
        [InlineData("Балка 300 x 500", "300×500 мм")]
        [InlineData("Стена 200 мм", "200 мм")]
        [InlineData("Свая D20", "D20 мм")]
        [InlineData("Колонна 400х600 мм, бетон B25", "400×600 мм")]
        [InlineData("Плита 200,5", "200.5 мм")]
        public void Parse_ReadsTheSectionOfATypeName(string typeName, string expected)
        {
            Assert.Equal(expected, SectionNaming.Parse(typeName));
        }

        [Theory]
        [InlineData("Колонна монолитная")]
        [InlineData("")]
        [InlineData(null)]
        public void Parse_ReturnsNullWithoutNumbers(string? typeName)
        {
            Assert.Null(SectionNaming.Parse(typeName));
        }

        [Fact]
        public void Group_CountsTypesAndOrdersThem()
        {
            var rows = new List<SectionRow>
            {
                new SectionRow("2 этаж", "Колонны", "Колонна 400х400", "400×400 мм"),
                new SectionRow("1 этаж", "Колонны", "Колонна 400х400", "400×400 мм"),
                new SectionRow("1 этаж", "Колонны", "Колонна 400х400", "400×400 мм"),
                new SectionRow("1 этаж", "Плиты", "Плита 200", "200 мм"),
                new SectionRow("1 этаж", "Колонны", "Колонна 300х300", null)
            };

            IReadOnlyList<SectionLevelGroup> groups = SectionCollector.Group(rows);

            Assert.Equal(2, groups.Count);
            Assert.Equal("1 этаж", groups[0].Level);
            Assert.Equal(new[] { "Колонны", "Плиты" }, groups[0].Categories.Select(c => c.Category));

            SectionItem first = groups[0].Categories[0].Items[0];
            Assert.Equal("Колонна 300х300", first.TypeName);
            Assert.Null(first.Section);
            Assert.Equal(1, first.Count);

            SectionItem second = groups[0].Categories[0].Items[1];
            Assert.Equal(2, second.Count);
            Assert.Equal("400×400 мм", second.Section);
        }

        [Fact]
        public void Group_ReturnsNothingForNothing()
        {
            Assert.Empty(SectionCollector.Group(new List<SectionRow>()));
        }
    }
}
