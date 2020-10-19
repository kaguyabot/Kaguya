using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AutoAssignRole : KaguyaBase
    {
        [AdminCommand]
        [Command("AutoAssignRole")]
        [Alias("aar")]
        [Summary("Allows a server administrator to register a role, or a list of roles, " +
                 "as a role that automatically gets assigned to users as soon as they join " +
                 "the server.")]
        [Remarks("<role> {...}")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(params SocketRole[] roles)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            List<AutoAssignedRole> aars = server.AutoAssignedRoles.ToList();
            int limit = server.IsPremium ? 15 : 3;

            if (roles.Length > (limit - aars.Count))
            {
                throw new KaguyaPremiumException("Adding more than the allotted number of auto-assigned roles.\n" +
                                                 "For premium servers, this value is 15. For non-premium " +
                                                 "servers, this value is 3.");
            }

            foreach (SocketRole role in roles)
            {
                try
                {
                    var aar = new AutoAssignedRole
                    {
                        ServerId = server.ServerId,
                        RoleId = role.Id,
                        Server = server
                    };

                    if (!aars.Contains(aar))
                    {
                        await DatabaseQueries.InsertAsync(aar);
                        await ConsoleLogger.LogAsync($"Server added auto-assigned role. " +
                                                     $"[Guild {server.ServerId} | Role Id: {role.Id}", LogLvl.TRACE);
                    }
                }
                catch (Exception e)
                {
                    throw new KaguyaSupportException("An error occurred when trying to add your " +
                                                     "auto-assigned role.\n\n" +
                                                     $"Message: {e.Message}");
                }
            }

            string plural = roles.Length > 0
                ? "as roles"
                : "as a role";

            await SendBasicSuccessEmbedAsync($"Successfully added `{roles.Humanize()}` {plural} that will " +
                                             $"automatically be assigned to users upon join.");
        }
    }
}