using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddRole : ModuleBase<ShardedCommandContext>
    {
        [Command("addrole")]
        [Alias("ar")]
        [Summary("Adds a singular role, or list of roles, to a user. New roles are separated by periods.")]
        [Remarks("ar <user> <role>.<role2> {...}\nar Stage Penguins.Space Monkeys.SomeWACKYRole")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task GiveRole(IGuildUser user, [Remainder]string args)
        {
            string[] roleNames = ArrayInterpreter.ReturnParams(args);

            int i = 0;

            foreach(string roleName in roleNames)
            {
                try
                {
                    await user.AddRoleAsync(Context.Guild.Roles.Where(x => x.Name.ToLower() == roleName.ToLower()).FirstOrDefault());
                    i++;
                }
                catch (Exception ex)
                {
                    await Logger.Log($"Exception thrown when adding role to user through command addrole: {ex.Message}", DataStorage.JsonStorage.LogLevel.WARN);
                }
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"{user.Username} has been given {i} roles."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}