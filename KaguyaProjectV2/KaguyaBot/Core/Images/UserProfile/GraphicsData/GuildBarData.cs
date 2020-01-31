using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public static class GuildBarData
    {
        public static ProfileTemplateXpBar Bar = new ProfileTemplateXpBar
        {
            Color = Rgba32.BlueViolet,
            Length = 446,
            LocA = new ProfileTemplateLoc
            {
                X = 38.5f,
                Y = 213f
            },
            LocB = new ProfileTemplateLoc
            {
                X = 37.5f,
                Y = 253.5f
            }
        };

        //public static PointF[] GuildXpBarCoordinates(User user, Server server)
        //{

        //}
    }
}
