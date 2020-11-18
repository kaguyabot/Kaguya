using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Helpers;
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
        public async Task Command(params string[] args)
        {
            var users = new List<SocketGuildUser>();
            var invalidUsers = new List<string>();
            
            foreach (string a in args)
            {
                SocketGuildUser user = DiscordHelpers.ParseGuildUser(a, Context.Guild);
                
                if (user != null)
                {
                    users.Add(user);
                }
                else
                {
                    invalidUsers.Add(a);
                }
            }

            // Embolden all users, format them in a comma separated list.
            string kickString = users.Humanize(x => x.ToString().ToDiscordBold(), "");
            var errorSb = new StringBuilder("Failed to kick users:\n");
            
            if(invalidUsers.Any())
                errorSb.AppendLine(invalidUsers.Humanize(x => x.ToDiscordBold(), ""));

            List<Task> awaiters = new List<Task>();
            foreach (SocketGuildUser user in users)
            {
                awaiters.Add(user.KickAsync());
            }

            await Task.WhenAll(awaiters);
            
            var finalSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(kickString))
                finalSb.AppendLine(kickString);

            if (!String.IsNullOrWhiteSpace(errorSb.ToString()))
                finalSb.AppendLine("\n\n" + errorSb);

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Masskick",
                Description = finalSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}