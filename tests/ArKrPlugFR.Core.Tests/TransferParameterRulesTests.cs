using ArKrPlugFR.Core.Parameters;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class TransferParameterRulesTests
    {
        [Fact]
        public void Normalize_TrimsAValue()
        {
            Assert.Equal("Ар-1", TransferValue.Normalize("  Ар-1  "));
        }

        [Fact]
        public void Normalize_TreatsSpacesAsNothingToTransfer()
        {
            Assert.Null(TransferValue.Normalize("   "));
            Assert.Null(TransferValue.Normalize(null));
        }

        [Fact]
        public void IsUsable_RejectsAnEmptyName()
        {
            Assert.True(ParameterNameRules.IsUsable("ADSK_Раздел"));
            Assert.False(ParameterNameRules.IsUsable("  "));
            Assert.False(ParameterNameRules.IsUsable(null));
        }

        [Fact]
        public void Matches_IgnoresCaseAndSurroundingSpaces()
        {
            Assert.True(ParameterNameRules.Matches(" adsk_section ", "ADSK_Section"));
            Assert.False(ParameterNameRules.Matches("ADSK_Section", "ADSK_Stage"));
            Assert.False(ParameterNameRules.Matches(null, "ADSK_Section"));
        }

        [Fact]
        public void RoomSpreadOptions_AnySelectedNeedsAtLeastOneReceiver()
        {
            var none = new RoomSpreadOptions { Walls = false, Floors = false, Ceilings = false, Rooms = false };
            var one = new RoomSpreadOptions { Walls = false, Floors = false, Ceilings = false, Rooms = true };

            Assert.False(none.AnySelected);
            Assert.True(one.AnySelected);
        }

        [Fact]
        public void RoomSpreadOptions_StartsWithTheStructuresTicked()
        {
            var options = new RoomSpreadOptions();

            Assert.True(options.Walls);
            Assert.True(options.Floors);
            Assert.True(options.Ceilings);
            Assert.False(options.Rooms);
        }
    }
}
