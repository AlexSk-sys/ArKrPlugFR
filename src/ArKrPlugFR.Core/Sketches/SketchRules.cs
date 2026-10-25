using System;
using System.Globalization;

namespace ArKrPlugFR.Core.Sketches
{
    /// <summary>
    /// The rules of a sketch sheet: how many bars one run sketches and what its views are
    /// called.
    /// <para>
    /// A run is capped so that a mistake costs a handful of sheets and not a hundred, and the
    /// name of a view is built from the shape and the place of the bar in the run, which keeps
    /// the names unique — the platform refuses two views with one name. The rules are plain
    /// text and numbers and are tested without Revit.
    /// </para>
    /// </summary>
    public static class SketchRules
    {
        /// <summary>How many bars one run sketches at most.</summary>
        public const int MaximumSketches = 25;

        /// <summary>The name used when a bar carries no shape name.</summary>
        public const string FallbackShape = "Bar";

        /// <summary>Works out how many bars this run sketches.</summary>
        /// <param name="requested">How many bars were picked.</param>
        /// <param name="maximum">The limit of one run.</param>
        /// <returns>The number of bars the run will sketch.</returns>
        public static int Plan(int requested, int maximum)
        {
            if (requested <= 0 || maximum <= 0)
            {
                return 0;
            }

            return requested < maximum ? requested : maximum;
        }

        /// <summary>Builds the name of the view of one bar.</summary>
        /// <param name="shapeName">Name of the shape of the bar; may be empty.</param>
        /// <param name="index">Place of the bar in the run, counted from one.</param>
        /// <returns>The name of the view.</returns>
        public static string ViewName(string? shapeName, int index)
        {
            string shape = shapeName?.Trim() ?? string.Empty;

            if (shape.Length == 0)
            {
                shape = FallbackShape;
            }

            return index.ToString(CultureInfo.InvariantCulture) + ". " + shape;
        }

        /// <summary>Formats a length in millimetres the way a note shows it.</summary>
        /// <param name="millimeters">The length.</param>
        /// <returns>The number without trailing zeros.</returns>
        public static string Millimeters(double millimeters)
        {
            return millimeters.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
