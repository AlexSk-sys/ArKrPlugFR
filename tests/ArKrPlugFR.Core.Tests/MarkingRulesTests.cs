using System.Collections.Generic;
using ArKrPlugFR.Core.Marking;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class MarkingRulesTests
    {
        [Fact]
        public void Detect_AStructuralColumn_IsAColumn()
        {
            Assert.Equal(StructureKind.Column, MarkingRules.Detect(Facts("OST_StructuralColumns")));
        }

        [Fact]
        public void Detect_AnArchitecturalColumn_IsAColumn()
        {
            Assert.Equal(StructureKind.Column, MarkingRules.Detect(Facts("OST_Columns")));
        }

        [Fact]
        public void Detect_StructuralFraming_IsABeam()
        {
            Assert.Equal(StructureKind.Beam, MarkingRules.Detect(Facts("OST_StructuralFraming")));
        }

        [Fact]
        public void Detect_FloorsAndWalls()
        {
            Assert.Equal(StructureKind.Floor, MarkingRules.Detect(Facts("OST_Floors")));
            Assert.Equal(StructureKind.Wall, MarkingRules.Detect(Facts("OST_Walls")));
        }

        [Fact]
        public void Detect_ABroadFoundation_IsAFoundation()
        {
            StructureFacts facts = new StructureFacts("OST_StructuralFoundation", null, 3000d, 3000d, 600d);

            Assert.Equal(StructureKind.Foundation, MarkingRules.Detect(facts));
        }

        [Fact]
        public void Detect_ADeepFoundation_IsAPile()
        {
            StructureFacts facts = new StructureFacts("OST_StructuralFoundation", null, 400d, 400d, 8000d);

            Assert.Equal(StructureKind.Pile, MarkingRules.Detect(facts));
        }

        [Fact]
        public void Detect_AnUnknownCategory_HasNoKind()
        {
            Assert.Null(MarkingRules.Detect(Facts("OST_Furniture")));
            Assert.Null(MarkingRules.Detect(Facts(null)));
            Assert.Null(MarkingRules.Detect(Facts("   ")));
        }

        [Fact]
        public void Material_FindsConcreteWords()
        {
            Assert.Equal(MaterialClass.Concrete, MarkingRules.Material(new[] { "Бетон B25" }));
            Assert.Equal(MaterialClass.Concrete, MarkingRules.Material(new[] { "Reinforced concrete" }));
            Assert.Equal(MaterialClass.Concrete, MarkingRules.Material(new[] { "Железобетон" }));
        }

        [Fact]
        public void Material_FindsSteelWords()
        {
            Assert.Equal(MaterialClass.Steel, MarkingRules.Material(new[] { "Сталь C245" }));
            Assert.Equal(MaterialClass.Steel, MarkingRules.Material(new[] { "Steel, 345" }));
            Assert.Equal(MaterialClass.Steel, MarkingRules.Material(new[] { "Металл" }));
        }

        [Fact]
        public void Material_ConcreteWinsWhenBothArePresent()
        {
            Assert.Equal(
                MaterialClass.Concrete,
                MarkingRules.Material(new[] { "Арматура A500", "Бетон B25" }));
        }

        [Fact]
        public void Material_UnknownNamesMakeNoFamily()
        {
            Assert.Equal(MaterialClass.None, MarkingRules.Material(new[] { "Кирпич" }));
            Assert.Equal(MaterialClass.None, MarkingRules.Material(new List<string>()));
            Assert.Equal(MaterialClass.None, MarkingRules.Material(null));
        }

        [Fact]
        public void Mark_AppendsTheSuffixOfTheMaterialFamily()
        {
            var settings = new MarkingSettings();

            Assert.Equal("Км", MarkingRules.Mark(settings, StructureKind.Column, MaterialClass.Concrete));
            Assert.Equal("Бст", MarkingRules.Mark(settings, StructureKind.Beam, MaterialClass.Steel));
            Assert.Equal("П", MarkingRules.Mark(settings, StructureKind.Floor, MaterialClass.None));
        }

        [Fact]
        public void Mark_WithAnEmptyCode_IsEmpty()
        {
            var settings = new MarkingSettings { WallMark = "  " };

            Assert.Equal(string.Empty, MarkingRules.Mark(settings, StructureKind.Wall, MaterialClass.Concrete));
        }

        [Fact]
        public void Mark_WithoutASuffix_AppendsNothing()
        {
            var settings = new MarkingSettings { ConcreteSuffix = " " };

            Assert.Equal("Ф", MarkingRules.Mark(settings, StructureKind.Foundation, MaterialClass.Concrete));
        }

        [Fact]
        public void Settings_StartWithTheDefaultCodesAndSuffixes()
        {
            var settings = new MarkingSettings();

            Assert.True(settings.OnlyEmpty);
            Assert.Equal("К", settings.MarkOf(StructureKind.Column));
            Assert.Equal("Б", settings.MarkOf(StructureKind.Beam));
            Assert.Equal("П", settings.MarkOf(StructureKind.Floor));
            Assert.Equal("СТ", settings.MarkOf(StructureKind.Wall));
            Assert.Equal("Ф", settings.MarkOf(StructureKind.Foundation));
            Assert.Equal("С", settings.MarkOf(StructureKind.Pile));
            Assert.Equal("м", settings.SuffixOf(MaterialClass.Concrete));
            Assert.Equal("ст", settings.SuffixOf(MaterialClass.Steel));
            Assert.Equal(string.Empty, settings.SuffixOf(MaterialClass.None));
        }

        private static StructureFacts Facts(string? identifier)
        {
            return new StructureFacts(identifier, null, 400d, 400d, 3000d);
        }
    }
}
