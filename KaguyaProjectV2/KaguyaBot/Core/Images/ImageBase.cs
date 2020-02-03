using Discord.WebSocket;
using SixLabors.Fonts;

namespace KaguyaProjectV2.KaguyaBot.Core.Images
{
    public abstract class ImageBase
    {
        public const string FONT_PATH = @"C:\Users\stage\Documents\GitHub\Kaguya\KaguyaProjectV2\Resources\Fonts\frankMedium.ttf";
        public const string PROFILE_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\ProfileSmall.png";
        public const string XP_TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\XpLevelUpSmall.png";

        public static Font Font(float fontSize)
        {
            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(FONT_PATH);
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
            var fontSize = username.Length < charThreshold
                ? baseFontSize : baseFontSize - (username.Length + charThreshold);
            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(FONT_PATH);
            return new Font(frankGothicFont, fontSize);
        }

        public static int XpNameXCoordinate(string name)
        {
            var limit = 142; // Left-most x-coordinate, anything less than this cuts into the template. We don't want that.
            var coord = name.Length > 5 ? 240 - (18 * (name.Length - 5)) : 240;

            if (coord < limit)
                coord = limit;

            return coord;
        }
    }
}
