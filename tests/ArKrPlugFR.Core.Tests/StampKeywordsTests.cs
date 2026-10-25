using System.Collections.Generic;
using ArKrPlugFR.Core.Sheets;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class StampKeywordsTests
    {
        [Theory]
        [InlineData("Штамп АР")]
        [InlineData("Company stamp")]
        [InlineData("Title block")]
        [InlineData("Оформление_Титул")]
        [InlineData("основная надпись листа")]
        public void LooksLikeStamp_RecognisesStampWording(string name)
        {
            Assert.True(StampKeywords.LooksLikeStamp(new[] { name }));
        }

        [Theory]
        [InlineData("Ось 1")]
        [InlineData("Разрез 2-2")]
        [InlineData("Ведомость расхода стали")]
        [InlineData("View 1")]
        public void LooksLikeStamp_LeavesOtherNamesAlone(string name)
        {
            Assert.False(StampKeywords.LooksLikeStamp(new[] { name }));
        }

        [Fact]
        public void LooksLikeStamp_ReadsFamilyTypeAndElementNames()
        {
            var names = new List<string?> { "Обобщённая аннотация", null, "Титул_А3" };

            Assert.True(StampKeywords.LooksLikeStamp(names));
        }

        [Fact]
        public void LooksLikeStamp_RejectsMissingNames()
        {
            Assert.False(StampKeywords.LooksLikeStamp(null));
            Assert.False(StampKeywords.LooksLikeStamp(new string?[] { null, string.Empty }));
            Assert.False(StampKeywords.LooksLikeStamp(new string[0]));
        }

        [Fact]
        public void LooksLikeStamp_IgnoresLetterCase()
        {
            Assert.True(StampKeywords.LooksLikeStamp(new[] { "STAMP_A3" }));
            Assert.True(StampKeywords.LooksLikeStamp(new[] { "ТИТУЛ" }));
        }
    }
}
