using ArKrPlugFR.Core.Export;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class CsvRulesTests
    {
        [Fact]
        public void Escape_LeavesAPlainCellAlone()
        {
            Assert.Equal("Км-1", CsvRules.Escape("Км-1"));
        }

        [Fact]
        public void Escape_QuotesACellWithASeparator()
        {
            Assert.Equal("\"Бетон, B25\"", CsvRules.Escape("Бетон, B25"));
        }

        [Fact]
        public void Escape_DoublesAQuoteInsideACell()
        {
            Assert.Equal("\"a \"\"b\"\" c\"", CsvRules.Escape("a \"b\" c"));
        }

        [Fact]
        public void Escape_QuotesACellWithALineBreak()
        {
            Assert.Equal("\"one\ntwo\"", CsvRules.Escape("one\ntwo"));
        }

        [Fact]
        public void Escape_ReadsNullAsEmpty()
        {
            Assert.Equal(string.Empty, CsvRules.Escape(null));
        }

        [Fact]
        public void Row_JoinsTheCellsWithTheSeparator()
        {
            Assert.Equal("1,Wall,Бетон,12.5", CsvRules.Row(new[] { "1", "Wall", "Бетон", "12.5" }));
        }

        [Fact]
        public void Number_WritesADotWhateverTheMachineCulture()
        {
            Assert.Equal("12.5", CsvRules.Number(12.5d));
            Assert.Equal("250", CsvRules.Number(250d));
        }

        [Fact]
        public void Volume_ConvertsCubicFeetToCubicMeters()
        {
            Volume volume = Volume.FromCubicFeet(1d);

            Assert.Equal(0.028316846592d, volume.CubicMeters, 12);
            Assert.Equal(1d, volume.CubicFeet, 9);
        }

        [Fact]
        public void Volume_RoundTripsThroughCubicMeters()
        {
            Volume volume = Volume.FromCubicMeters(2.5d);

            Assert.Equal(2.5d, volume.CubicMeters, 12);
        }
    }
}
