using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Parameters;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// Spreads the value of every picked room onto the structures around it: the boundary
    /// walls, the floor under the room, the ceiling above it and, when asked for, the room
    /// itself.
    /// <para>
    /// A room without a value is counted and skipped, because the tool transfers values
    /// and never clears parameters. Every receiver is written the instance parameter
    /// first and the type parameter second; one that has neither is reported.
    /// </para>
    /// </summary>
    internal static class RoomSpreadEngine
    {
        /// <summary>Runs the spread.</summary>
        /// <param name="document">Project to change.</param>
        /// <param name="rooms">Picked rooms.</param>
        /// <param name="sourceName">Name of the parameter the value is read from.</param>
        /// <param name="targetName">Name of the parameter the value is written to.</param>
        /// <param name="options">Which receivers take the value.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static TransferOutcome Run(
            Document document,
            IReadOnlyList<Room> rooms,
            string sourceName,
            string targetName,
            RoomSpreadOptions options,
            TransferText text,
            Log log)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (rooms == null)
            {
                throw new ArgumentNullException(nameof(rooms));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var outcome = new TransferOutcome { Considered = rooms.Count };

            if (!ParameterNameRules.IsUsable(sourceName))
            {
                outcome.Stopped = text.SourceRequired;

                return outcome;
            }

            if (!ParameterNameRules.IsUsable(targetName))
            {
                outcome.Stopped = text.TargetRequired;

                return outcome;
            }

            if (!options.AnySelected)
            {
                outcome.Stopped = text.NothingTicked;

                return outcome;
            }

            DocumentEdit.Run(document, text.RoomTransactionName, () =>
            {
                bool changed = false;

                foreach (Room room in rooms)
                {
                    string label = ElementDescription.Describe(room);

                    try
                    {
                        if (room.Location == null)
                        {
                            outcome.Skipped++;
                            TransferNotes.Add(outcome.Notes, text.UnplacedRoom(label));

                            continue;
                        }

                        string? value = TransferValue.Normalize(ParameterSupport.Read(room, sourceName));

                        if (value == null)
                        {
                            outcome.Empty++;

                            continue;
                        }

                        RoomStructures structures = RoomSupport.Read(document, room);
                        var targets = new List<Element>();
                        var seen = new HashSet<long>();

                        if (options.Walls)
                        {
                            foreach (Element wall in structures.Walls)
                            {
                                Add(targets, seen, wall);
                            }
                        }

                        if (options.Floors)
                        {
                            Add(targets, seen, structures.Floor);
                        }

                        if (options.Ceilings)
                        {
                            Add(targets, seen, structures.Ceiling);
                        }

                        if (options.Rooms)
                        {
                            Add(targets, seen, room);
                        }

                        if ((options.Floors || options.Ceilings) && structures.Floor == null && structures.Ceiling == null)
                        {
                            TransferNotes.Add(outcome.Notes, text.NoFloorOrCeiling(label));
                        }

                        foreach (Element target in targets)
                        {
                            string targetLabel = ElementDescription.Describe(target);

                            try
                            {
                                if (ParameterSupport.TryWrite(target, targetName, value))
                                {
                                    outcome.Written++;
                                    changed = true;
                                }
                                else
                                {
                                    outcome.WithoutParameter++;
                                    TransferNotes.Add(outcome.Notes, text.NoTargetParameter(targetLabel));
                                }
                            }
                            catch (Exception exception)
                            {
                                outcome.Failed++;
                                TransferNotes.Add(outcome.Failures, text.WriteFailed(targetLabel, exception.Message));
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        outcome.Failed++;
                        TransferNotes.Add(outcome.Failures, text.WriteFailed(label, exception.Message));
                    }
                }

                return changed;
            });

            log.Info(
                "TransferParameters/rooms: read " + outcome.Considered + ", written " + outcome.Written +
                ", empty " + outcome.Empty + ", skipped " + outcome.Skipped +
                ", without the parameter " + outcome.WithoutParameter + ", refused " + outcome.Failed + ".",
                nameof(RoomSpreadEngine));

            return outcome;
        }

        private static void Add(List<Element> targets, HashSet<long> seen, Element? candidate)
        {
            if (candidate != null && seen.Add(ElementIdCompat.GetValue(candidate.Id)))
            {
                targets.Add(candidate);
            }
        }
    }
}
