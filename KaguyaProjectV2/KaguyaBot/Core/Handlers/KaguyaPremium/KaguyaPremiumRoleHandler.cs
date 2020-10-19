using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaPremium
{
    public static class KaguyaPremiumRoleHandler
    {
        public static Task Initialize()
        {
            var timer = new Timer(300000); //5 mins = 300000ms
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    DiscordShardedClient client = ConfigProperties.Client;
                    SocketGuild kaguyaSupportServer = client.GetGuild(546880579057221644); //Kaguya Support Discord Server

                    SocketRole premiumRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 657104752559259659);
                    if (premiumRole == null)
                    {
                        await ConsoleLogger.LogAsync("The Kaguya Premium role was null when trying " +
                                                     "to apply it to a premium user!!", LogLvl.ERROR);

                        return;
                    }

                    // We can't say x.IsPremium as the ORM doesn't like this.
                    List<User> validPremium = await DatabaseQueries.GetAllAsync<User>(x => x.PremiumExpiration > DateTime.Now.ToOADate());
                    foreach (User premUser in validPremium)
                    {
                        SocketGuildUser guildUser = kaguyaSupportServer.GetUser(premUser.UserId);

                        if (guildUser == null)
                            continue;

                        if (guildUser.Roles.Contains(premiumRole))
                            continue;

                        await guildUser.AddRoleAsync(premiumRole);
                        await ConsoleLogger.LogAsync($"Premium Subscriber {guildUser} has had their {premiumRole.Name} " +
                                                     $"role given to them in the Kaguya Support server.", LogLvl.INFO);
                    }

                    // Check for expired premium users.
                    IEnumerable<SocketGuildUser> nonPremGuildUsers = kaguyaSupportServer.Users.Where(x => !validPremium.Any(y => y.UserId == x.Id));
                    foreach (SocketGuildUser socketUser in nonPremGuildUsers)
                    {
                        if (socketUser == null) continue;

                        if (!kaguyaSupportServer.Users.Any(x => x.Id == socketUser.Id))
                            continue;

                        SocketRole premRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 657104752559259659);
                        SocketGuildUser kaguyaSuppUser = kaguyaSupportServer.GetUser(socketUser.Id);

                        if (premRole is null)
                        {
                            var exception = new NullReferenceException("The Kaguya Premium role may not be null.");
                            await ConsoleLogger.LogAsync(exception);

                            return;
                        }

                        if (kaguyaSuppUser == null)
                        {
                            var exception = new NullReferenceException($"The user may not be null (ID: {socketUser.Id})");
                            await ConsoleLogger.LogAsync(exception);

                            return;
                        }

                        if (!kaguyaSuppUser.Roles.Contains(premRole))
                            continue;

                        await kaguyaSuppUser.RemoveRoleAsync(premRole);
                        await ConsoleLogger.LogAsync($"Expired premium subscriber {socketUser} " +
                                                     $"has had their {premRole.Name} role removed from them in the " +
                                                     $"Kaguya Support server.", LogLvl.INFO);
                    }
                }
                catch (OverflowException ex)
                {
                    await ConsoleLogger.LogAsync(ex);
                }
            };

            return Task.CompletedTask;
        }
    }
}