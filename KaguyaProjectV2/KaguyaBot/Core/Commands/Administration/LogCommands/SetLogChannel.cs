using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SetLogChannel : ModuleBase<ShardedCommandContext>
    {
        [Command("setlogchannel")]
        [Alias("log")]
        [Summary("Enables a list of given logtypes and sends the log messages to a specific channel. All available logtypes may be displayed with the `logtypes` command.")]
        [Remarks("<logtype> <channel>\ndeletedmessages #my-log-channel\nkaguyaserverlog.bans.unbans #my-admin-log-channel\ntwitchnotifications #live-streams")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task SetChannel(string logType, IGuildChannel channel)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully set logtype `{string.Join(", ", logTypes)}` to channel `#{channel.Name}`"
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
