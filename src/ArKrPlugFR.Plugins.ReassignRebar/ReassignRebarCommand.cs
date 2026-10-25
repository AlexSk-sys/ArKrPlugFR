using System;
using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ArKrPlugFR.Plugins.ReassignRebar
{
    /// <summary>
    /// Attaches the picked rebar to the structure it actually belongs to.
    /// <para>
    /// The tool asks for the rebar, then for the column, wall, beam or floor to attach
    /// it to, and does the rest in one undoable step. Real bars are re-hosted, bars from
    /// IFC get the host's mark, nothing else changes.
    /// </para>
    /// </summary>
    public sealed class ReassignRebarCommand : PluginCommand
    {
        /// <summary>Name of the settings file of this tool.</summary>
        private const string SettingsName = "reassign-rebar";

        /// <summary>How many refusal lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new ReassignText(context.Culture);
            UIDocument uiDocument = context.UiDocument;

            var picked = new List<Element>();

            try
            {
                foreach (Reference reference in uiDocument.Selection.PickObjects(
                    ObjectType.Element,
                    new BarSelectionFilter(),
                    text.PickBars))
                {
                    if (context.Document.GetElement(reference) is Element element)
                    {
                        picked.Add(element);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ToolWindowPresenter.Show(text.WindowTitle, text.Subtitle, text.ReportCaption, text.Cancelled);

                return Result.Cancelled;
            }

            if (picked.Count == 0)
            {
                ToolWindowPresenter.Show(text.WindowTitle, text.Subtitle, text.ReportCaption, text.NothingPicked);

                return Result.Cancelled;
            }

            Element host;

            try
            {
                Reference reference = uiDocument.Selection.PickObject(
                    ObjectType.Element,
                    new HostSelectionFilter(),
                    text.PickHost);

                host = context.Document.GetElement(reference);
            }
            catch (OperationCanceledException)
            {
                ToolWindowPresenter.Show(text.WindowTitle, text.Subtitle, text.ReportCaption, text.Cancelled);

                return Result.Cancelled;
            }

            ReassignSettings settings = ToolSettings.Load(
                AppPaths.SettingsDirectory,
                SettingsName,
                new ReassignSettings());

            ReassignOutcome outcome = ReassignEngine.Run(
                context.Document, picked, host, settings, text, context.Log);

            ToolWindowPresenter.Show(
                text.WindowTitle,
                text.Subtitle,
                text.ReportCaption,
                BuildReport(outcome, text));

            return Result.Succeeded;
        }

        private static string BuildReport(ReassignOutcome outcome, ReassignText text)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }
            else
            {
                builder.AppendLine(text.Result(
                    outcome.Reassigned,
                    outcome.IfcUpdated,
                    outcome.Skipped,
                    outcome.Refused));
            }

            int shown = 0;

            foreach (string failure in outcome.Failures)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(failure);
                shown++;
            }

            if (outcome.Failures.Count > shown)
            {
                builder.AppendLine(text.MoreLine(outcome.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>Lets the user pick rebar of any of its three shapes.</summary>
        private sealed class BarSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return ReassignEngine.Classify(elem) != Core.HostChange.RebarKind.Other;
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        /// <summary>Lets the user pick the structure the rebar is attached to.</summary>
        private sealed class HostSelectionFilter : ISelectionFilter
        {
            /// <inheritdoc />
            public bool AllowElement(Element elem)
            {
                return elem is Wall
                    || elem is Floor
                    || (elem is FamilyInstance
                        && (ElementScope.HasCategory(elem, BuiltInCategory.OST_StructuralColumns)
                            || ElementScope.HasCategory(elem, BuiltInCategory.OST_Columns)
                            || ElementScope.HasCategory(elem, BuiltInCategory.OST_StructuralFraming)));
            }

            /// <inheritdoc />
            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
