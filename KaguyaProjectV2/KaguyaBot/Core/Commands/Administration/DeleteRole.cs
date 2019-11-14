using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class DeleteRole : ModuleBase<ShardedCommandContext>
    {
        [Command("deleterole")]
        [Alias("dr")]
        [Summary("Deletes a role, or a list of roles, from the server. New roles are separated by periods.")]
        [Remarks("dr <role>.<role2>.{...}\ndr Stage Penguins.Some long role.Moofins")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRole([Remainder]string targetRole)
        {
            var roles = Context.Guild.Roles.Where(r => r.Name.ToLower() == targetRole.ToLower()).ToList();
            if (roles.Count() > 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Role Deletion: Multiple Matching Roles",
                    Description = $"{roles.Count()} has been given {targetRole} roles."
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
                    Title = "Role Deletion: Success",
                    Description = $"**{Context.User.Mention} Successfully deleted role `{role.Name}`**"
                };
                await role.DeleteAsync();

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Role Deletion: Error",
                    Description = $"**{Context.User.Mention} I could not find the specified role!**"
                };
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
