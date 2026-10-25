using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Loads;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Compatibility;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>What one run did.</summary>
    internal sealed class LoadOutcome
    {
        internal LoadOutcome(
            string? stopped,
            int walls,
            int created,
            int skipped,
            int refused,
            bool caseCreated,
            int hostedStayed,
            IReadOnlyList<string> reasons)
        {
            Stopped = stopped;
            Walls = walls;
            Created = created;
            Skipped = skipped;
            Refused = refused;
            CaseCreated = caseCreated;
            HostedStayed = hostedStayed;
            Reasons = reasons ?? Array.Empty<string>();
        }

        internal string? Stopped { get; }

        internal int Walls { get; }

        internal int Created { get; }

        internal int Skipped { get; }

        internal int Refused { get; }

        internal bool CaseCreated { get; }

        internal int HostedStayed { get; }

        internal IReadOnlyList<string> Reasons { get; }
    }

    /// <summary>
    /// Creates the line loads of the chosen walls at the bottom of every wall.
    /// <para>
    /// The load line is the wall's own location line, which lies at the base of the wall.
    /// The force and the moment are converted from the units a user types into the internal
    /// units of the platform; the vectors follow <see cref="LineLoadRules"/>, which is
    /// tested without Revit. The whole run is one undoable step, and every wall that is
    /// skipped or refused is reported with its id.
    /// </para>
    /// </summary>
    internal static class WallLineLoadsEngine
    {
        /// <summary>Runs the load creation for the given walls.</summary>
        /// <param name="document">Project to edit.</param>
        /// <param name="walls">The walls to load, ordered by id.</param>
        /// <param name="settings">The load values and the load case.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        /// <returns>What the run did.</returns>
        internal static LoadOutcome Run(
            Document document,
            IReadOnlyList<Wall> walls,
            LoadSettings settings,
            LoadText text,
            Log log)
        {
            if (walls.Count == 0)
            {
                return new LoadOutcome(text.NoWalls, 0, 0, 0, 0, false, 0, Array.Empty<string>());
            }

            if (!LineLoadRules.IsUsable(settings.Force, settings.Moment))
            {
                return new LoadOutcome(text.NoForce, walls.Count, 0, 0, 0, false, 0, Array.Empty<string>());
            }

            var reasons = new List<string>();
            var plan = new List<(Wall Wall, Line Line)>();

            foreach (Wall wall in walls)
            {
                if (wall.Location is LocationCurve location && location.Curve is Line line)
                {
                    plan.Add((wall, line));
                }
                else
                {
                    reasons.Add(text.NotStraight(ElementDescription.Describe(wall)));
                }
            }

            if (plan.Count == 0)
            {
                return new LoadOutcome(text.NoStraightWalls, walls.Count, 0, reasons.Count, 0, false, 0, reasons);
            }

            double force = UnitCompat.ForcePerLengthToInternal(settings.Force);
            double moment = UnitCompat.MomentPerLengthToInternal(settings.Moment);

            int created = 0;
            int skipped = reasons.Count;
            int refused = 0;
            int hostedStayed = 0;
            bool caseCreated = false;

            try
            {
                DocumentEdit.Run(document, text.TransactionName, () =>
                {
                    LoadCase loadCase = FindOrCreateCase(document, settings.CaseName, text, log, out caseCreated);

                    var planes = new Dictionary<double, SketchPlane>();

                    foreach ((Wall wall, Line line) in plan)
                    {
                        try
                        {
                            ElementId? host = LineLoadCompat.FindAnalyticalHost(document, wall.Id);

                            if (LineLoadCompat.RequiresAnalyticalHost && host == null)
                            {
                                skipped++;
                                reasons.Add(text.NoAnalyticalHost(ElementDescription.Describe(wall)));

                                continue;
                            }

                            XYZ start = line.GetEndPoint(0);
                            XYZ end = line.GetEndPoint(1);
                            SketchPlane plane = PlaneAt(document, planes, start.Z);

                            LineLoad load = LineLoadCompat.CreateFree(
                                document,
                                host ?? ElementId.InvalidElementId,
                                start,
                                end,
                                ToXyz(LineLoadRules.Force(force)),
                                ToXyz(LineLoadRules.Moment(moment)),
                                plane);

                            load.LoadCaseId = loadCase.Id;

                            SetVectors(load, force, moment, settings.Uniform);
                            SetComment(load, settings.Comment);

                            if (load.IsHosted)
                            {
                                hostedStayed++;
                            }

                            created++;

                            log.Info(
                                "WallLineLoads: created a line load on " + ElementDescription.Describe(wall) + ".",
                                nameof(WallLineLoadsEngine));
                        }
                        catch (Exception exception)
                        {
                            refused++;
                            reasons.Add(text.Refused(ElementDescription.Describe(wall), exception.Message));

                            log.Warn(
                                "WallLineLoads: " + ElementDescription.Describe(wall) +
                                " was refused: " + exception.Message,
                                nameof(WallLineLoadsEngine),
                                exception);
                        }
                    }

                    return created > 0;
                });
            }
            catch (Exception exception)
            {
                log.Error(
                    "WallLineLoads: the run failed: " + exception.Message,
                    nameof(WallLineLoadsEngine),
                    exception);

                return new LoadOutcome(exception.Message, walls.Count, created, skipped, refused, caseCreated, hostedStayed, reasons);
            }

            log.Info(
                "WallLineLoads: " + created.ToString(CultureInfo.InvariantCulture) + " load(s) over " +
                walls.Count.ToString(CultureInfo.InvariantCulture) + " wall(s), " +
                skipped.ToString(CultureInfo.InvariantCulture) + " skipped, " +
                refused.ToString(CultureInfo.InvariantCulture) + " refused.",
                nameof(WallLineLoadsEngine));

            return new LoadOutcome(null, walls.Count, created, skipped, refused, caseCreated, hostedStayed, reasons);
        }

        private static LoadCase FindOrCreateCase(
            Document document,
            string name,
            LoadText text,
            Log log,
            out bool created)
        {
            created = false;

            foreach (LoadCase existing in new FilteredElementCollector(document)
                .OfClass(typeof(LoadCase))
                .Cast<LoadCase>())
            {
                if (string.Equals(existing.Name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return existing;
                }
            }

            LoadNature? nature = new FilteredElementCollector(document)
                .OfClass(typeof(LoadNature))
                .Cast<LoadNature>()
                .FirstOrDefault();

            if (nature == null)
            {
                nature = LoadNature.Create(document, name);
            }

            LoadCase loadCase = LoadCase.Create(document, name, nature.Id, LoadCaseCategory.Dead);

            created = true;

            log.Info(
                "WallLineLoads: created the load case «" + name + "».",
                nameof(WallLineLoadsEngine));

            return loadCase;
        }

        private static SketchPlane PlaneAt(Document document, Dictionary<double, SketchPlane> planes, double z)
        {
            double key = Math.Round(z, 6);

            if (planes.TryGetValue(key, out SketchPlane? plane))
            {
                return plane;
            }

            Plane geometry = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, key));
            SketchPlane created = SketchPlane.Create(document, geometry);

            planes[key] = created;

            return created;
        }

        private static void SetVectors(LineLoad load, double force, double moment, bool uniform)
        {
            load.ForceVector1 = ToXyz(LineLoadRules.Force(force));
            load.MomentVector1 = ToXyz(LineLoadRules.Moment(moment));
            load.ForceVector2 = ToXyz(LineLoadRules.EndForce(force, uniform));
            load.MomentVector2 = ToXyz(LineLoadRules.EndMoment(moment, uniform));
        }

        private static void SetComment(LineLoad load, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return;
            }

            Parameter? parameter = load.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
            {
                parameter.Set(comment);
            }
        }

        private static XYZ ToXyz(LoadVector vector)
        {
            return new XYZ(vector.X, vector.Y, vector.Z);
        }
    }
}
