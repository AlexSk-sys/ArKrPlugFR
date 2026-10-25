using ArKrPlugFR.Core.HostChange;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class HostChangeRulesTests
    {
        [Fact]
        public void Route_ReHostsARealBar()
        {
            Assert.Equal(HostChangeAction.ChangeHost, HostChangeRules.Route(RebarKind.Bar));
        }

        [Fact]
        public void Route_UpdatesTheMarkOfAnIfcBar()
        {
            Assert.Equal(HostChangeAction.UpdateIfcMark, HostChangeRules.Route(RebarKind.IfcBar));
        }

        [Fact]
        public void Route_SkipsASystemBar()
        {
            Assert.Equal(HostChangeAction.Skip, HostChangeRules.Route(RebarKind.BarInSystem));
        }

        [Fact]
        public void Route_SkipsAnythingElse()
        {
            Assert.Equal(HostChangeAction.Skip, HostChangeRules.Route(RebarKind.Other));
        }
    }
}
