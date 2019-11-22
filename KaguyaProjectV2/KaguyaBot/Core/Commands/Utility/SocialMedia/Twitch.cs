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
        [Command("addtwitch")]
        [Alias("twitch", "at", "addtwitchchannel")]
        [Summary("Takes in a Twitch channel name, Discord text channel, and a true/false statement to enable " +
                 "logging livestream notifications for a Twitch channel. The true/false at the end of the command " +
                 "determines whether the live-stream notification should mention everyone when posting the announcement.")]
        [Remarks("<TwitchChannel> <Discord chat channel> <true/false>\nTheKaguyaBot #live-streams true\nTheKaguyaBot #live-streams false")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task AddTwitchChannel()
        {

        }
    }
}
