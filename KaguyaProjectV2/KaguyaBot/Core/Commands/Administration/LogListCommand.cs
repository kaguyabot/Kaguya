using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class LogListCommand : KaguyaBase
    {
        [AdminCommand]
        [Command("LogTypes")]
        [Alias("loglist", "logs")]
        [Summary("Displays all possible logtypes as well as what channel they're currently assigned to if enabled.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            string logSettingString = "";

            foreach (PropertyInfo prop in server.GetType().GetProperties()
                                                .Where(x => x.PropertyType == typeof(ulong) && x.Name.Contains("Log")))
            {
                string n = prop.Name.ToLower();
                string[] premLogs = { "warn", "unwarn", "mute", "unmute", "shadowban", "unshadowban" };
                
                if (!server.IsPremium && premLogs.Any(x => n.Contains(x)))
                    continue;

                ulong matchChannel = (ulong) prop.GetValue(server);

                SocketTextChannel channel = Client.GetGuild(Context.Guild.Id).GetTextChannel(matchChannel);
                bool deletedChannel = channel == null && matchChannel != 0;

                // wtf is this shit...
                // todo: Add 'Kaguya Premium Only' tags to warn, unwarn, shadowban, unshadowban, mute, and unmute logs.
                logSettingString +=
                    $"**{(prop.Name == "ModLog" ? "ModLog (Kaguya Premium Only)" : prop.Name.Replace("Log", ""))}** - {(channel == null && !deletedChannel ? "`Not assigned.`" : " ")} " +
                    $"{(deletedChannel ? $"*`Deleted Channel`*" : $"{(channel == null ? null : $"`#{channel.Name}`")}")}\n";
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Log Settings for {Context.Guild.Name}",
                Description = logSettingString,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"To enable a log type, use the {server.CommandPrefix}log command. " +
                           $"To disable, use the {server.CommandPrefix}rlog command."
                }
            };

            await SendEmbedAsync(embed);
        }
    }
}