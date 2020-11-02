using SixLabors.Fonts;
using System;
using System.IO;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;

namespace KaguyaProjectV2.KaguyaBot.Core.Images
{
    public abstract class ImageBase
    {
        private static readonly string _linuxFontPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}.fonts/framd.ttf";
        private static readonly string _fontPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Fonts)}/framd.ttf";
#if DEBUG
        public const string PROFILE_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\ProfileSmall.png";
        public const string XP_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\XpLevelUpSmall.png";
#else
        public const string PROFILE_TEMPLATE_PATH = @"Resources\Images\ProfileSmall.png";
        public const string XP_TEMPLATE_PATH = @"Resources\Images\XpLevelUpSmall.png";
#endif

        public static Font Font(float fontSize)
        {
            string fontPath = GetFontPath();

            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(fontPath);

            return new Font(frankGothicFont, fontSize);
        }

        public static string GetFontPath()
        {
            string fontPath;
            if (File.Exists(_fontPath))
            {
                fontPath = _fontPath;
            }
            else if (File.Exists(_linuxFontPath))
            {
                fontPath = _linuxFontPath;
            }
            else
            {
                throw new KaguyaSupportException("The font needed for profile " +
                                                 "image generation could not be found.");
            }

            return fontPath;
        }

        /// <summary>
        /// Returns the default font but with a size that reduces based on how long the user's username is.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="baseFontSize"></param>
        /// <param name="charThreshold"></param>
        /// <returns></returns>
        public static Font Font(string username, float baseFontSize, int charThreshold)
        {
            float fontSize = username.Length < charThreshold
                ? baseFontSize
                : baseFontSize - (username.Length + charThreshold);

            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(_fontPath);

            return new Font(frankGothicFont, fontSize);
        }

        public static int XpNameXCoordinate(string name)
        {
            int limit = 142; // Left-most x-coordinate, anything less than this cuts into the template. We don't want that.
            int coord = name.Length > 5 ? 240 - (18 * (name.Length - 5)) : 240;

            if (coord < limit)
                coord = limit;

            return coord;
        }
    }
}