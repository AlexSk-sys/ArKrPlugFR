using System.Collections.Generic;
using ArKrPlugFR.Core.Plugins;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class PluginManifestReaderTests
    {
        private const string ValidManifest = @"{
  ""id"": ""unjoin-elements"",
  ""assembly"": ""ArKrPlugFR.Plugins.UnjoinElements.dll"",
  ""entryType"": ""ArKrPlugFR.Plugins.UnjoinElements.UnjoinElementsCommand"",
  ""titles"": { ""en"": ""Unjoin elements"", ""ru"": ""Разъединить элементы"" },
  ""descriptions"": { ""en"": ""Removes joins inside the selection."", ""ru"": ""Убирает соединения внутри выделения."" },
  ""ribbon"": { ""tab"": ""ArKrPlugFR"", ""panel"": ""Elements"", ""buttonSize"": ""large"", ""order"": 10 },
  ""supportedRevitVersions"": [""2020"", ""2022"", ""2024""]
}";

        [Fact]
        public void Parse_ReadsEveryField()
        {
            PluginManifest manifest = PluginManifestReader.Parse(ValidManifest);

            Assert.Equal("unjoin-elements", manifest.Id);
            Assert.Equal("ArKrPlugFR.Plugins.UnjoinElements.dll", manifest.Assembly);
            Assert.Equal("ArKrPlugFR.Plugins.UnjoinElements.UnjoinElementsCommand", manifest.EntryType);
            Assert.Equal("Unjoin elements", manifest.GetTitle("en"));
            Assert.Equal("Разъединить элементы", manifest.GetTitle("ru"));
            Assert.Equal("ArKrPlugFR", manifest.Ribbon.Tab);
            Assert.Equal("Elements", manifest.Ribbon.Panel);
            Assert.Equal("large", manifest.Ribbon.ButtonSize);
            Assert.Equal(10, manifest.Ribbon.Order);
            Assert.True(manifest.SupportsRevitVersion("2022"));
        }

        [Fact]
        public void Parse_DefaultButtonSizeIsLarge()
        {
            const string json = @"{
  ""id"": ""x"", ""assembly"": ""a.dll"", ""entryType"": ""N.C"",
  ""titles"": { ""en"": ""X"" },
  ""ribbon"": { ""tab"": ""T"", ""panel"": ""P"" }
}";

            Assert.Equal("large", PluginManifestReader.Parse(json).Ribbon.ButtonSize);
        }

        [Fact]
        public void Parse_RejectsEmptyText()
        {
            Assert.Throws<PluginManifestException>(() => PluginManifestReader.Parse("   "));
        }

        [Fact]
        public void Parse_RejectsMalformedJson()
        {
            PluginManifestException exception = Assert.Throws<PluginManifestException>(
                () => PluginManifestReader.Parse("{ \"id\": "));

            Assert.Contains("malformed", exception.Reason);
        }

        [Fact]
        public void Parse_RejectsMissingId()
        {
            const string json = @"{ ""assembly"": ""a.dll"", ""entryType"": ""N.C"", ""titles"": { ""en"": ""X"" }, ""ribbon"": { ""tab"": ""T"", ""panel"": ""P"" } }";

            PluginManifestException exception = Assert.Throws<PluginManifestException>(
                () => PluginManifestReader.Parse(json));

            Assert.Contains("'id'", exception.Reason);
        }

        [Fact]
        public void Parse_RejectsMissingRibbon()
        {
            const string json = @"{ ""id"": ""x"", ""assembly"": ""a.dll"", ""entryType"": ""N.C"", ""titles"": { ""en"": ""X"" } }";

            PluginManifestException exception = Assert.Throws<PluginManifestException>(
                () => PluginManifestReader.Parse(json));

            Assert.Contains("'ribbon'", exception.Reason);
        }

        [Fact]
        public void Parse_RejectsUnknownButtonSize()
        {
            const string json = @"{ ""id"": ""x"", ""assembly"": ""a.dll"", ""entryType"": ""N.C"", ""titles"": { ""en"": ""X"" }, ""ribbon"": { ""tab"": ""T"", ""panel"": ""P"", ""buttonSize"": ""huge"" } }";

            Assert.Throws<PluginManifestException>(() => PluginManifestReader.Parse(json));
        }

        [Fact]
        public void Parse_TreatsMissingVersionListAsAllVersions()
        {
            const string json = @"{ ""id"": ""x"", ""assembly"": ""a.dll"", ""entryType"": ""N.C"", ""titles"": { ""en"": ""X"" }, ""ribbon"": { ""tab"": ""T"", ""panel"": ""P"" } }";

            PluginManifest manifest = PluginManifestReader.Parse(json);

            Assert.Empty(manifest.SupportedRevitVersions);
            Assert.True(manifest.SupportsRevitVersion("2019"));
        }

        [Fact]
        public void GetTitle_FallsBackToEnglishThenToAnyValue()
        {
            var titles = new Dictionary<string, string> { { "de", "Werkzeug" } };
            PluginManifest manifest = new PluginManifest(
                "x",
                "a.dll",
                "N.C",
                titles,
                new Dictionary<string, string>(),
                new RibbonPlacement("T", "P"));

            Assert.Equal("Werkzeug", manifest.GetTitle("ru"));
        }
    }
}
