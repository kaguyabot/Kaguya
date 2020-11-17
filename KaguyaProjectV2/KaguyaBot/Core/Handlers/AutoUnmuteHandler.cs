using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class AutoUnmuteHandler
    {
        public static Task Initialize()
        {
            var timer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += async (_, _) =>
            {
                List<MutedUser> curMutedUsers = await DatabaseQueries.GetAllAsync<MutedUser>(x => x.ExpiresAt < DateTime.Now.ToOADate());
                foreach (MutedUser mutedUser in curMutedUsers)
                {
                    SocketGuild guild = ConfigProperties.Client.GetGuild(mutedUser.ServerId);

                    if (guild == null)
                        goto RemoveFromDB;

                    Server server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);
                    SocketGuildUser user = guild.GetUser(mutedUser.UserId);
                    SocketGuildUser selfUser = guild.GetUser(ConfigProperties.Client.CurrentUser.Id);

                    try
                    {
                        SocketRole muteRole = guild.Roles.FirstOrDefault(x => x.Name == "kaguya-mute");
                        await user.RemoveRoleAsync(muteRole);
                        
                        KaguyaEvents.TriggerUnmute(new ModeratorEventArgs(server, guild, user, selfUser, 
                            "Automatic unmute (timed mute has expired)", null));
                    }
                    catch (Exception)
                    {
                        await ConsoleLogger.LogAsync($"Exception handled when unmuting a user in guild [Name: {guild.Name} | ID: {guild.Id}]",
                            LogLvl.WARN);
                    }

                    await DatabaseQueries.UpdateAsync(server);

                    RemoveFromDB:
                    await DatabaseQueries.DeleteAsync(mutedUser);
                    await ConsoleLogger.LogAsync($"User [ID: {mutedUser.UserId}] has been automatically unmuted.",
                        LogLvl.DEBUG);
                }
            };

            return Task.CompletedTask;
        }
    }
}