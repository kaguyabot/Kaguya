using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class RemoveRoleReward : KaguyaBase
    {
        [ExpCommand]
        [Command("RemoveRoleReward")]
        [Alias("rrr")]
        [Summary("Allows an administrator to remove a role from being given out as a " +
                 "level-up reward, or disable all roles for being given out for a given level.")]
        [Remarks("<level>\n<role name or ID>\nSomeRoleName\n10\n675870125353861160")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder]string arg)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            SocketRole role = null;
            int level = 0;

            if (arg.AsInteger(false) == 0 && arg.AsUlong(false) == 0)
            {
                role = Context.Guild.Roles.First(x => x.Name.ToLower() == arg.ToLower());
            }

            if (arg.AsUlong(false) != 0 && arg.AsInteger(false) > 100000)
            {
                role = Context.Guild.Roles.First(x => x.Id == arg.AsUlong());
            }

            if (arg.AsInteger(false) != 0 && arg.AsInteger(false) <= 100000)
            {
                level = arg.AsInteger();
            }

            if (role == null && level == 0 && server.RoleRewards.Any(x => x.RoleId != arg.AsUlong()))
            {
                await SendBasicErrorEmbedAsync("You did not reply with a proper level or role name/ID.");
                return;
            }

            if (role != null && level == 0 || server.RoleRewards.Any(x => x.RoleId == arg.AsUlong()))
            {
                ServerRoleReward toRemove = role == null ?
                    server.RoleRewards.ToList().First(x => x.RoleId == arg.AsUlong()) :
                    server.RoleRewards.First(x => x.RoleId == role.Id);

                if (toRemove == null)
                {
                    throw new KaguyaSupportException("The specified role could not be found in this server's " +
                                                     "role rewards list.");
                }

                await DatabaseQueries.DeleteAsync(toRemove);
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Successfully removed role " +
                                                 $"{(role == null ? $"`{toRemove.RoleId}`" : role.Mention)} at `level " +
                                                 $"{toRemove.Level}` from this server's role rewards.");
                return;
            }

            if (role == null && level != 0)
            {
                var toRemove = server.RoleRewards.Where(x => x.Level == level).ToList();
                if (toRemove.Count != 0)
                {
                    await DatabaseQueries.DeleteAsync(toRemove);

                    await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Successfully deleted " +
                                                     $"all roles for the server that were given " +
                                                     $"at `level {level:N0}`.");
                    return;
                }

                throw new KaguyaSupportException($"There are no role rewards for this server at `level {level:N0}`");
            }

            throw new KaguyaSupportException("There was nothing to remove.");
        }
    }
}
