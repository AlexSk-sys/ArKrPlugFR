using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace ArKrPlugFR.Ui.Interop
{
    /// <summary>
    /// Bridges WPF windows and Revit's main window.
    /// <para>
    /// A plugin window that is not owned by Revit can end up behind the Revit
    /// window or outside the screen the user is working on; both are avoidable by
    /// setting the owner explicitly.
    /// </para>
    /// </summary>
    public static class WindowOwner
    {
        /// <summary>Returns the handle of the host window, or <see cref="IntPtr.Zero"/> when it is not available.</summary>
        public static IntPtr GetHostWindowHandle()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.MainWindowHandle;
                }
            }
            catch (InvalidOperationException)
            {
                return IntPtr.Zero;
            }
            catch (NotSupportedException)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sets the owner of a window to the host window so it stays above Revit,
        /// and centres it when no owner could be set.
        /// </summary>
        public static void AttachToHost(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            IntPtr handle = GetHostWindowHandle();

            if (handle != IntPtr.Zero)
            {
                new WindowInteropHelper(window).Owner = handle;
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
