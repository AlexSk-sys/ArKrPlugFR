using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Bridges the differences in <c>ElementId</c> between Revit releases.
    /// <para>
    /// The integer accessor and the integer constructor of an element id are
    /// deprecated from Revit 2024 in favour of a long-valued API that does not
    /// exist in Revit 2020 and 2022. The release symbol comes from
    /// <c>Directory.Build.props</c> and is set by the <c>RevitVersion</c>
    /// property, so all conditional compilation stays in this folder.
    /// </para>
    /// </summary>
    public static class ElementIdCompat
    {
        /// <summary>Returns the numeric value of an element id, in every supported release.</summary>
        /// <param name="id">The element id.</param>
        public static long GetValue(ElementId id)
        {
#if REVIT_2024
            return id.Value;
#else
            return id.IntegerValue;
#endif
        }

        /// <summary>Creates an element id from a numeric value, in every supported release.</summary>
        /// <param name="value">The numeric value of the element id.</param>
        public static ElementId Create(long value)
        {
#if REVIT_2024
            return new ElementId(value);
#else
            return new ElementId((int)value);
#endif
        }
    }
}
