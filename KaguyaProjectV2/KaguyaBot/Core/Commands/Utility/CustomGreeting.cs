using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class CustomGreeting : KaguyaBase
    {
        [UtilityCommand]
        [Command("SetCustomGreeting")]
        [Alias("greeting", "welcome", "scg")]
        [Summary("Allows a server administrator to set a custom greeting for a server. Custom " +
                 "greetings are limited to `1,750` characters. The channel this command is used in " +
                 "will determine where the greeting messages are sent, but this may be later " +
                 "overridden via the `log` command, as greetings are also technically a logtype. Special " +
                 "parameters may also be used in your greeting:\n\n" +
                 "`{USERNAME}` - The name of the user who just joined the server.\n" +
                 "`{USERMENTION}` - Mentions the user who just joined.\n" +
                 "`{SERVER}` - The name of this server.\n" +
                 "`{MEMBERCOUNT}` - The member count of this server, including the user who just joined. " +
                 "Formatted as such: `321st`, `4th`, etc.\n")]
        [Remarks("<msg>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command([Remainder] string message)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            if (message.Length > 1750)
            {
                await SendBasicErrorEmbedAsync($"Sorry, your message is too long. The maximum " +
                                                               $"length of a greeting is `1,750` characters. " +
                                                               $"This message's length is `{message.Length:N0}` characters.");
                return;
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Custom Greeting",
                Description = "Successfully set this server's custom greeting to:\n\n" +
                              $"`{message}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"**This greeting is disabled!!** To enable your new greeting, use the {server.CommandPrefix}tg command."
                }
            };

            if (!server.CustomGreetingIsEnabled)
            {
                embed.Description += "$\n\n**This custom greeting is not enabled yet! " +
                                     $"Use the `{server.CommandPrefix}tg` command to enable!";
            }

            server.CustomGreeting = message;
            server.LogGreetings = Context.Channel.Id;
            await DatabaseQueries.UpdateAsync(server);
            await SendEmbedAsync(embed);
        }
    }
}
