using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class ViewGreeting : KaguyaBase
    {
        [UtilityCommand]
        [Command("ViewGreeting")]
        [Alias("vg", "gv")]
        [Summary("Displays this server's greeting message.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Greeting Message",
                Description = $"Current greeting:\n\n`" +
                              $"{(!string.IsNullOrEmpty(server.CustomGreeting) ? $"`{server.CustomGreeting}`" : "*No greeting set.*")}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"This greeting is currently {(server.CustomGreetingIsEnabled ? "enabled" : "disabled")}. Toggle " +
                           $"this setting with the {server.CommandPrefix}tg command."
                }
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}