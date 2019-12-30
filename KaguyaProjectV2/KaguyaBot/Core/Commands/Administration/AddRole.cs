using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddRole : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("AddRole")]
        [Alias("ar")]
        [Summary("Takes a user and assigns them a role or list of roles. If a role has a space in the name, " +
                 "surround it with quotation marks. New roles are separated by spaces.")]
        [Remarks("<user> <role> {...}")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task GiveRole(IGuildUser user, params string[] args)
        {
            int i = 0;
            foreach(string roleName in args)
            {
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
                try
                {
                    await user.AddRoleAsync(role);
                    i++;
                }
                catch (Exception ex)
                {
                    await ConsoleLogger.Log($"Exception thrown when adding role to user through command addrole: {ex.Message}", LogLevel.WARN);
                }
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"`{user.Username}` has been given `{i.ToWords()}` roles."
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}