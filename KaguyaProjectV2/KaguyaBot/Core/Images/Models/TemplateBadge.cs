using Discord;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.Models
{
    public class TemplateBadge
    {
        public User User { get; set; }
        public Emote Emote { get; set; }
        public TemplateLoc Loc { get; set; }
    }
}
