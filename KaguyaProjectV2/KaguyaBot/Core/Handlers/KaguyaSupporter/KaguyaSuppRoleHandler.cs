using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using MoreLinq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using System.Collections.Generic;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaSupporter
{
    public static class KaguyaSuppRoleHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer(300000); //5 mins
            timer.Start();
            timer.AutoReset = true;
            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    var client = ConfigProperties.Client;
                    var kaguyaSupportServer = client.GetGuild(546880579057221644); //Kaguya Support Discord Server

                    var supporters = await DatabaseQueries.GetAllAsync<SupporterKey>(x =>
                    x.Expiration > DateTime.Now.ToOADate() &&
                    x.UserId != 0);
                    var premiumUsers = await DatabaseQueries.GetAllAsync<PremiumKey>(x =>
                    x.Expiration > DateTime.Now.ToOADate() &&
                    x.UserId != 0);

                    var validSupporters = new List<User>();
                    var validPremium = new List<User>();

                    foreach(var supporter in supporters)
                    {
                        var user = await DatabaseQueries.GetOrCreateUserAsync(supporter.UserId);
                        if(user.SupporterExpirationDate > DateTime.Now.ToOADate())
                            validSupporters.Add(user);
                    }

                    foreach(var prem in premiumUsers)
                    {
                        var user = await DatabaseQueries.GetOrCreateUserAsync(prem.UserId);
                        if(user.PremiumExpirationDate > DateTime.Now.ToOADate())
                            validPremium.Add(user);
                    }
                    
                    foreach (var supporter in validSupporters)
                    {
                        SocketUser socketUser = null;
                        try
                        {
                            socketUser = client.GetUser(supporter.UserId);
                        }
                        catch (Exception)
                        {
                            await ConsoleLogger.LogAsync($"Tried to get data for a supporter in hopes of " +
                                                        $"applying the Kaguya Supporter role to them, but they " +
                                                        $"couldn't be found. A null-reference exception was thrown.", LogLvl.WARN);
                            continue;
                        }

                        if (socketUser == null)
                            continue;

                        if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                            continue;

                        var supporterRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya supporter");
                        var kaguyaSuppGuildUser = kaguyaSupportServer.GetUser(socketUser.Id);

                        if (supporterRole is null)
                        {
                            await ConsoleLogger.LogAsync("The Kaguya Supporter role was found to be null " + 
                            $"when trying to apply it to user {socketUser}", LogLvl.ERROR);
                            continue;
                        }

                        if (kaguyaSuppGuildUser.Roles.Contains(supporterRole))
                            continue;

                        await kaguyaSuppGuildUser.AddRoleAsync(supporterRole);
                        await ConsoleLogger.LogAsync($"Supporter {socketUser} has had their {supporterRole.Name} role " +
                                                    $"given to them in the Kaguya Support server.", LogLvl.INFO);
                    }

                    foreach (var premUser in validPremium)
                    {
                        var socketUser = client.GetUser(premUser.UserId);

                        if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                            continue;

                        var premiumRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 657104752559259659);
                        var kaguyaSuppGuildUser = kaguyaSupportServer.GetUser(socketUser.Id);

                        if (premiumRole is null)
                        {
                            await ConsoleLogger.LogAsync("The Kaguya Premium role was found to be null " + 
                            $"when trying to apply it to user {socketUser}", LogLvl.ERROR);
                            continue;
                        }

                        if (kaguyaSuppGuildUser.Roles.Contains(premiumRole))
                            continue;

                        await kaguyaSuppGuildUser.AddRoleAsync(premiumRole);
                        await ConsoleLogger.LogAsync($"Premium Subscriber {socketUser} has had their {premiumRole.Name} role given to them in the Kaguya Support server.", LogLvl.INFO);
                    }

                    // Check for expired supporter tags.
                    var expiredSupporters = await DatabaseQueries.GetAllAsync<SupporterKey>(x => x.Expiration < DateTime.Now.ToOADate() && x.UserId != 0);
                    var expiredPremiumUsers = await DatabaseQueries.GetAllAsync<PremiumKey>(x => x.Expiration < DateTime.Now.ToOADate() && x.UserId != 0);

                    foreach (var expSupporter in expiredSupporters)
                    {
                        if(validSupporters.Any(x => x.UserId == expSupporter.UserId))
                            continue;

                        var socketUser = client.GetUser(expSupporter.UserId);
                        if (socketUser == null) continue;

                        if(!kaguyaSupportServer.Users.Contains(socketUser))
                            continue;

                        var supporterRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 569279637679505409);
                        var kaguyaSuppUser = kaguyaSupportServer.GetUser(socketUser.Id);

                        if (supporterRole is null)
                            throw new NullReferenceException("The Kaguya Supporter role may not be null.");

                        if (!kaguyaSuppUser.Roles.Contains(supporterRole))
                            continue;

                        await kaguyaSuppUser.RemoveRoleAsync(supporterRole);
                        await ConsoleLogger.LogAsync($"Expired supporter {socketUser} has had their {supporterRole.Name} role removed from them in the Kaguya Support server.", LogLvl.INFO);
                    }

                    foreach (var expPrem in expiredPremiumUsers)
                    {
                        // If the user is already identified as an active premium subscriber in one server, they aren't an expired user.
                        if (validPremium.Any(x => x.UserId == expPrem.UserId))
                            continue;

                        var socketUser = client.GetUser(expPrem.UserId);
                        if (socketUser == null) continue;

                        if(!kaguyaSupportServer.Users.Contains(socketUser))
                            continue;

                        var premRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 657104752559259659);
                        var kaguyaSuppUser = kaguyaSupportServer.GetUser(socketUser.Id);

                        if (premRole is null)
                            throw new NullReferenceException("The Kaguya Premium role may not be null.");

                        if (!kaguyaSuppUser.Roles.Contains(premRole))
                            continue;

                        await kaguyaSuppUser.RemoveRoleAsync(premRole);
                        await ConsoleLogger.LogAsync($"Expired premium subscriber {socketUser} has had their {premRole.Name} role " + 
                        "removed from them in the Kaguya Support server.", LogLvl.INFO);
                    }
                }
                catch(OverflowException ex)
                {
                    await ConsoleLogger.LogAsync(ex);
                }
            };
            return Task.CompletedTask;
        }
    }
}
