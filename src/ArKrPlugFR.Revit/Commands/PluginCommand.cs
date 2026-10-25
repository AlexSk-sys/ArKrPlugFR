using System;
using System.Globalization;
using ArKrPlugFR.Core.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Revit.Commands
{
    /// <summary>
    /// Base class for every tool command.
    /// <para>
    /// It owns the parts that are identical in all tools and easy to get wrong:
    /// the active-document guard, the culture used for captions, logging before
    /// and after the run, and turning an unexpected exception into a message
    /// instead of a Revit crash.
    /// </para>
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public abstract class PluginCommand : IExternalCommand
    {
        /// <summary>Gets the logger shared by the add-in.</summary>
        protected static Log Log { get; } = CreateLog();

        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                throw new ArgumentNullException(nameof(commandData));
            }

            string source = GetType().Name;
            string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            try
            {
                if (commandData.Application.ActiveUIDocument == null)
                {
                    message = "Open a project before running this tool.";
                    Log.Warn("Command cancelled: no active document.", source);
                    return Result.Cancelled;
                }

                var context = new PluginContext(commandData, Log, culture);

                Log.Info("Command started.", source);

                Result result = Run(context);

                Log.Info("Command finished with " + result + ".", source);

                return result;
            }
            catch (OperationCanceledException)
            {
                Log.Info("Command cancelled by the user.", source);
                return Result.Cancelled;
            }
            catch (Exception exception)
            {
                Log.Error("Command failed: " + exception.Message, source, exception);
                message = exception.Message;

                TaskDialog.Show(
                    "ArKrPlugFR",
                    exception.Message + Environment.NewLine + Environment.NewLine +
                    "Details were written to:" + Environment.NewLine + AppPaths.LogDirectory);

                return Result.Failed;
            }
        }

        /// <summary>Runs the tool.</summary>
        /// <param name="context">Active document, logger and culture.</param>
        protected abstract Result Run(PluginContext context);

        private static Log CreateLog()
        {
            var log = new Log();

            try
            {
                log.Add(new FileLogSink(AppPaths.LogDirectory));
            }
            catch (Exception)
            {
                // An unwritable log directory must not stop the add-in from loading.
            }

            return log;
        }
    }
}
