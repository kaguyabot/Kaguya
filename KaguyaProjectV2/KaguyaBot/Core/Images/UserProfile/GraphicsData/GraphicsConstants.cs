using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.Fonts;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public static class GraphicsConstants
    {
        // Todo: Un-hardcode file paths.
        public const string FONT_PATH = @"C:\Users\stage\Documents\GitHub\Kaguya\KaguyaProjectV2\Resources\Fonts\frankMedium.ttf";
        public const string TEMPLATE_PATH = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\ProfileSmall.png";

        public static Font Font(float fontSize)
        {
            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(FONT_PATH);
            return new Font(frankGothicFont, fontSize);
        }
    }
}
