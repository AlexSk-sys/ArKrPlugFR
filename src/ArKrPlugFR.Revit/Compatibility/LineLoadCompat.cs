using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Revit.Compatibility
{
    /// <summary>
    /// Creates a line load that is not attached to an analytical element, on every supported
    /// release.
    /// <para>
    /// Revit 2020 and 2022 create a non-hosted load directly, from the two ends of the line
    /// and a work plane. Revit 2024 removed that overload: there a load is always created on
    /// an analytical host, and the documented way to a free load is to release it from the
    /// host afterwards. The difference lives here rather than in the tool, which asks for a
    /// free load and gets one — or an exception that says why not.
    /// </para>
    /// </summary>
    public static class LineLoadCompat
    {
        /// <summary>
        /// Gets a value indicating whether the release creates line loads only on an
        /// analytical host, so the caller has to offer one.
        /// </summary>
        public static bool RequiresAnalyticalHost
        {
            get
            {
#if REVIT_2024
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Returns the analytical element associated with a physical element, or <c>null</c>
        /// when the release does not need one or the element has none.
        /// </summary>
        /// <param name="document">The document to read.</param>
        /// <param name="physicalId">Id of the physical element, for example a wall.</param>
        public static ElementId? FindAnalyticalHost(Document document, ElementId physicalId)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (physicalId == null)
            {
                throw new ArgumentNullException(nameof(physicalId));
            }

#if REVIT_2024
            AnalyticalToPhysicalAssociationManager manager =
                AnalyticalToPhysicalAssociationManager.GetAnalyticalToPhysicalAssociationManager(document);

            if (manager == null)
            {
                return null;
            }

            ElementId associated = manager.GetAssociatedElementId(physicalId);

            return associated == null || associated == ElementId.InvalidElementId ? null : associated;
#else
            return null;
#endif
        }

        /// <summary>Creates a line load that is not attached to an analytical element.</summary>
        /// <param name="document">The document to edit.</param>
        /// <param name="analyticalHostId">The analytical element to create the load on; only 2024 needs it.</param>
        /// <param name="start">Start of the load line.</param>
        /// <param name="end">End of the load line.</param>
        /// <param name="force">Force vector of the start of the load.</param>
        /// <param name="moment">Moment vector of the start of the load.</param>
        /// <param name="plane">Work plane of the load; only 2020 and 2022 use it.</param>
        /// <returns>The new load.</returns>
        public static LineLoad CreateFree(
            Document document,
            ElementId analyticalHostId,
            XYZ start,
            XYZ end,
            XYZ force,
            XYZ moment,
            SketchPlane plane)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            if (force == null)
            {
                throw new ArgumentNullException(nameof(force));
            }

            if (moment == null)
            {
                throw new ArgumentNullException(nameof(moment));
            }

#if REVIT_2024
            if (analyticalHostId == null || analyticalHostId == ElementId.InvalidElementId)
            {
                throw new InvalidOperationException(
                    "This release creates a line load only on an analytical host, and none was given.");
            }

            LineLoad hosted = LineLoad.Create(
                document,
                analyticalHostId,
                Line.CreateBound(start, end),
                force,
                moment,
                null);

            if (hosted == null)
            {
                throw new InvalidOperationException("The platform did not create the line load.");
            }

            hosted.RemoveHostConstraint();

            return hosted;
#else
            LineLoad load = LineLoad.Create(document, start, end, force, moment, null, plane);

            if (load == null)
            {
                throw new InvalidOperationException("The platform did not create the line load.");
            }

            return load;
#endif
        }
    }
}
