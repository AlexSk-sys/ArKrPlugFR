using System;
using ArKrPlugFR.Core.Schedules;
using Xunit;

namespace ArKrPlugFR.Core.Tests
{
    public sealed class HeaderCellTests
    {
        [Fact]
        public void TryResolve_CountsFromOneInsideTheHeaderArea()
        {
            // A header that runs from row 0 to row 5 and from column 0 to column 2.
            bool resolved = HeaderCell.TryResolve(0, 5, 0, 2, 4, 1, out int row, out int column, out HeaderCellProblem problem);

            Assert.True(resolved);
            Assert.Equal(3, row);
            Assert.Equal(0, column);
            Assert.Equal(HeaderCellProblem.None, problem);
        }

        [Fact]
        public void TryResolve_WorksWithAHeaderThatDoesNotStartAtZero()
        {
            bool resolved = HeaderCell.TryResolve(2, 6, 1, 3, 1, 2, out int row, out int column, out _);

            Assert.True(resolved);
            Assert.Equal(2, row);
            Assert.Equal(2, column);
        }

        [Fact]
        public void TryResolve_ReportsARowOutsideTheHeaderArea()
        {
            bool resolved = HeaderCell.TryResolve(0, 3, 0, 1, 5, 1, out int row, out int column, out HeaderCellProblem problem);

            Assert.False(resolved);
            Assert.Equal(0, row);
            Assert.Equal(0, column);
            Assert.Equal(HeaderCellProblem.RowOutOfRange, problem);
        }

        [Fact]
        public void TryResolve_ReportsAColumnOutsideTheHeaderArea()
        {
            bool resolved = HeaderCell.TryResolve(0, 3, 0, 1, 1, 3, out _, out _, out HeaderCellProblem problem);

            Assert.False(resolved);
            Assert.Equal(HeaderCellProblem.ColumnOutOfRange, problem);
        }

        [Fact]
        public void TryResolve_ReportsACountBelowOne()
        {
            Assert.False(HeaderCell.TryResolve(0, 3, 0, 1, 0, 1, out _, out _, out HeaderCellProblem rowProblem));
            Assert.Equal(HeaderCellProblem.RowOutOfRange, rowProblem);

            Assert.False(HeaderCell.TryResolve(0, 3, 0, 1, 1, 0, out _, out _, out HeaderCellProblem columnProblem));
            Assert.Equal(HeaderCellProblem.ColumnOutOfRange, columnProblem);
        }
    }

    public sealed class HeaderValueTests
    {
        [Fact]
        public void FirstNonEmpty_SkipsMissingAndBlankValues()
        {
            Assert.Equal("КЖ-1", HeaderValue.FirstNonEmpty(new string?[] { null, "", "   ", " КЖ-1 ", "КЖ-2" }));
        }

        [Fact]
        public void FirstNonEmpty_ReturnsNullWhenNothingIsUsable()
        {
            Assert.Null(HeaderValue.FirstNonEmpty(new string?[] { null, "", "  " }));
            Assert.Null(HeaderValue.FirstNonEmpty(Array.Empty<string>()));
        }

        [Fact]
        public void FirstNonEmpty_RejectsANullSequence()
        {
            Assert.Throws<ArgumentNullException>(() => HeaderValue.FirstNonEmpty(null!));
        }
    }
}
