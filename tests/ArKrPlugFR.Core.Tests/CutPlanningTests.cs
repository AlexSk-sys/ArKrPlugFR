using System;
using ArKrPlugFR.Core.Concrete;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class CutPlanningTests
    {
        [Fact]
        public void Profile_ReadsTheCrossingAcrossTheRun()
        {
            var crossing = new BoundingBox(new Point3(10d, 20d, 30d), new Point3(110d, 220d, 330d));

            AxisRectangle alongX = CutPlanning.Profile(crossing, CutAxis.X);
            Assert.Equal(20d, alongX.MinU);
            Assert.Equal(220d, alongX.MaxU);
            Assert.Equal(30d, alongX.MinV);
            Assert.Equal(330d, alongX.MaxV);

            AxisRectangle alongY = CutPlanning.Profile(crossing, CutAxis.Y);
            Assert.Equal(10d, alongY.MinU);
            Assert.Equal(110d, alongY.MaxU);
            Assert.Equal(30d, alongY.MinV);

            AxisRectangle alongZ = CutPlanning.Profile(crossing, CutAxis.Z);
            Assert.Equal(10d, alongZ.MinU);
            Assert.Equal(20d, alongZ.MinV);
            Assert.Equal(220d, alongZ.MaxV);
        }

        [Fact]
        public void ExtentAlong_MeasuresTheElementAlongTheRun()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(300d, 400d, 500d));

            Assert.Equal(300d, CutPlanning.ExtentAlong(box, CutAxis.X));
            Assert.Equal(400d, CutPlanning.ExtentAlong(box, CutAxis.Y));
            Assert.Equal(500d, CutPlanning.ExtentAlong(box, CutAxis.Z));
        }

        [Fact]
        public void CenterAlong_ReadsTheMiddleOfTheElement()
        {
            var box = new BoundingBox(new Point3(100d, 0d, 0d), new Point3(300d, 400d, 600d));

            Assert.Equal(200d, CutPlanning.CenterAlong(box, CutAxis.X));
            Assert.Equal(300d, CutPlanning.CenterAlong(box, CutAxis.Z));
        }

        [Fact]
        public void WithClearance_GrowsEverySide()
        {
            var crossing = new AxisRectangle(100d, 200d, 300d, 400d);

            AxisRectangle opening = CutPlanning.WithClearance(crossing, Length.FromMillimeters(50d));

            Assert.Equal(50d, opening.MinU);
            Assert.Equal(250d, opening.MaxU);
            Assert.Equal(250d, opening.MinV);
            Assert.Equal(450d, opening.MaxV);
        }

        [Fact]
        public void WithClearance_ReadsANegativeValueAsNone()
        {
            var crossing = new AxisRectangle(100d, 200d, 300d, 400d);

            AxisRectangle opening = CutPlanning.WithClearance(crossing, Length.FromMillimeters(-10d));

            Assert.Equal(crossing.MinU, opening.MinU);
            Assert.Equal(crossing.MaxV, opening.MaxV);
        }

        [Fact]
        public void Fit_ClampsTheOpeningToTheElement()
        {
            var opening = new AxisRectangle(0d, 400d, 200d, 600d);
            var face = new AxisRectangle(100d, 300d, 250d, 550d);

            AxisRectangle fitted = CutPlanning.Fit(opening, face);

            Assert.Equal(100d, fitted.MinU);
            Assert.Equal(300d, fitted.MaxU);
            Assert.Equal(250d, fitted.MinV);
            Assert.Equal(550d, fitted.MaxV);
        }

        [Fact]
        public void Fit_CollapsesAnOpeningThatMissesTheElement()
        {
            var opening = new AxisRectangle(400d, 500d, 0d, 100d);
            var face = new AxisRectangle(0d, 300d, 0d, 300d);

            AxisRectangle fitted = CutPlanning.Fit(opening, face);

            Assert.Equal(0d, fitted.Width.Millimeters);
        }

        [Fact]
        public void Coincides_ComparesEveryEdgeWithinTheTolerance()
        {
            var first = new AxisRectangle(100d, 300d, 400d, 600d);
            var second = new AxisRectangle(105d, 295d, 395d, 605d);

            Assert.True(CutPlanning.Coincides(first, second, Length.FromMillimeters(10d)));
            Assert.False(CutPlanning.Coincides(first, second, Length.FromMillimeters(3d)));
        }

        [Fact]
        public void IsUsable_RequiresBothEdgesToReachTheMinimum()
        {
            var minimum = Length.FromMillimeters(10d);

            Assert.True(CutPlanning.IsUsable(new AxisRectangle(0d, 100d, 0d, 50d), minimum));
            Assert.False(CutPlanning.IsUsable(new AxisRectangle(0d, 5d, 0d, 50d), minimum));
            Assert.False(CutPlanning.IsUsable(new AxisRectangle(0d, 100d, 0d, 5d), minimum));
        }

        [Fact]
        public void ProjectedExtent_MeasuresTheShadowOfTheBox()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(300d, 400d, 500d));

            Assert.Equal(300d, CutPlanning.ProjectedExtent(box, new Point3(1d, 0d, 0d)), 3);
            Assert.Equal(400d, CutPlanning.ProjectedExtent(box, new Point3(0d, 5d, 0d)), 3);
            Assert.Equal(500d, CutPlanning.ProjectedExtent(box, new Point3(0d, 0d, 1d)), 3);
        }

        [Fact]
        public void ProjectedExtent_GrowsWhenTheDirectionIsDiagonal()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));

            // A unit cube projected onto the diagonal of its footprint casts a shadow
            // as wide as its diagonal.
            Assert.Equal(Math.Sqrt(20000d), CutPlanning.ProjectedExtent(box, new Point3(1d, 1d, 0d)), 3);
        }

        [Fact]
        public void ProjectedExtent_RejectsADirectionThatIsASinglePoint()
        {
            var box = new BoundingBox(new Point3(0d, 0d, 0d), new Point3(100d, 100d, 100d));

            Assert.Throws<ArgumentException>(() => CutPlanning.ProjectedExtent(box, Point3.Origin));
        }
    }
}
