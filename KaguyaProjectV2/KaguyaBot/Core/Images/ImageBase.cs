using SixLabors.Fonts;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Images
{
    public abstract class ImageBase
    {
        public static string FontPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Fonts)}/framd.ttf";
#if DEBUG
        public const string PROFILE_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\ProfileSmall.png";
        public const string XP_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\XpLevelUpSmall.png";
#else
        public const string PROFILE_TEMPLATE_PATH = @"Resources\Images\ProfileSmall.png";
        public const string XP_TEMPLATE_PATH = @"Resources\Images\XpLevelUpSmall.png";
#endif

        public static Font Font(float fontSize)
        {
            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(FontPath);

            return new Font(frankGothicFont, fontSize);
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
            FontFamily frankGothicFont = fontCollection.Install(FontPath);

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