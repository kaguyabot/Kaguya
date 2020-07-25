using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaPremium
{
    public static class KaguyaPremiumRoleHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer(300000); //5 mins = 300000ms
            timer.Start();
            timer.AutoReset = true;
            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    var client = ConfigProperties.Client;
                    var kaguyaSupportServer = client.GetGuild(546880579057221644); //Kaguya Support Discord Server

                    var premiumUsers = await DatabaseQueries.GetAllAsync<PremiumKey>(x =>
                    x.Expiration > DateTime.Now.ToOADate() && x.UserId != 0);

                    var validPremium = new List<User>();
                    
                    foreach(var prem in premiumUsers)
                    {
                        var user = await DatabaseQueries.GetOrCreateUserAsync(prem.UserId);
                        if(user.PremiumExpirationDate > DateTime.Now.ToOADate())
                            validPremium.Add(user);
                    }
                    
                    var premiumRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Id == 657104752559259659);
                    if (premiumRole == null)
                    {
                        await ConsoleLogger.LogAsync("The Kaguya Premium role was found to be null when trying " +
                                                     "to apply it to a premium user!!", LogLvl.ERROR);
                        return;
                    }
                    
                    foreach (var premUser in validPremium)
                    {
                        var guildUser = kaguyaSupportServer.GetUser(premUser.UserId); 

                        if (guildUser.Roles.Contains(premiumRole))
                            continue;

                        await guildUser.AddRoleAsync(premiumRole);
                        await ConsoleLogger.LogAsync($"Premium Subscriber {guildUser} has had their {premiumRole.Name} role given to them in the Kaguya Support server.", LogLvl.INFO);
                    }

                    // Check for expired premium users.
                    var expiredKeys = await DatabaseQueries.GetAllAsync<PremiumKey>(x => x.Expiration < DateTime.Now.ToOADate() && x.UserId != 0);
                    foreach (var expKey in expiredKeys)
                    {
                        // If the user is already identified as an active premium subscriber in one server, they aren't an expired user.
                        if (validPremium.Any(x => x.UserId == expKey.UserId))
                            continue;

                        var socketUser = client.GetUser(expKey.UserId);
                        if (socketUser == null) continue;

                        if(!kaguyaSupportServer.Users.Any(x => x.Id == socketUser.Id))
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
