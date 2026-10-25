using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Creates a floor from a profile on every supported release.
    /// <para>
    /// Revit 2020 creates floors through the creation document. From Revit 2022 on the
    /// static <c>Floor.Create</c> is the supported call; the older one is obsolete there
    /// and gone from 2024. The difference lives here rather than in the tool.
    /// </para>
    /// </summary>
    public static class FloorCompat
    {
        /// <summary>Creates a floor on the level and with the type of a source floor.</summary>
        /// <param name="document">The document to edit.</param>
        /// <param name="profile">Profile of the floor: the first loop is the outer one, the rest are openings.</param>
        /// <param name="source">Floor whose type and level are carried over.</param>
        /// <returns>The new floor.</returns>
        public static Floor CreateLike(Document document, IReadOnlyList<CurveLoop> profile, Floor source)
        {
#if REVIT_2020
            var curves = new CurveArray();

            foreach (CurveLoop loop in profile)
            {
                foreach (Curve curve in loop)
                {
                    curves.Append(curve);
                }
            }

            var level = (Level)document.GetElement(source.LevelId);

            return document.Create.NewFloor(curves, source.FloorType, level, false);
#else
            return Floor.Create(document, new List<CurveLoop>(profile), source.FloorType.Id, source.LevelId);
#endif
        }
    }
}
