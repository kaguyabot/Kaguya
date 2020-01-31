using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateBadge
    {
        public User User { get; set; }

        public Emote Emote { get; set; }
        public ProfileTemplateLoc Loc { get; set; }
    }
}
