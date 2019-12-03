using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class AutoUnmuteHandler
    {
        public static Task CheckForUnmute()
        {
            Timer timer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += async (sender, e) =>
            {
                foreach (var mutedUser in ServerQueries.GetAllMutedUsers())
                {
                    if (mutedUser.ExpiresAt < DateTime.Now.ToOADate())
                    {
                        try
                        {
                            var guild = Global.ConfigProperties.client.GetGuild(mutedUser.ServerId);
                            var server = ServerQueries.GetServer(guild.Id);
                            var user = Global.ConfigProperties.client.GetGuild(server.Id).GetUser(mutedUser.UserId);

                            if (server.IsPremium)
                            {
                                server.TotalAdminActions++;
                                ServerQueries.UpdateServer(server);

                                await PremiumModerationLog.SendModerationLog(new PremiumModerationLog
                                {
                                    Server = server,
                                    Moderator = Global.ConfigProperties.client.GetGuild(server.Id)
                                        .GetUser(538910393918160916),
                                    ActionRecipient = user,
                                    Action = PremiumModActionHandler.UNMUTE,
                                    Reason = "User was automatically unmuted because their timed mute has expired."
                                });
                            }

                            var muteRole = guild.Roles.FirstOrDefault(x => x.Name == "kaguya-mute");
                            await user.RemoveRoleAsync(muteRole);
                        }
                        catch (Exception)
                        {
                            var guild = Global.ConfigProperties.client.GetGuild(mutedUser.ServerId);
                            await ConsoleLogger.Log(
                                $"Exception handled when unmuting a user in guild [Name: {guild.Name} | ID: {guild.Id}]",
                                LogLevel.WARN);
                        }

                        ServerQueries.RemoveMutedUser(mutedUser);
                        await ConsoleLogger.Log($"User [ID: {mutedUser.UserId}] has been automatically unmuted.",
                            LogLevel.DEBUG);
                    }
                }
            };
            return Task.CompletedTask;
        }
    }
}
