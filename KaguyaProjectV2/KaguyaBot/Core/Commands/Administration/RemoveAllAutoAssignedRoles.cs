using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class RemoveAllAutoAssignedRoles : KaguyaBase
    {
        [AdminCommand]
        [Command("RemoveAllAutoAssignedRoles")]
        [Alias("raaar", "raar all", "raar -all")]
        [Summary("Allows a server administrator to remove " +
                 "all auto-assigned roles from the current server's " +
                 "list of auto-assigned roles.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var aars = server.AutoAssignedRoles.ToList();

            if (aars.Count == 0)
            {
                await SendBasicErrorEmbedAsync("There are no auto-assigned roles to remove.");
                return;
            }
            await DatabaseQueries.DeleteAsync(aars);
            await SendBasicSuccessEmbedAsync($"Successfully deleted all auto-assigned roles " +
                                             $"from the database.");
        }
    }
}
