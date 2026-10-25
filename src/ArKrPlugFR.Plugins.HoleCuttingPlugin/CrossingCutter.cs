using System;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Openings;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace ArKrPlugFR.Plugins.HoleCuttingPlugin
{
    /// <summary>What the cut did: the message to show and whether an opening was cut.</summary>
    internal sealed class CrossingOutcome
    {
        private CrossingOutcome(bool cut, string message)
        {
            Cut = cut;
            Message = message ?? string.Empty;
        }

        /// <summary>Gets a value indicating whether an opening was cut.</summary>
        internal bool Cut { get; }

        /// <summary>Gets the message to show the user.</summary>
        internal string Message { get; }

        /// <summary>Creates an outcome that reports a cut opening.</summary>
        /// <param name="message">Message to show.</param>
        /// <returns>The outcome.</returns>
        internal static CrossingOutcome Opened(string message) => new CrossingOutcome(true, message);

        /// <summary>Creates an outcome that reports why nothing was cut.</summary>
        /// <param name="message">Message to show.</param>
        /// <returns>The outcome.</returns>
        internal static CrossingOutcome Failed(string message) => new CrossingOutcome(false, message);
    }

    /// <summary>
    /// Works out where a service crosses a wall and cuts the opening.
    /// <para>
    /// The crossing is the intersection of the two solids, so a service meeting the wall
    /// at an angle is covered by the extent it really covers, and the opening is the
    /// crossing grown by the clearance on every side. Only the opening is added to the
    /// project: no family has to be loaded and no temporary element is created.
    /// </para>
    /// </summary>
    internal static class CrossingCutter
    {
        /// <summary>Tells whether an element is a service the tool can cut around.</summary>
        /// <param name="element">Element to read.</param>
        /// <returns><c>true</c> for a duct, a pipe, a cable tray or a conduit.</returns>
        internal static bool IsSupported(Element element)
        {
            return element is Duct || element is Pipe || element is CableTray || element is Conduit;
        }

        /// <summary>Cuts the opening where a service crosses a wall.</summary>
        /// <param name="document">Project to cut in.</param>
        /// <param name="wall">Wall that receives the opening.</param>
        /// <param name="service">Service that crosses it.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the cut did.</returns>
        internal static CrossingOutcome Cut(
            Document document,
            Wall wall,
            Element service,
            HoleCuttingText text,
            Log log)
        {
            if (!WallFrame.TryRead(wall, out Point3 basePoint, out Point3 direction))
            {
                return CrossingOutcome.Failed(text.CurvedWall);
            }

            BoundingBox? crossingBox = FindCrossing(wall, service);

            if (crossingBox == null)
            {
                return CrossingOutcome.Failed(text.NoCrossing);
            }

            OpeningRectangle crossing = OpeningRules.InWallFrame(
                crossingBox.Value,
                basePoint,
                direction);

            OpeningRectangle opening = OpeningRules.WithClearance(crossing, OpeningRules.DefaultClearance);

            if (!OpeningRules.IsUsable(opening, OpeningRules.MinimumEdge))
            {
                return CrossingOutcome.Failed(text.TooSmall);
            }

            try
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    document.Create.NewOpening(
                        wall,
                        WallFrame.PointAt(basePoint, direction, opening.AlongMin, opening.HeightMin),
                        WallFrame.PointAt(basePoint, direction, opening.AlongMax, opening.HeightMax));

                    return true;
                });
            }
            catch (Exception exception)
            {
                log.Warn(
                    "HoleCutting: the opening in wall '" + wall.Name + "' was refused: " + exception.Message,
                    nameof(CrossingCutter),
                    exception);

                return CrossingOutcome.Failed(text.CutFailed(exception.Message));
            }

            log.Info(
                "HoleCutting: cut " + opening.ToString() + " in wall '" + wall.Name +
                "', crossing " + crossing.ToString() + ".",
                nameof(CrossingCutter));

            return CrossingOutcome.Opened(text.Success(opening, crossing, wall.Name));
        }

        /// <summary>
        /// Finds the box where the service and the wall meet: the intersection of their
        /// solids, or of their bounding boxes when a solid cannot be read.
        /// </summary>
        private static BoundingBox? FindCrossing(Wall wall, Element service)
        {
            Solid? wallSolid = Solids.FirstSolid(wall);
            Solid? serviceSolid = Solids.FirstSolid(service);

            if (wallSolid != null && serviceSolid != null)
            {
                Solid? intersection = Solids.Intersection(serviceSolid, wallSolid);

                if (intersection != null)
                {
                    BoundingBox? meeting = Solids.BoxOf(intersection);

                    if (meeting != null)
                    {
                        return meeting;
                    }
                }
            }

            BoundingBox? wallBox = ModelGeometry.GetModelBounds(wall);
            BoundingBox? serviceBox = ModelGeometry.GetModelBounds(service);

            if (wallBox == null || serviceBox == null)
            {
                return null;
            }

            return wallBox.Value.Intersect(serviceBox.Value);
        }

    }
}
