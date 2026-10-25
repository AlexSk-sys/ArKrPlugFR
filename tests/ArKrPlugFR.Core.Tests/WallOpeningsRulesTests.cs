using System;
using System.IO;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class WallOpeningsRulesTests
    {
        [Fact]
        public void RunsAlike_AcceptsTheSameDirectionAndTheOppositeOne()
        {
            var direction = new Point3(1d, 0d, 0d);

            Assert.True(WallAdjacency.RunsAlike(direction, new Point3(2d, 0d, 0d)));
            Assert.True(WallAdjacency.RunsAlike(direction, new Point3(-3d, 0d, 0d)));
        }

        [Fact]
        public void RunsAlike_RefusesAWallRunningTheOtherWay()
        {
            var direction = new Point3(1d, 0d, 0d);

            Assert.False(WallAdjacency.RunsAlike(direction, new Point3(0d, 1d, 0d)));
            Assert.False(WallAdjacency.RunsAlike(direction, new Point3(1d, 1d, 0d)));
        }

        [Fact]
        public void RunsAlike_RefusesADirectionWithoutLength()
        {
            Assert.False(WallAdjacency.RunsAlike(new Point3(0d, 0d, 0d), new Point3(1d, 0d, 0d)));
            Assert.False(WallAdjacency.RunsAlike(new Point3(1d, 0d, 0d), new Point3(0d, 0d, 0d)));
        }

        [Fact]
        public void Gap_MeasuresAcrossTheWall()
        {
            // A wall running along X: a point 150 mm up the Y axis stands 150 mm across it.
            Length gap = WallAdjacency.Gap(
                new Point3(0d, 0d, 0d),
                new Point3(1d, 0d, 0d),
                new Point3(5000d, 150d, 0d));

            Assert.Equal(150d, gap.Millimeters, 3);
        }

        [Fact]
        public void Gap_ReadsEitherSideAsTheSameDistance()
        {
            Length above = WallAdjacency.Gap(
                new Point3(0d, 0d, 0d),
                new Point3(1d, 0d, 0d),
                new Point3(0d, -80d, 0d));

            Assert.Equal(80d, above.Millimeters, 3);
        }

        [Fact]
        public void Gap_RejectsADirectionWithoutLength()
        {
            Assert.Throws<ArgumentException>(() => WallAdjacency.Gap(
                new Point3(0d, 0d, 0d),
                new Point3(0d, 0d, 0d),
                new Point3(0d, 10d, 0d)));
        }

        [Fact]
        public void Lines_KeepsWallsInsideTheGapAndDropsTheOthers()
        {
            var basePoint = new Point3(0d, 0d, 0d);
            var direction = new Point3(1d, 0d, 0d);

            Assert.True(WallAdjacency.Lines(
                basePoint,
                direction,
                new Point3(0d, 100d, 0d),
                new Point3(1d, 0d, 0d),
                WallAdjacency.DefaultGap));

            Assert.False(WallAdjacency.Lines(
                basePoint,
                direction,
                new Point3(0d, 400d, 0d),
                new Point3(1d, 0d, 0d),
                WallAdjacency.DefaultGap));

            // Far away but running across: still not a wall that lines this one.
            Assert.False(WallAdjacency.Lines(
                basePoint,
                direction,
                new Point3(0d, 100d, 0d),
                new Point3(0d, 1d, 0d),
                WallAdjacency.DefaultGap));
        }

        [Fact]
        public void Coincides_ComparesEveryEdgeWithinTheTolerance()
        {
            var first = new OpeningRectangle(1000d, 1900d, 0d, 2100d);
            var samePlace = new OpeningRectangle(1005d, 1896d, 3d, 2098d);
            var elsewhere = new OpeningRectangle(3000d, 3900d, 0d, 2100d);

            Length tolerance = Length.FromMillimeters(10d);

            Assert.True(OpeningRules.Coincides(first, samePlace, tolerance));
            Assert.False(OpeningRules.Coincides(first, elsewhere, tolerance));
        }

        [Fact]
        public void Coincides_ReadsANegativeToleranceAsNone()
        {
            var first = new OpeningRectangle(0d, 900d, 0d, 2100d);
            var second = new OpeningRectangle(5d, 900d, 0d, 2100d);

            Assert.False(OpeningRules.Coincides(first, second, Length.FromMillimeters(-5d)));
            Assert.True(OpeningRules.Coincides(first, first, Length.FromMillimeters(-5d)));
        }

        [Fact]
        public void ToolSettings_RoundTripsThroughTheFile()
        {
            string directory = NewDirectory();

            try
            {
                var stored = new SampleSettings { Doors = false, Windows = true };

                Assert.True(ToolSettings.Save(directory, "wall-openings", stored));

                SampleSettings loaded = ToolSettings.Load(directory, "wall-openings", new SampleSettings());

                Assert.False(loaded.Doors);
                Assert.True(loaded.Windows);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void ToolSettings_FallsBackWhenThereIsNoFile()
        {
            string directory = NewDirectory();

            try
            {
                SampleSettings loaded = ToolSettings.Load(directory, "wall-openings", new SampleSettings());

                Assert.True(loaded.Doors);
                Assert.True(loaded.Windows);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void ToolSettings_FallsBackOnCorruptContent()
        {
            string directory = NewDirectory();

            try
            {
                File.WriteAllText(ToolSettings.PathOf(directory, "wall-openings"), "{ this is not json");

                SampleSettings loaded = ToolSettings.Load(directory, "wall-openings", new SampleSettings());

                Assert.True(loaded.Doors);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void ToolSettings_IgnoresAWriteItCannotMake()
        {
            // A directory that cannot exist as a folder on the way: the write fails and
            // is swallowed instead of stopping the tool.
            Assert.False(ToolSettings.Save("|", "wall-openings", new SampleSettings()));
        }

        [Fact]
        public void ToolSettings_RefusesANameThatIsNotAPlainFileName()
        {
            Assert.Throws<ArgumentException>(() => ToolSettings.PathOf("C:\\settings", "..\\escape"));
            Assert.Throws<ArgumentException>(() => ToolSettings.PathOf("C:\\settings", "tool.json"));
            Assert.Throws<ArgumentException>(() => ToolSettings.PathOf("  ", "tool"));
        }

        private static string NewDirectory()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "ArKrPlugFR-tests-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);

            return directory;
        }

        private sealed class SampleSettings
        {
            public bool Doors { get; set; } = true;

            public bool Windows { get; set; } = true;
        }
    }
}
