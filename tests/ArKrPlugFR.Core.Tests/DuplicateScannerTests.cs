using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArKrPlugFR.Core.Duplication;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class DuplicateScannerTests
    {
        /// <summary>A type with no placed instances.</summary>
        private static TypeRecord Type(long id, string name, params (string Name, string Value)[] values)
        {
            return Type(id, name, 0, values);
        }

        /// <summary>A type with placed instances.</summary>
        private static TypeRecord Type(long id, string name, int instances, params (string Name, string Value)[] values)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach ((string valueName, string value) in values)
            {
                map[valueName] = value;
            }

            return new TypeRecord(id, familyId: 1L, name, instances, map);
        }

        private static FamilyRecord Family(string name, params TypeRecord[] types)
        {
            return new FamilyRecord(name, "Doors", types);
        }

        [Fact]
        public void Scan_ReportsATypeThatRepeatsAnotherTypeOfTheSameFamily()
        {
            FamilyRecord family = Family(
                "Door",
                Type(1L, "1200x1000", 3, ("Width", "1200")),
                Type(2L, "1200x1000_copy", ("Width", "1200")));

            DuplicateScanResult result = DuplicateScanner.Scan(new[] { family });

            DuplicateTypeRow row = Assert.Single(result.TypeDuplicates);
            Assert.Equal("1200x1000_copy", row.Duplicate.TypeName);
            Assert.Equal("1200x1000", row.Kept.TypeName);
            Assert.True(row.IsRemovable);
            Assert.Equal(1, result.RemovableTypeCount);
        }

        [Fact]
        public void Scan_KeepsTheTypeThatComesFirstByName()
        {
            FamilyRecord family = Family(
                "Door",
                Type(1L, "Copy", ("Width", "1200")),
                Type(2L, "Original", 5, ("Width", "1200")));

            DuplicateTypeRow row = Assert.Single(DuplicateScanner.Scan(new[] { family }).TypeDuplicates);

            // The kept type is chosen by name, as the specification asks; usage only
            // decides whether a row may be deleted.
            Assert.Equal("Original", row.Duplicate.TypeName);
            Assert.Equal("Copy", row.Kept.TypeName);
            Assert.False(row.IsRemovable);
        }

        [Fact]
        public void Scan_IgnoresParametersThatOnlyIdentifyOrAnnotateAType()
        {
            FamilyRecord family = Family(
                "Door",
                Type(1L, "A", 1, ("Width", "1200"), ("Comments", "left over"), ("Cost", "100")),
                Type(2L, "B", ("Width", "1200"), ("Comments", "garbage"), ("Cost", "200")));

            Assert.Single(DuplicateScanner.Scan(new[] { family }).TypeDuplicates);
        }

        [Fact]
        public void Scan_KeepsTypesApartWhenARealParameterDiffers()
        {
            FamilyRecord family = Family(
                "Door",
                Type(1L, "A", ("Width", "1200")),
                Type(2L, "B", ("Width", "1500")));

            Assert.Empty(DuplicateScanner.Scan(new[] { family }).TypeDuplicates);
        }

        [Fact]
        public void Scan_TreatsAnEmptyParameterAsAbsent()
        {
            FamilyRecord family = Family(
                "Door",
                Type(1L, "A", ("Width", "1200")),
                Type(2L, "B", ("Width", "1200"), ("FireRating", "  ")));

            Assert.Single(DuplicateScanner.Scan(new[] { family }).TypeDuplicates);
        }

        [Fact]
        public void Scan_DoesNotCompareTypesOfDifferentFamilies()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Door A", Type(1L, "A", ("Width", "1200"))),
                Family("Door B", Type(2L, "B", ("Width", "1200"))),
            });

            Assert.Empty(result.TypeDuplicates);
        }

        [Fact]
        public void Scan_ReportsFamiliesThatOfferTheSameTypes()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Door_1", Type(1L, "1200x1000", ("Width", "1200"))),
                Family("Door_1_copy", Type(2L, "1200x1000", ("Width", "1200"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Door_1_copy", row.Duplicate.Name);
            Assert.Equal("Door_1", row.Kept.Name);
            Assert.Equal(1, row.DuplicateTypeCount);
            Assert.Equal(1, row.KeptTypeCount);
        }

        [Fact]
        public void Scan_KeepsTheFamilyTheProjectUsesMost()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Alpha", Type(1L, "Only", ("Width", "1200"))),
                Family("Beta", Type(2L, "Only", 4, ("Width", "1200"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Alpha", row.Duplicate.Name);
            Assert.Equal("Beta", row.Kept.Name);
            Assert.True(row.IsRemovable);
        }

        [Fact]
        public void Scan_DoesNotDeleteAFamilyThatHasInstances()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Alpha", Type(1L, "Only", 2, ("Width", "1200"))),
                Family("Beta", Type(2L, "Only", 4, ("Width", "1200"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Alpha", row.Duplicate.Name);
            Assert.Equal(2, row.InstanceCount);
            Assert.False(row.IsRemovable);
        }

        [Fact]
        public void Scan_ReportsAFamilyWhoseTypesAreAllContainedInAnother()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Small", Type(1L, "A", ("Width", "1200"))),
                Family("Large", Type(2L, "A", ("Width", "1200")), Type(3L, "B", ("Width", "1500"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Small", row.Duplicate.Name);
            Assert.Equal("Large", row.Kept.Name);
            Assert.Equal(1, row.MatchedTypeCount);
            Assert.Equal(2, row.KeptTypeCount);
        }

        [Fact]
        public void Scan_DoesNotReverseAContainedCopy()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Large", Type(1L, "A", ("Width", "1200")), Type(2L, "B", ("Width", "1500"))),
                Family("Small", Type(3L, "A", ("Width", "1200"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Small", row.Duplicate.Name);
        }

        [Fact]
        public void Scan_UsesAFamilyOnlyOnce()
        {
            // "Middle" and "Small" offer the same single type, so they are a
            // full-copy group and neither takes part in the containment pass.
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Middle", Type(1L, "A", ("Width", "1200"))),
                Family("Large", Type(2L, "A", ("Width", "1200")), Type(3L, "B", ("Width", "1500"))),
                Family("Small", Type(4L, "A", ("Width", "1200"))),
            });

            DuplicateFamilyRow row = Assert.Single(result.FamilyDuplicates);
            Assert.Equal("Middle", row.Kept.Name);
        }

        [Fact]
        public void Scan_IgnoresFamiliesWithoutTypes()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Empty"),
                Family("AlsoEmpty"),
                Family("Real", Type(1L, "A", ("Width", "1200"))),
            });

            Assert.Empty(result.FamilyDuplicates);
        }

        [Fact]
        public void Scan_CountsEveryTypeItCompared()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family("Door", Type(1L, "A", ("Width", "1200")), Type(2L, "B", ("Width", "1500"))),
                Family("Window", Type(3L, "C", ("Width", "600"))),
            });

            Assert.Equal(3, result.CheckedTypeCount);
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Scan_ReportsNearDuplicatesOnlyWhenNothingElseWasFound()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family(
                    "Door",
                    Type(1L, "A", ("Width", "1200"), ("Height", "2000")),
                    Type(2L, "B", ("Width", "1500"), ("Height", "2000"))),
            });

            NearDuplicatePair pair = Assert.Single(result.NearDuplicates);
            Assert.Equal("Door", pair.FamilyName);
            Assert.Equal("A", pair.FirstTypeName);
            Assert.Equal("B", pair.SecondTypeName);
            Assert.Equal(1, pair.DifferenceCount);
        }

        [Fact]
        public void Scan_DoesNotReportNearDuplicatesWhenDuplicatesExist()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(new[]
            {
                Family(
                    "Door",
                    Type(1L, "A", ("Width", "1200")),
                    Type(2L, "B", ("Width", "1200")),
                    Type(3L, "C", ("Width", "1400"))),
            });

            Assert.NotEmpty(result.TypeDuplicates);
            Assert.Empty(result.NearDuplicates);
        }

        [Fact]
        public void Scan_RespectsTheNearDuplicateReportLimit()
        {
            var types = new List<TypeRecord>();

            for (int index = 0; index < 10; index++)
            {
                types.Add(Type(
                    index + 1L,
                    "T" + index.ToString(CultureInfo.InvariantCulture),
                    ("Width", (1200 + (index * 100)).ToString(CultureInfo.InvariantCulture)),
                    ("Height", "2000")));
            }

            DuplicateScanResult result = DuplicateScanner.Scan(
                new[] { Family("Door", types.ToArray()) },
                new DuplicateScanOptions(nearDuplicateReportLimit: 2));

            Assert.Equal(2, result.NearDuplicates.Count);
        }

        [Fact]
        public void Scan_IsDeterministic()
        {
            var families = new[]
            {
                Family("Beta", Type(1L, "Zed", ("Width", "1200"))),
                Family("Alpha", Type(2L, "Zed", ("Width", "1200"))),
            };

            DuplicateScanResult first = DuplicateScanner.Scan(families);
            DuplicateScanResult second = DuplicateScanner.Scan(families);

            Assert.Equal(first.FamilyDuplicates[0].Duplicate.Name, second.FamilyDuplicates[0].Duplicate.Name);
            Assert.Equal("Alpha", first.FamilyDuplicates[0].Kept.Name);
            Assert.Equal("Beta", first.FamilyDuplicates[0].Duplicate.Name);
        }

        [Fact]
        public void Scan_OfNothingFindsNothing()
        {
            DuplicateScanResult result = DuplicateScanner.Scan(Array.Empty<FamilyRecord>());

            Assert.True(result.IsEmpty);
            Assert.Equal(0, result.CheckedTypeCount);
        }

        [Fact]
        public void Scan_RejectsNullFamilies()
        {
            Assert.Throws<ArgumentNullException>(() => DuplicateScanner.Scan(null!));
        }

        [Fact]
        public void Options_RejectInconsistentNearDuplicateRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DuplicateScanOptions(nearDuplicateMinimumDifferences: 3, nearDuplicateMaximumDifferences: 1));
        }

        [Fact]
        public void TypeSignature_IgnoresParameterOrder()
        {
            TypeRecord left = Type(1L, "A", ("Width", "1200"), ("Height", "2000"));
            TypeRecord right = Type(2L, "B", ("Height", "2000"), ("Width", "1200"));

            var options = new DuplicateScanOptions();

            Assert.Equal(TypeSignature.Create(left, options), TypeSignature.Create(right, options));
        }

        [Fact]
        public void TypeSignature_CountsMissingParametersAsDifferences()
        {
            var options = new DuplicateScanOptions();

            TypeSignature left = TypeSignature.Create(Type(1L, "A", ("Width", "1200")), options);
            TypeSignature right = TypeSignature.Create(Type(2L, "B", ("Width", "1200"), ("Height", "2000")), options);

            Assert.Equal(1, left.DifferenceCount(right));
            Assert.Equal(1, right.DifferenceCount(left));
        }
    }
}
