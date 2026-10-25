using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Concrete;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class ConcreteRulesTests
    {
        [Theory]
        [InlineData("Concrete wall 200")]
        [InlineData("Бетон В25")]
        [InlineData("железобетон")]
        [InlineData("CAST-IN-PLACE CONCRETE")]
        public void IsConcreteName_RecognisesTheWordInAnyCase(string name)
        {
            Assert.True(ConcreteRules.IsConcreteName(name));
        }

        [Theory]
        [InlineData("Brick 250")]
        [InlineData("Steel beam")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsConcreteName_RejectsEverythingElse(string? name)
        {
            Assert.False(ConcreteRules.IsConcreteName(name));
        }

        [Fact]
        public void AnyConcrete_IsTrueWhenTheMaterialCarriesTheWord()
        {
            var names = new List<string?> { "Стена отделочная", "Кирпич", "Бетон тяжёлый" };

            Assert.True(ConcreteRules.AnyConcrete(names));
        }

        [Fact]
        public void AnyConcrete_IsFalseForNothing()
        {
            Assert.False(ConcreteRules.AnyConcrete(null));
            Assert.False(ConcreteRules.AnyConcrete(new List<string?>()));
        }

        [Fact]
        public void ServiceAxis_FollowsTheLongestSide()
        {
            Assert.Equal(CutAxis.X, ConcreteRules.ServiceAxis(Box(3000d, 400d, 300d)));
            Assert.Equal(CutAxis.Y, ConcreteRules.ServiceAxis(Box(400d, 3000d, 300d)));
            Assert.Equal(CutAxis.Z, ConcreteRules.ServiceAxis(Box(400d, 300d, 3000d)));
        }

        [Fact]
        public void ServiceAxis_ResolvesTiesTowardsXThenY()
        {
            Assert.Equal(CutAxis.X, ConcreteRules.ServiceAxis(Box(500d, 500d, 500d)));
            Assert.Equal(CutAxis.Y, ConcreteRules.ServiceAxis(Box(400d, 500d, 500d)));
        }

        [Fact]
        public void PassesThrough_AcceptsACrossingThatCoversTheThickness()
        {
            Assert.True(ConcreteRules.PassesThrough(200d, 200d, ConcreteRules.PassTolerance));
            Assert.True(ConcreteRules.PassesThrough(260d, 200d, ConcreteRules.PassTolerance));
            Assert.True(ConcreteRules.PassesThrough(199.5d, 200d, ConcreteRules.PassTolerance));
        }

        [Fact]
        public void PassesThrough_RejectsACrossingThatStopsInside()
        {
            Assert.False(ConcreteRules.PassesThrough(120d, 200d, ConcreteRules.PassTolerance));
            Assert.False(ConcreteRules.PassesThrough(0d, 200d, ConcreteRules.PassTolerance));
        }

        [Fact]
        public void PassesThrough_RejectsAnElementWithoutThickness()
        {
            Assert.False(ConcreteRules.PassesThrough(50d, 0d, ConcreteRules.PassTolerance));
        }

        [Fact]
        public void PassesThrough_ReadsANegativeToleranceAsNone()
        {
            Assert.False(ConcreteRules.PassesThrough(199d, 200d, Length.FromMillimeters(-5d)));
            Assert.True(ConcreteRules.PassesThrough(200d, 200d, Length.FromMillimeters(-5d)));
        }

        private static BoundingBox Box(double width, double depth, double height)
        {
            return new BoundingBox(
                new Point3(0d, 0d, 0d),
                new Point3(width, depth, height));
        }
    }
}
