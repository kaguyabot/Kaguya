﻿using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class DeleteRole : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("DeleteRole")]
        [Alias("dr")]
        [Summary("Deletes a role, or a list of roles, from the server. New roles are separated by periods.")]
        [Remarks("<role>.<role2>.{...}\nPenguins.Some long role.Moofins")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRole([Remainder]string targetRole)
        {
            string[] roleNames = ArrayInterpreter.ReturnParams(targetRole);
            List<IRole> roles = new List<IRole>();

            foreach(string element in roleNames)
            {
                var rolesFound = Context.Guild.Roles.Where(r => r.Name.ToLower() == element.ToLower()).ToList();
                foreach(var socketRole in rolesFound)
                {
                    roles.Add(socketRole);
                }
            }

            if (roles.Count() > 1)
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Title = "Roles Deleted"
                };

                foreach (var role in roles)
                {
                    embed.AddField("Role Deleted", $"`{role.Name}` with `{role.Permissions.ToList().Count()}` permissions has been deleted.");
                    await role.DeleteAsync();
                }
                await ReplyAsync(embed: embed.Build());
            }
            else if (roles.Count() == 1)
            {
                var role = roles.First();
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"**{Context.User.Mention} Successfully deleted role `{role.Name}`**"
                };
                await role.DeleteAsync();

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"**{Context.User.Mention} I could not find the specified role!**"
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}