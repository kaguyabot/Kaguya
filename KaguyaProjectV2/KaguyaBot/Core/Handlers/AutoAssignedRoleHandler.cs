using Discord.Net;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class AutoAssignedRoleHandler
    {
        public static async Task Trigger(SocketGuildUser u)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(u.Guild.Id);
            List<AutoAssignedRole> aars = server.AutoAssignedRoles.ToList();
            SocketGuild guild = u.Guild;

            foreach (AutoAssignedRole r in aars)
            {
                try
                {
                    SocketRole role = guild.GetRole(r.RoleId);
                    await u.AddRoleAsync(role);
                }
                catch (HttpException)
                {
                    await ConsoleLogger.LogAsync($"A Discord.Net.HttpException was thrown when trying to add a role " +
                                                 $"to user {u.Id} in guild {guild.Id}. I will remove " +
                                                 $"this auto assigned role from the database to " +
                                                 $"prevent further errors.", LogLvl.ERROR);

                    AutoAssignedRole roleToDelete = aars.First(x => x.RoleId == r.RoleId);
                    await DatabaseQueries.DeleteAsync(roleToDelete);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"A generic exception was thrown when trying to add a role " +
                                                 $"to user {u.Id} in guild {guild.Id}" +
                                                 $"\nMessage: {e.Message}" +
                                                 $"\nStack Trace: {e.StackTrace}", LogLvl.ERROR);
                }

                await ConsoleLogger.LogAsync($"Added auto-assigned role {r.RoleId} to user {u.Id} in guild " +
                                             $"{guild.Id}.", LogLvl.TRACE);
            }
        }
    }
}