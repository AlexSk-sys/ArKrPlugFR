using System;
using ArKrPlugFR.Core.Units;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Reads the facts of a bar type on every supported release.
    /// <para>
    /// The diameter of a bar type changed its name with Revit 2022: the old property is
    /// deprecated there and gone from 2024, and the nominal diameter is the value a drawing
    /// states. The difference lives here so no tool has to know which release it is built
    /// against.
    /// </para>
    /// </summary>
    public static class RebarCompat
    {
        /// <summary>Returns the nominal diameter of a bar type, in millimetres.</summary>
        /// <param name="type">The bar type to read.</param>
        public static double NominalDiameterMillimeters(RebarBarType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

#if REVIT_2020
            return Length.FromFeet(type.BarDiameter).Millimeters;
#else
            return Length.FromFeet(type.BarNominalDiameter).Millimeters;
#endif
        }
    }
}
