using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class PremiumBadge
    {
        public PremiumBadge(User user)
        {
            Data.User = user;
        }

        public readonly TemplateBadge Data = new TemplateBadge
        {
            Emote = ConfigProperties.Client.GetGuild(546880579057221644)?.GetEmoteAsync(672187970534768641)?.Result,
            Loc = new TemplateLoc
            {
                X = 100,
                Y = 100
            }
        };
    }
}
