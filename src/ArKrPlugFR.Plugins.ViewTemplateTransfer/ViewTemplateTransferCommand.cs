using System.Collections.Generic;
using ArKrPlugFR.Revit.Commands;
using ArKrPlugFR.Ui.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// Copies views and view templates from the active project into another project
    /// that is open in the same session.
    /// <para>
    /// The active project is the source and is never written to. The window lists its
    /// views and the projects that can take them, and the copy runs as one transaction
    /// inside the chosen project. The tool opens no file of its own, so it has nothing
    /// to close when it ends.
    /// </para>
    /// </summary>
    public sealed class ViewTemplateTransferCommand : PluginCommand
    {
        /// <inheritdoc />
        protected override Result Run(PluginContext context)
        {
            Document source = context.Document;

            var text = new ViewTemplateTransferText(context.Culture);

            var openDocuments = new List<Document>();

            foreach (Document document in context.UiApplication.Application.Documents)
            {
                openDocuments.Add(document);
            }

            var viewModel = new TransferViewModel(source, openDocuments, text, context.Log);
            var window = new ViewTemplateTransferWindow(viewModel);

            WindowOwner.AttachToHost(window);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
