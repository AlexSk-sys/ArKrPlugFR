using System;
using ArKrPlugFR.Core.Localization;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class LocalizedTextTests
    {
        private static readonly LocalizedText Greeting = new LocalizedText("Hello", "Привет");

        [Fact]
        public void Resolve_ReturnsRussianForRu()
        {
            Assert.Equal("Привет", Greeting.Resolve("ru"));
            Assert.Equal("Привет", Greeting.Resolve("RU"));
        }

        [Fact]
        public void Resolve_FallsBackToEnglishForOtherCultures()
        {
            Assert.Equal("Hello", Greeting.Resolve("en"));
            Assert.Equal("Hello", Greeting.Resolve("de"));
            Assert.Equal("Hello", Greeting.Resolve(null));
            Assert.Equal("Hello", Greeting.Resolve(string.Empty));
        }

        [Fact]
        public void Resolve_FallsBackToEnglishWhenRussianIsEmpty()
        {
            var text = new LocalizedText("Only English", string.Empty);

            Assert.Equal("Only English", text.Resolve("ru"));
        }

        [Fact]
        public void Invariant_IsTheSameInEveryLanguage()
        {
            LocalizedText text = LocalizedText.Invariant("Revit 2022");

            Assert.Equal("Revit 2022", text.Resolve("ru"));
            Assert.Equal("Revit 2022", text.Resolve("en"));
        }

        [Fact]
        public void Constructor_RejectsEmptyEnglish()
        {
            Assert.Throws<ArgumentException>(() => new LocalizedText("  ", "непусто"));
        }

        [Fact]
        public void Resolve_FormatsWithInvariantSeparators()
        {
            var text = new LocalizedText("Value {0}", "Значение {0}");

            Assert.Equal("Значение 1.5", text.Resolve("ru", 1.5d));
            Assert.Equal("Value 1.5", text.Resolve("en", 1.5d));
        }

        [Fact]
        public void Resolve_WithoutArgumentsDoesNotFormat()
        {
            var text = new LocalizedText("100% {done}", "100% {done}");

            Assert.Equal("100% {done}", text.Resolve("en"));
        }

        [Fact]
        public void Equality_ComparesBothVariants()
        {
            Assert.Equal(new LocalizedText("Hello", "Привет"), Greeting);
            Assert.NotEqual(new LocalizedText("Hello", "Здравствуйте"), Greeting);
        }
    }
}
