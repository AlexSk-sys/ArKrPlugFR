using System.Collections.Generic;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// Cuts openings in the finish walls that line the selected walls, opposite the doors
    /// and windows standing in them.
    /// <para>
    /// The user selects the bearing walls, chooses whether doors, windows or both are
    /// processed, and the tool does the rest in one undoable step. The switches are kept
    /// between sessions.
    /// </para>
    /// </summary>
    public sealed class WallOpeningsCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            var text = new WallOpeningsText(context.Culture);

            var walls = new List<Wall>();

            ICollection<ElementId>? selected = context.UiApplication.ActiveUIDocument?.Selection.GetElementIds();

            if (selected != null)
            {
                foreach (ElementId id in selected)
                {
                    if (context.Document.GetElement(id) is Wall wall)
                    {
                        walls.Add(wall);
                    }
                }
            }

            if (walls.Count == 0)
            {
                Log.Info("WallOpenings: no wall in the selection.", nameof(WallOpeningsCommand));

                ToolWindowPresenter.Show(
                    text.WindowTitle,
                    text.Subtitle,
                    text.ReportCaption,
                    text.NothingSelected);

                return Result.Cancelled;
            }

            var viewModel = new WallOpeningsViewModel(context.Document, walls, text, context.Log);
            var window = new WallOpeningsWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
