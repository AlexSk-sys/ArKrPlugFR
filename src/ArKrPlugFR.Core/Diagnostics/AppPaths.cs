using System;
using System.IO;

namespace ArKrPlugFR.Core.Diagnostics
{
    /// <summary>
    /// Per-user locations the add-in owns. Everything lives under the local
    /// application data folder so the add-in never writes inside the Revit
    /// installation directory, which is read-only for a normal user.
    /// </summary>
    public static class AppPaths
    {
        /// <summary>Name of the folder created per user.</summary>
        public const string ProductFolderName = "ArKrPlugFR";

        /// <summary>Directory that holds log files.</summary>
        public static string LogDirectory => Path.Combine(RootDirectory, "logs");

        /// <summary>Directory that holds tool settings.</summary>
        public static string SettingsDirectory => Path.Combine(RootDirectory, "settings");

        /// <summary>Root directory of the per-user data of the add-in.</summary>
        public static string RootDirectory => Path.Combine(BaseDirectory, ProductFolderName);

        private static string BaseDirectory
        {
            get
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (string.IsNullOrEmpty(folder))
                {
                    folder = Path.GetTempPath();
                }

                return folder;
            }
        }
    }
}
