using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Geometry;
using ArKrPlugFR.Core.Joins;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.UnjoinElements
{
    /// <summary>
    /// Removes the cuts that columns or beams make in the selected walls.
    /// <para>
    /// The search is deliberately split in two: the cheap part asks Revit which
    /// elements are joined to each selected wall, and the rule that decides whether
    /// a join counts lives in the domain layer, where it is tested without Revit.
    /// </para>
    /// </summary>
    internal sealed class UnjoinEngine
    {
        private const BuiltInCategory WallCategory = BuiltInCategory.OST_Walls;

        private readonly PluginContext _context;
        private readonly UnjoinSearchOptions _options;

        internal UnjoinEngine(PluginContext context, UnjoinSearchOptions options)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>Runs the tool.</summary>
        /// <param name="neighbourCategories">Categories whose cuts are to be removed.</param>
        /// <param name="transactionName">Name shown in the undo list.</param>
        internal UnjoinOutcome Run(
            IReadOnlyCollection<BuiltInCategory> neighbourCategories,
            string transactionName)
        {
            if (neighbourCategories == null)
            {
                throw new ArgumentNullException(nameof(neighbourCategories));
            }

            Document document = _context.Document;
            ICollection<ElementId> selection = _context.UiDocument.Selection.GetElementIds();

            if (selection.Count == 0)
            {
                return UnjoinOutcome.NoSelection();
            }

            IReadOnlyList<Element> walls = ElementScope.WithCategory(
                ElementScope.Resolve(document, selection),
                WallCategory);

            if (walls.Count == 0)
            {
                return UnjoinOutcome.NoWalls();
            }

            var wallBoxes = new Dictionary<long, BoundingBox>();
            var wallElements = new Dictionary<long, Element>();

            foreach (Element wall in walls)
            {
                BoundingBox? box = ModelGeometry.GetModelBounds(wall);

                if (box == null)
                {
                    continue;
                }

                long id = ElementIdCompat.GetValue(wall.Id);
                wallBoxes[id] = box.Value;
                wallElements[id] = wall;
            }

            if (wallBoxes.Count == 0)
            {
                return UnjoinOutcome.NoWalls();
            }

            IReadOnlyDictionary<long, BoundingBox> neighbourBoxes = CollectNeighbourBoxes(document, neighbourCategories);
            IReadOnlyList<UnjoinCandidate> candidates = CollectCandidates(
                document,
                wallElements,
                neighbourBoxes);

            IReadOnlyList<JoinPair> pairs = UnjoinCandidates.Select(wallBoxes, candidates, _options.SearchTolerance);

            if (pairs.Count == 0)
            {
                _context.Log.Info(
                    "No cut found in " + wallBoxes.Count + " wall(s).",
                    nameof(UnjoinEngine));

                return UnjoinOutcome.NothingToDo(wallBoxes.Count);
            }

            var failed = new List<JoinPair>();
            int removedCount = 0;

            DocumentEdit.Run(document, transactionName, () =>
            {
                foreach (JoinPair pair in pairs)
                {
                    Element? wall = document.GetElement(ElementIdCompat.Create(pair.First));
                    Element? neighbour = document.GetElement(ElementIdCompat.Create(pair.Second));

                    if (wall == null || neighbour == null)
                    {
                        failed.Add(pair);
                        continue;
                    }

                    try
                    {
                        if (!JoinGeometryUtils.AreElementsJoined(document, wall, neighbour))
                        {
                            // The specification calls this "there was no cut": skip it,
                            // it is not a failure.
                            continue;
                        }

                        JoinGeometryUtils.UnjoinGeometry(document, wall, neighbour);

                        if (JoinGeometryUtils.AreElementsJoined(document, wall, neighbour))
                        {
                            failed.Add(pair);
                        }
                        else
                        {
                            removedCount++;
                        }
                    }
                    catch (Exception exception)
                    {
                        _context.Log.Warn(
                            "Could not unjoin elements " + pair + ": " + exception.Message,
                            nameof(UnjoinEngine));

                        failed.Add(pair);
                    }
                }

                return removedCount > 0;
            });

            _context.Log.Info(
                "Offered " + pairs.Count + " cut(s), removed " + removedCount + ", could not change " + failed.Count + ".",
                nameof(UnjoinEngine));

            return UnjoinOutcome.Completed(wallBoxes.Count, pairs, removedCount, failed);
        }

        private static IReadOnlyDictionary<long, BoundingBox> CollectNeighbourBoxes(
            Document document,
            IReadOnlyCollection<BuiltInCategory> neighbourCategories)
        {
            var boxes = new Dictionary<long, BoundingBox>();

            foreach (Element element in DocumentInfo.GetElements(document, neighbourCategories))
            {
                BoundingBox? box = ModelGeometry.GetModelBounds(element);

                if (box != null)
                {
                    boxes[ElementIdCompat.GetValue(element.Id)] = box.Value;
                }
            }

            return boxes;
        }

        private IReadOnlyList<UnjoinCandidate> CollectCandidates(
            Document document,
            IReadOnlyDictionary<long, Element> walls,
            IReadOnlyDictionary<long, BoundingBox> neighbourBoxes)
        {
            var candidates = new List<UnjoinCandidate>();

            foreach (KeyValuePair<long, Element> wall in walls)
            {
                ICollection<ElementId> joined;

                try
                {
                    joined = JoinGeometryUtils.GetJoinedElements(document, wall.Value);
                }
                catch (Exception exception)
                {
                    _context.Log.Warn(
                        "Could not read the joins of element #" + wall.Key + ": " + exception.Message,
                        nameof(UnjoinEngine));

                    continue;
                }

                foreach (ElementId joinedId in joined)
                {
                    long otherId = ElementIdCompat.GetValue(joinedId);

                    if (otherId == wall.Key || !neighbourBoxes.TryGetValue(otherId, out BoundingBox box))
                    {
                        continue;
                    }

                    candidates.Add(new UnjoinCandidate(wall.Key, otherId, box, isJoined: true));
                }
            }

            return candidates;
        }
    }
}
