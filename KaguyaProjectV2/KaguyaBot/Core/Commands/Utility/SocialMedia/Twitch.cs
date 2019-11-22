using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility.SocialMedia
{
    public class Twitch : ModuleBase<ShardedCommandContext>
    {
        [UtilityCommand]
        [Command("twitch")]
        public async Task SetTwitch(string twitchChannel, IGuildChannel notifChannel, bool mentionEveryone)
        {

        }
    }
}
