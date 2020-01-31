using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public class SupporterBadge
    {
        public SupporterBadge(User user)
        {
            Data.User = user;
        }

        public ProfileTemplateBadge Data = new ProfileTemplateBadge
        {
            Emote = ConfigProperties.Client.GetGuild(546880579057221644).GetEmoteAsync(672187970534768641).Result,
            Loc = new ProfileTemplateLoc
            {
                X = 100,
                Y = 100
            }
        };
    }
}
