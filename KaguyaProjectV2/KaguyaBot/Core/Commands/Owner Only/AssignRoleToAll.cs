using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AssignRoleToAll : KaguyaBase
    {
        [OwnerCommand]
        [Command("AssignRoleToAll", RunMode = RunMode.Async)]
        [Alias("arta")]
        [Summary("Assigns the given role to every user in the server.")]
        [Remarks("<role>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(SocketRole role)
        {
            await ReplyAsync($"{Context.User.Mention} Executing...");
            int i = 0;

            await Context.Guild.DownloadUsersAsync();
            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.Roles.Contains(role)))
            {
                await user.AddRoleAsync(role);
                i++;
            }

            await SendBasicSuccessEmbedAsync($"Successfully added `{role.Name}` role to `{i:N0}` users.");
        }
    }
}