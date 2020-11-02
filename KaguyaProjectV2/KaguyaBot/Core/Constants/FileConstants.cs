using System.IO;

namespace KaguyaProjectV2.KaguyaBot.Core.Constants
{
    /// <summary>
    /// A class containing cross-platform file pathing utilized by the project.
    /// </summary>
    public static class FileConstants
    {
        /// <summary>
        /// The root directory of this project. The "KaguyaProjectV2" directory.
        /// </summary>
        /// <returns></returns>
        public static string RootDir { get; } = Directory.GetCurrentDirectory();

        public static string ResourcesDir { get; } = Path.Combine(RootDir, "Resources");
        public static string ImagesDir { get; } = Path.Combine(ResourcesDir, "Images");
        public static string LogsDir { get; } = Path.Combine(ResourcesDir, "Logs");
        public static string FontsDir { get; } = Path.Combine(ResourcesDir, "Fonts");
    }
}