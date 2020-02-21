using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class RemoveAutoAssignedRole : KaguyaBase
    {
        [AdminCommand]
        [Command("RemoveAutoAssignedRole")]
        [Alias("raar", "aarr", "autoassignremove")]
        [Summary("Allows a server administrator to remove a role, or a list of roles, " +
                 "from the existing list of auto-assigned roles for this server.")]
        [Remarks("<role> {...}")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command(params SocketRole[] roles)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var aars = server.AutoAssignedRoles.ToList();

            int i = 0;
            foreach (var role in roles)
            {
                if (aars.Any(x => x.RoleId == role.Id))
                {
                    await DatabaseQueries.DeleteAsync(aars.First(x => x.RoleId == role.Id));
                    await ConsoleLogger.LogAsync($"Deleted role {role.Id} from guild {Context.Guild.Id}'s " +
                                                 $"list of auto-assigned roles.", LogLvl.TRACE);
                    i++;
                }
            }

            if (i == 0)
            {
                await SendBasicErrorEmbedAsync($"There were no records of this role in your list " +
                                               $"of auto-assigned roles.");
                return;
            }

            if (i < roles.Length)
            {
                await SendBasicSuccessEmbedAsync($"Successfully found {i} matching roles and deleted them. " +
                                               $"This is less than the amount of roles you specified, though, " +
                                               $"which means the remaining roles don't exist for this server's " +
                                               $"list of automatically assigned roles.");
            }

            if (i == roles.Length)
            {
                await SendBasicSuccessEmbedAsync($"Successfully removed {roles.Humanize()} from " +
                                                 $"this server's list of automatically assigned roles.");
            }

            //todo: Test.
        }
    }
}
