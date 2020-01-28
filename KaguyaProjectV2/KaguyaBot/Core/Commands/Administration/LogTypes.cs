using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class LogTypes : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("LogTypes")]
        [Alias("loglist", "logs")]
        [Summary("Displays all possible logtypes as well as what channel they're currently assigned to if enabled.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var logTypes = LogQuery.AllLogTypes;

            string logSettingString = "";

            foreach (var prop in server.GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(ulong) && !x.Name.Contains("Id")))
            {
                if (!server.IsPremium && prop.Name.ToLower() == "modlog")
                    continue;

                var matchChannel = (ulong)prop.GetValue(server);

                SocketTextChannel? channel = ConfigProperties.Client.GetGuild(Context.Guild.Id).GetTextChannel(matchChannel);
                var deletedChannel = channel == null && matchChannel != 0;

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

            await Context.Channel.SendEmbedAsync(embed);
        }
    }
}
