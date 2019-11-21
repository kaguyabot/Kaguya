using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands
{
    public class SetLogChannel : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("setlogchannel")]
        [Alias("log")]
        [Summary("Enables a list of given logtypes and sends the log messages to a specific channel. All available logtypes may be displayed with the `logtypes` command.")]
        [Remarks("<logtype> <channel>\ndeletedmessages #my-log-channel\nkaguyaserverlog.bans.unbans #my-admin-log-channel\ntwitchnotifications #live-streams")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task SetChannel(string logType, IGuildChannel channel)
        {
            KaguyaEmbedBuilder embed;
            List<string> logTypes = await LogQuery.LogSwitcher(logType, true, channel.GuildId, channel);

            if(logTypes.Count == 0)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Please specify a valid log type."
                };
                embed.SetColor(EmbedColor.RED);
                goto Reply;
            }

            if (logTypes.Any(x => x.Equals("all", System.StringComparison.OrdinalIgnoreCase)))
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully enabled all log types."
                };
            }
            else
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully enabled logtype `{string.Join(", ", logTypes).ToUpper()}`."
                };
            }

            Reply:
            await ReplyAsync(embed: embed.Build());
        }

        [Command("resetlogchannel")]
        [Alias("rlog")]
        [Summary("Disables a list of given logtypes. All available logtypes may be displayed with the `logtypes` command.")]
        [Remarks("<logtype>\ndeletedmessages\nwarns.unwarns.bans.unbans\ntwitchnotifications")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetLogChannel(string logType)
        { 
            KaguyaEmbedBuilder embed;
            List<string> logTypes = await LogQuery.LogSwitcher(logType, false, Context.Guild.Id);

            if (logTypes.Count == 0)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Please specify a valid log type."
                };
                embed.SetColor(EmbedColor.RED);
                goto Reply;
            }

            if (logTypes.Any(x => x.Equals("all", System.StringComparison.OrdinalIgnoreCase)))
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully disabled all log types."
                };
            }
            else
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully disabled logtype `{string.Join(", ", logTypes).ToUpper()}`."
                };
            }

            Reply:
            await ReplyAsync(embed: embed.Build());
        }
    }
}
