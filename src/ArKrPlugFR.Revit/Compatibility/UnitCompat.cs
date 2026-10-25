using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Converts the load units a user types into the internal units of the platform.
    /// <para>
    /// The unit identifiers changed with Revit 2022: up to 2021 a unit is a value of
    /// <c>DisplayUnitType</c>, from 2022 on it is a <c>ForgeTypeId</c>. The difference
    /// lives here so no tool has to know which release it is built against.
    /// </para>
    /// </summary>
    public static class UnitCompat
    {
        /// <summary>Converts a force per length from kilonewtons per metre to internal units.</summary>
        /// <param name="kilonewtonsPerMetre">The value the user typed.</param>
        /// <returns>The value in the internal units of the platform.</returns>
        public static double ForcePerLengthToInternal(double kilonewtonsPerMetre)
        {
#if REVIT_2020
            return UnitUtils.ConvertToInternalUnits(
                kilonewtonsPerMetre,
                DisplayUnitType.DUT_KILONEWTONS_PER_METER);
#else
            return UnitUtils.ConvertToInternalUnits(kilonewtonsPerMetre, UnitTypeId.KilonewtonsPerMeter);
#endif
        }

        /// <summary>Converts a moment per length from kilonewton metres per metre to internal units.</summary>
        /// <param name="kilonewtonMetresPerMetre">The value the user typed.</param>
        /// <returns>The value in the internal units of the platform.</returns>
        public static double MomentPerLengthToInternal(double kilonewtonMetresPerMetre)
        {
#if REVIT_2020
            return UnitUtils.ConvertToInternalUnits(
                kilonewtonMetresPerMetre,
                DisplayUnitType.DUT_KILONEWTON_METERS_PER_METER);
#else
            return UnitUtils.ConvertToInternalUnits(
                kilonewtonMetresPerMetre,
                UnitTypeId.KilonewtonMetersPerMeter);
#endif
        }
    }
}
