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
    public class MassKick : KaguyaBase
    {
        [AdminCommand]
        [Command("MassKick")]
        [Alias("mk")]
        [Summary("Allows a server moderator with the `Kick Members` permission to kick a user, or list of users, " +
            "out of the server. A default reason will be provided in the Audit Log.")]
        [Remarks("<user> {...}")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Command(params SocketGuildUser[] args)
        {
            var kickSb = new StringBuilder();
            var errorSb = new StringBuilder();
            foreach (SocketGuildUser user in args)
            {
                try
                {
                    kickSb.AppendLine($"Kicked user `{user}`.");
                    await user.KickAsync($"Masskick operation from user " +
                                         $"@{Context.User.Username}#{Context.User.Discriminator}");
                }
                catch (Exception e)
                {
                    errorSb.AppendLine($"Failed to kick user `{user}`");
                }
            }

            var finalSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(kickSb.ToString()))
                finalSb.AppendLine(kickSb.ToString());

            if (!String.IsNullOrWhiteSpace(errorSb.ToString()))
                finalSb.AppendLine($"\n\n{errorSb}");

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Masskick",
                Description = finalSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}