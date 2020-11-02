using System;
using System.IO;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Constants
{
    /// <summary>
    /// A class containing cross-platform file pathing utilized by the project.
    /// </summary>
    public static class FileConstants
    {
        private static string _workingDirectory = Environment.CurrentDirectory;
        /// <summary>
        /// The root directory of this project. The "KaguyaProjectV2" directory.
        /// </summary>
        /// <returns></returns>
        private static string RootDir
        {
            get
            {
                var fSb = new StringBuilder();
                DirectoryInfo parent = Directory.GetParent(_workingDirectory);
                DirectoryInfo[] subDirs = parent.GetDirectories();
                
                // We start at the bottom, iterating through to the top.
                for (int dIdx = subDirs.Length - 1; dIdx >= 0; dIdx--)
                {
                    string dir = subDirs[dIdx].FullName;

                    if (dir.EndsWith("KaguyaProjectV2"))
                        return dir;
                }

                return fSb.ToString();
            }
        }

        public static string ResourcesDir { get; } = Path.Combine(RootDir, "Resources");
        public static string ImagesDir { get; } = Path.Combine(ResourcesDir, "Images");
        public static string LogsDir { get; } = Path.Combine(ResourcesDir, "Logs");
        public static string FontsDir { get; } = Path.Combine(ResourcesDir, "Fonts");
    }
}