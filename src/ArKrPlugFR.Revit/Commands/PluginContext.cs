using System;
using ArKrPlugFR.Core.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Revit.Commands
{
    /// <summary>
    /// Everything a tool needs to run: the Revit document, the user interface
    /// document, a logger and the active culture.
    /// <para>
    /// Commands receive this instead of pulling globals, so a tool can be
    /// exercised against a fake context without a running Revit.
    /// </para>
    /// </summary>
    public sealed class PluginContext
    {
        /// <summary>Creates a context.</summary>
        /// <param name="commandData">Data supplied by Revit for the running command.</param>
        /// <param name="log">Logger the tool writes to.</param>
        /// <param name="culture">Two-letter culture code used to pick UI strings.</param>
        public PluginContext(ExternalCommandData commandData, Log log, string culture)
        {
            CommandData = commandData ?? throw new ArgumentNullException(nameof(commandData));
            Log = log ?? throw new ArgumentNullException(nameof(log));
            Culture = culture ?? "en";
        }

        /// <summary>Gets the data supplied by Revit for the running command.</summary>
        public ExternalCommandData CommandData { get; }

        /// <summary>Gets the Revit application.</summary>
        public UIApplication UiApplication => CommandData.Application;

        /// <summary>Gets the active document.</summary>
        public Document Document => CommandData.Application.ActiveUIDocument.Document;

        /// <summary>Gets the document the user is working in.</summary>
        public UIDocument UiDocument => CommandData.Application.ActiveUIDocument;

        /// <summary>Gets the logger of the add-in.</summary>
        public Log Log { get; }

        /// <summary>Gets the two-letter culture code for user interface strings.</summary>
        public string Culture { get; }
    }
}
