using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.Alignment;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Units;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>What one alignment did.</summary>
    internal sealed class AlignmentOutcome
    {
        internal AlignmentOutcome(
            string? stopped,
            int considered,
            int moved,
            int inPlace,
            int skipped,
            int refused,
            int dragged,
            IReadOnlyList<string> notes)
        {
            Stopped = stopped;
            Considered = considered;
            Moved = moved;
            InPlace = inPlace;
            Skipped = skipped;
            Refused = refused;
            Dragged = dragged;
            Notes = notes ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal int Considered { get; }

        internal int Moved { get; }

        internal int InPlace { get; }

        internal int Skipped { get; }

        internal int Refused { get; }

        internal int Dragged { get; }

        internal IReadOnlyList<string> Notes { get; }
    }

    /// <summary>
    /// Brings analytical elements in line with a target element.
    /// <para>
    /// The member alignment lifts every element by the vertical offset
    /// <see cref="AlignmentRules"/> works out; the node alignment moves every node to the
    /// nearest point of the target line and drags the ends of the analytical members that
    /// sat on the node, so a member never loses its node. Both actions are one undoable step
    /// and touch analytical elements only.
    /// </para>
    /// </summary>
    internal static class AlignmentEngine
    {
        /// <summary>How far a value may lie from its target and still count as in place, in millimetres.</summary>
        private const double ToleranceMillimeters = 0.5;

        /// <summary>Aligns the picked elements to the height of the target.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="target">The target element.</param>
        /// <param name="elements">The elements to move.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static AlignmentOutcome AlignMembers(
            Document document,
            Element target,
            IReadOnlyList<Element> elements,
            AlignmentText text,
            Log log)
        {
            if (!TryReadSegment(target, out Segment3 targetLine))
            {
                return new AlignmentOutcome(text.NoTargetLine, 0, 0, 0, 0, 0, 0, Array.Empty<string>());
            }

            var notes = new List<string>();
            var moves = new List<(Element Element, double OffsetMillimeters)>();
            int inPlace = 0;
            int skipped = 0;

            foreach (Element element in elements)
            {
                if (!TryReadSegment(element, out Segment3 current))
                {
                    skipped++;
                    notes.Add(text.SkippedNoLine(ElementDescription.Describe(element)));

                    continue;
                }

                double offset = AlignmentRules.VerticalOffset(targetLine, current);

                if (Math.Abs(offset) < ToleranceMillimeters)
                {
                    inPlace++;

                    continue;
                }

                moves.Add((element, offset));
            }

            int moved = 0;
            int refused = 0;

            if (moves.Count > 0)
            {
                DocumentEdit.Run(document, text.MembersTransaction, () =>
                {
                    foreach ((Element element, double offset) in moves)
                    {
                        try
                        {
                            XYZ shift = new XYZ(0, 0, Length.FromMillimeters(offset).Feet);

                            ElementTransformUtils.MoveElement(document, element.Id, shift);
                            moved++;

                            log.Info(
                                "AnalyticalAlignment: moved " + ElementDescription.Describe(element) + ".",
                                nameof(AlignmentEngine));
                        }
                        catch (Exception exception)
                        {
                            refused++;
                            notes.Add(text.Refused(ElementDescription.Describe(element), exception.Message));

                            log.Warn(
                                "AnalyticalAlignment: " + ElementDescription.Describe(element) +
                                " was refused: " + exception.Message,
                                nameof(AlignmentEngine),
                                exception);
                        }
                    }

                    return moved > 0;
                });
            }

            return new AlignmentOutcome(
                null,
                elements.Count,
                moved,
                inPlace,
                skipped,
                refused,
                0,
                notes);
        }

        /// <summary>Aligns the picked nodes onto the target line, dragging the member ends with them.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="target">The target element.</param>
        /// <param name="nodes">The nodes to move.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static AlignmentOutcome AlignNodes(
            Document document,
            Element target,
            IReadOnlyList<Element> nodes,
            AlignmentText text,
            Log log)
        {
            if (!TryReadSegment(target, out Segment3 targetLine))
            {
                return new AlignmentOutcome(text.NoTargetLine, 0, 0, 0, 0, 0, 0, Array.Empty<string>());
            }

            var notes = new List<string>();
            var moves = new List<(Element Node, XYZ From, XYZ To)>();
            int inPlace = 0;
            int skipped = 0;

            foreach (Element element in nodes)
            {
                if (!(element is ReferencePoint point) || point.Position == null)
                {
                    skipped++;
                    notes.Add(text.SkippedNotANode(ElementDescription.Describe(element)));

                    continue;
                }

                Point3 position = ToMillimeters(point.Position);
                Point3 projected = AlignmentRules.ProjectOntoLine(position, targetLine);

                if (position.DistanceTo(projected) < ToleranceMillimeters)
                {
                    inPlace++;

                    continue;
                }

                moves.Add((element, point.Position, ToFeet(projected)));
            }

            int moved = 0;
            int refused = 0;
            int dragged = 0;

            if (moves.Count > 0)
            {
                DocumentEdit.Run(document, text.NodesTransaction, () =>
                {
                    List<(ElementId Id, Segment3 Segment)> members = AnalyticalSegments(document);

                    foreach ((Element node, XYZ from, XYZ to) in moves)
                    {
                        try
                        {
                            if (!TryMoveNode(node, from, to))
                            {
                                throw new InvalidOperationException("The platform did not move the node.");
                            }

                            moved++;
                            dragged += DragMembers(document, members, from, to, notes, text, log);
                        }
                        catch (Exception exception)
                        {
                            refused++;
                            notes.Add(text.Refused(ElementDescription.Describe(node), exception.Message));

                            log.Warn(
                                "AnalyticalAlignment: " + ElementDescription.Describe(node) +
                                " was refused: " + exception.Message,
                                nameof(AlignmentEngine),
                                exception);
                        }
                    }

                    return moved > 0;
                });
            }

            return new AlignmentOutcome(
                null,
                nodes.Count,
                moved,
                inPlace,
                skipped,
                refused,
                dragged,
                notes);
        }

        private static bool TryMoveNode(Element node, XYZ from, XYZ to)
        {
            if (node is ReferencePoint point)
            {
                try
                {
                    point.Position = to;

                    return true;
                }
                catch (Exception)
                {
                    // A point that is driven by its host refuses a direct position; the
                    // element itself is moved instead.
                }
            }

            ElementTransformUtils.MoveElement(node.Document, node.Id, to - from);

            return true;
        }

        private static int DragMembers(
            Document document,
            List<(ElementId Id, Segment3 Segment)> members,
            XYZ from,
            XYZ to,
            List<string> notes,
            AlignmentText text,
            Log log)
        {
            Point3 oldPoint = ToMillimeters(from);
            int dragged = 0;

            foreach ((ElementId id, Segment3 segment) in members)
            {
                if (!AlignmentRules.HasEndpointAt(segment, oldPoint, ToleranceMillimeters * 2))
                {
                    continue;
                }

                Element? member = document.GetElement(id);

                if (member == null || !(member.Location is LocationCurve location) || location.Curve == null)
                {
                    continue;
                }

                try
                {
                    XYZ start = location.Curve.GetEndPoint(0);
                    XYZ end = location.Curve.GetEndPoint(1);
                    XYZ newStart = start.DistanceTo(from) <= Length.FromMillimeters(ToleranceMillimeters * 2).Feet ? to : start;
                    XYZ newEnd = end.DistanceTo(from) <= Length.FromMillimeters(ToleranceMillimeters * 2).Feet ? to : end;

                    if (newStart.DistanceTo(newEnd) < Length.FromMillimeters(ToleranceMillimeters).Feet)
                    {
                        continue;
                    }

                    location.Curve = Line.CreateBound(newStart, newEnd);
                    dragged++;
                }
                catch (Exception exception)
                {
                    notes.Add(text.Refused(ElementDescription.Describe(member), exception.Message));

                    log.Warn(
                        "AnalyticalAlignment: dragging " + ElementDescription.Describe(member) +
                        " failed: " + exception.Message,
                        nameof(AlignmentEngine),
                        exception);
                }
            }

            return dragged;
        }

        private static List<(ElementId Id, Segment3 Segment)> AnalyticalSegments(Document document)
        {
            var segments = new List<(ElementId, Segment3)>();

            foreach (ElementId id in AnalyticalCompat.CollectAnalyticalIds(document))
            {
                Element? element = document.GetElement(id);

                if (element != null && TryReadSegment(element, out Segment3 segment))
                {
                    segments.Add((id, segment));
                }
            }

            return segments;
        }

        private static bool TryReadSegment(Element element, out Segment3 segment)
        {
            segment = default;

            if (!(element.Location is LocationCurve location) || location.Curve == null)
            {
                return false;
            }

            try
            {
                XYZ start = location.Curve.GetEndPoint(0);
                XYZ end = location.Curve.GetEndPoint(1);

                segment = new Segment3(ToMillimeters(start), ToMillimeters(end));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Point3 ToMillimeters(XYZ point)
        {
            return new Point3(
                Length.FromFeet(point.X).Millimeters,
                Length.FromFeet(point.Y).Millimeters,
                Length.FromFeet(point.Z).Millimeters);
        }

        private static XYZ ToFeet(Point3 point)
        {
            return new XYZ(
                Length.FromMillimeters(point.X).Feet,
                Length.FromMillimeters(point.Y).Feet,
                Length.FromMillimeters(point.Z).Feet);
        }
    }
}
