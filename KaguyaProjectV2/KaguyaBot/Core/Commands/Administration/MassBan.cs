using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class MassBan : KaguyaBase
    {
        [AdminCommand]
        [Command("MassBan")]
        [Alias("mb")]
        [Summary("Allows a server moderator with the `Ban Members` permission to ban a user, or list of users, " +
            "from the server. A default reason will be provided in the Audit Log.")]
        [Remarks("<user> {...}")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Command(params SocketGuildUser[] args)
        {
            var banSb = new StringBuilder();
            var errorSb = new StringBuilder();
            foreach (SocketGuildUser user in args)
            {
                try
                {
                    banSb.AppendLine($"Banned user `{user}`.");
                    await user.BanAsync(5, $"Massban operation from user " +
                                           $"@{Context.User.Username}#{Context.User.Discriminator}");
                }
                catch (Exception e)
                {
                    errorSb.AppendLine($"Failed to ban user `{user}`");
                }
            }

            var finalSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(banSb.ToString()))
                finalSb.AppendLine(banSb.ToString());

            if (!String.IsNullOrWhiteSpace(errorSb.ToString()))
                finalSb.AppendLine($"\n\n{errorSb}");

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Massban",
                Description = finalSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}