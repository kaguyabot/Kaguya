using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KaguyaProjectV2.KaguyaBot.Core.Constants
{
    /// <summary>
    ///     A class containing cross-platform file pathing utilized by the project.
    /// </summary>
    public static class FileConstants
    {
        private static readonly string _workingDirectory = Environment.CurrentDirectory;

        /// <summary>
        ///     The root directory of this project. The "KaguyaProjectV2" directory.
        /// </summary>
        /// <returns></returns>
        private static string RootDir
        {
            get
            {
                var rg = new Regex(@".*KaguyaProjectV2");
                if (!rg.IsMatch(_workingDirectory))
                {
                    throw new Exception("An invalid working directory exists. Does not contain " +
                                        "required: 'KaguyaProjectV2' folder. Directory in question: " +
                                        $"'{_workingDirectory}'");
                }

                return rg.Match(_workingDirectory).Value;
            }
        }

        public static string ResourcesDir { get; } = Path.Combine(RootDir, "Resources");
        public static string ImagesDir { get; } = Path.Combine(ResourcesDir, "Images");
        public static string LogsDir { get; } = Path.Combine(ResourcesDir, "Logs");
        public static string FontsDir { get; } = Path.Combine(ResourcesDir, "Fonts");
    }
}