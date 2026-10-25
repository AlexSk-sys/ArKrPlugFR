using System;
using ArKrPlugFR.Ui.Interop;

namespace ArKrPlugFR.Ui.Views
{
    /// <summary>
    /// Shows the shared tool window with a result.
    /// <para>
    /// Tools do not build windows of their own: they run, then hand the outcome to
    /// this one place, which keeps every tool's result looking the same and keeps
    /// window plumbing out of the plugins.
    /// </para>
    /// </summary>
    public static class ToolWindowPresenter
    {
        /// <summary>Shows a result window and waits until the user closes it.</summary>
        /// <param name="title">Window title, for example the tool name.</param>
        /// <param name="subtitle">Secondary line, for example the document name and a summary.</param>
        /// <param name="reportCaption">Caption above the report text.</param>
        /// <param name="report">Report text; an empty value hides the report area.</param>
        public static void Show(string title, string subtitle, string reportCaption, string report)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            var window = new ToolWindow
            {
                Title = title,
                Subtitle = subtitle ?? string.Empty,
                ReportCaption = reportCaption ?? string.Empty,
                Report = report ?? string.Empty,
            };

            WindowOwner.AttachToHost(window);
            window.ShowDialog();
        }
    }
}
