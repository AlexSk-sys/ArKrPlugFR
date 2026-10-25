using System.Collections.Generic;
using ArKrPlugFR.Core.Types;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class TypeCatalogTests
    {
        [Fact]
        public void Build_GroupsTypesUnderFamiliesUnderCategories()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(
                new[]
                {
                    new TypeFact(1, "Structural Framing", "Concrete beam", "200x400"),
                    new TypeFact(2, "Structural Framing", "Concrete beam", "300x600"),
                    new TypeFact(3, "Columns", "Concrete column", "400x400"),
                },
                null,
                "No category");

            Assert.Equal(2, tree.Count);
            Assert.Equal("Columns", tree[0].Name);
            Assert.Single(tree[0].Families);
            Assert.Equal("400x400", tree[0].Families[0].Types[0].Type);

            Assert.Equal("Structural Framing", tree[1].Name);
            Assert.Single(tree[1].Families);
            Assert.Equal(2, tree[1].Families[0].Types.Count);
        }

        [Fact]
        public void Build_CountsTheTypesOfEveryLevel()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(
                new[]
                {
                    new TypeFact(1, "Structural Framing", "Concrete beam", "200x400"),
                    new TypeFact(2, "Structural Framing", "Concrete beam", "300x600"),
                    new TypeFact(3, "Structural Framing", "Steel beam", "IPE 300"),
                },
                null,
                "No category");

            Assert.Equal(3, tree[0].TypeCount);
            Assert.Equal(2, tree[0].Families[0].TypeCount);
            Assert.Equal(1, tree[0].Families[1].TypeCount);
        }

        [Fact]
        public void Build_SortsEveryLevelByName()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(
                new[]
                {
                    new TypeFact(1, "Walls", "Zeta wall", "b"),
                    new TypeFact(2, "Walls", "Alpha wall", "z"),
                    new TypeFact(3, "Walls", "Alpha wall", "a"),
                    new TypeFact(4, "Floors", "Slab", "200"),
                },
                null,
                "No category");

            Assert.Equal("Floors", tree[0].Name);
            Assert.Equal("Walls", tree[1].Name);
            Assert.Equal("Alpha wall", tree[1].Families[0].Name);
            Assert.Equal("Zeta wall", tree[1].Families[1].Name);
            Assert.Equal("a", tree[1].Families[0].Types[0].Type);
            Assert.Equal("z", tree[1].Families[0].Types[1].Type);
        }

        [Fact]
        public void Build_PutsTypesWithoutACategoryUnderTheLabel()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(
                new[] { new TypeFact(1, null, "Some family", "Type 1") },
                null,
                "No category");

            Assert.Single(tree);
            Assert.Equal("No category", tree[0].Name);
        }

        [Fact]
        public void Build_WithAnEmptyQuery_KeepsEveryType()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), string.Empty, "No category");

            Assert.Equal(3, CountTypes(tree));
        }

        [Fact]
        public void Build_FiltersByTheFamilyName()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), "beam", "No category");

            Assert.Equal(2, CountTypes(tree));
            Assert.Single(tree);
        }

        [Fact]
        public void Build_FiltersByTheTypeName()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), "IPE", "No category");

            Assert.Equal(1, CountTypes(tree));
            Assert.Equal("Steel beam", tree[0].Families[0].Name);
        }

        [Fact]
        public void Build_IgnoresCaseAndSurroundingSpacesInTheQuery()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), "  BEAM  ", "No category");

            Assert.Equal(2, CountTypes(tree));
        }

        [Fact]
        public void Build_DropsCategoriesAndFamiliesWithoutMatches()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), "column", "No category");

            Assert.Single(tree);
            Assert.Equal("Columns", tree[0].Name);
            Assert.Single(tree[0].Families);
        }

        [Fact]
        public void Build_KeepsTheSameTypeNameInTwoFamiliesApart()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(
                new[]
                {
                    new TypeFact(1, "Walls", "Wall A", "Type 1"),
                    new TypeFact(2, "Walls", "Wall B", "Type 1"),
                },
                null,
                "No category");

            Assert.Equal(2, tree[0].Families.Count);
        }

        [Fact]
        public void Build_WithNoFacts_ReturnsAnEmptyTree()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(new List<TypeFact>(), null, "No category");

            Assert.Empty(tree);
        }

        [Fact]
        public void Build_WithANullQuery_KeepsEveryType()
        {
            IReadOnlyList<CategoryGroup> tree = TypeCatalog.Build(Facts(), null, "No category");

            Assert.Equal(3, CountTypes(tree));
        }

        private static IReadOnlyList<TypeFact> Facts()
        {
            return new[]
            {
                new TypeFact(1, "Structural Framing", "Concrete beam", "200x400"),
                new TypeFact(2, "Structural Framing", "Steel beam", "IPE 300"),
                new TypeFact(3, "Columns", "Concrete column", "400x400"),
            };
        }

        private static int CountTypes(IReadOnlyList<CategoryGroup> tree)
        {
            int total = 0;

            foreach (CategoryGroup category in tree)
            {
                foreach (FamilyGroup family in category.Families)
                {
                    total += family.Types.Count;
                }
            }

            return total;
        }
    }
}
