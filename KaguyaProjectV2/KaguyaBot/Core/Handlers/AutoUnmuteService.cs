using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class AutoUnmuteService
    {
        public static async Task CheckForUnmute()
        {
            Timer timer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += Unmute_Timer_Elapsed;
        }

        private static async void Unmute_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var mutedUser in ServerQueries.GetAllMutedUsers())
            {
                if (mutedUser.ExpiresAt < DateTime.Now.ToOADate())
                {
                    try
                    {
                        var guild = Global.ConfigProperties.client.GetGuild(mutedUser.ServerId);
                        var user = guild.GetUser(mutedUser.UserId);

                        var muteRole = guild.Roles.FirstOrDefault(x => x.Name == "kaguya-mute");

                        await user.RemoveRoleAsync(muteRole);
                    }
                    catch (Exception)
                    {
                        var guild = Global.ConfigProperties.client.GetGuild(mutedUser.ServerId);
                        await ConsoleLogger.Log($"Exception handled when unmuting a user in guild [Name: {guild.Name} | ID: {guild.Id}]", LogLevel.WARN);
                    }

                    ServerQueries.RemoveMutedUser(mutedUser);
                    await ConsoleLogger.Log($"User [ID: {mutedUser.UserId}] has been automatically unmuted.", LogLevel.TRACE);
                }
            }
        }
    }
}
