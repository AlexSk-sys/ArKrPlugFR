using System.Collections.Generic;
using ArKrPlugFR.Core.Parameters;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class LayerTextRulesTests
    {
        [Fact]
        public void Format_NumberLayersAndNamesTheUnitInEnglish()
        {
            string text = LayerTextRules.Format(
                new[] { new LayerFact("Brick", 250d), new LayerFact("Insulation", 100d) },
                "en");

            Assert.Equal("1. Brick \u2014 250 mm\n2. Insulation \u2014 100 mm", text);
        }

        [Fact]
        public void Format_NamesTheUnitInRussian()
        {
            string text = LayerTextRules.Format(new[] { new LayerFact("Кирпич", 250d) }, "ru");

            Assert.Equal("1. \u041a\u0438\u0440\u043f\u0438\u0447 \u2014 250 \u043c\u043c", text);
        }

        [Fact]
        public void Format_PrintsAQuestionMarkForALayerWithoutMaterial()
        {
            string text = LayerTextRules.Format(new[] { new LayerFact(null, 40d) }, "en");

            Assert.Equal("1. ? \u2014 40 mm", text);
        }

        [Fact]
        public void Format_RoundsTheWidthToOneTenth()
        {
            string text = LayerTextRules.Format(new[] { new LayerFact("Plaster", 12.34d) }, "en");

            Assert.Equal("1. Plaster \u2014 12.3 mm", text);
        }

        [Fact]
        public void Format_WithNoLayers_IsEmpty()
        {
            Assert.Equal(string.Empty, LayerTextRules.Format(new List<LayerFact>(), "en"));
        }

        [Fact]
        public void Format_NumberingSkipsNothingWhenALayerIsMissing()
        {
            string text = LayerTextRules.Format(
                new[] { new LayerFact("Brick", 250d), null!, new LayerFact("Plaster", 10d) },
                "en");

            Assert.Equal("1. Brick \u2014 250 mm\n2. Plaster \u2014 10 mm", text);
        }
    }
}
