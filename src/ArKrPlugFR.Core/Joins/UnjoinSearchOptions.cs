using System;
using ArKrPlugFR.Core.Units;

namespace ArKrPlugFR.Core.Joins
{
    /// <summary>
    /// The settings that decide which joins a search considers.
    /// <para>
    /// The tolerance exists because a column and the wall it cuts rarely share an
    /// exact face: Revit reports the two boxes a little apart, and a zero margin
    /// would miss the very join the user is looking for.
    /// </para>
    /// </summary>
    public sealed class UnjoinSearchOptions
    {
        /// <summary>The default search margin, half a foot, in millimetres.</summary>
        public const double DefaultToleranceMillimeters = Length.MillimetersPerFoot / 2d;

        /// <summary>Creates the options with the default search margin.</summary>
        public UnjoinSearchOptions()
            : this(Length.FromMillimeters(DefaultToleranceMillimeters))
        {
        }

        /// <summary>Creates the options with a specific search margin.</summary>
        /// <param name="searchTolerance">Margin added around an element box; must not be negative.</param>
        public UnjoinSearchOptions(Length searchTolerance)
        {
            if (searchTolerance.Millimeters < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(searchTolerance),
                    searchTolerance,
                    "The search margin cannot be negative.");
            }

            SearchTolerance = searchTolerance;
        }

        /// <summary>Gets the margin added around an element box before testing for overlap.</summary>
        public Length SearchTolerance { get; }
    }
}
