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
                var client = ConfigProperties.Client;
                var kaguyaSupportServer = client.GetGuild(546880579057221644); //Kaguya Support Discord Server

                var supporters = await DatabaseQueries.GetAllAsync<SupporterKey>(x =>
                    x.Expiration > DateTime.Now.ToOADate() &&
                    x.UserId != 0);
                var premiumUsers = await DatabaseQueries.GetAllAsync<PremiumKey>(x =>
                    x.Expiration > DateTime.Now.ToOADate() &&
                    x.UserId != 0);

                // Get rid of duplicates.
                supporters = supporters.DistinctBy(x => x.UserId).ToList();
                premiumUsers = premiumUsers.DistinctBy(x => x.UserId).ToList();

                foreach (var supporter in supporters)
                {
                    var socketUser = client.GetUser(supporter.UserId);

                    if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                        continue;

                    var supporterRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("supporter"));
                    var kaguyaSuppGuildUser = kaguyaSupportServer.GetUser(socketUser.Id);

                    if (supporterRole is null)
                        throw new NullReferenceException("The Kaguya Supporter role may not be null.");

                    if (kaguyaSuppGuildUser.Roles.Contains(supporterRole))
                        continue;

                    await kaguyaSuppGuildUser.AddRoleAsync(supporterRole);
                    await ConsoleLogger.LogAsync($"Supporter {socketUser} has had their {supporterRole.Name} role given to them in the Kaguya Support server.", LogLvl.INFO);
                }

                foreach (var premUser in premiumUsers)
                {
                    var socketUser = client.GetUser(premUser.UserId);

                    if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                        continue;

                    var premiumRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("premium"));
                    var kaguyaSuppGuildUser = kaguyaSupportServer.GetUser(socketUser.Id);

                    if (premiumRole is null)
                        throw new NullReferenceException("The Kaguya Premium role may not be null.");

                    if (kaguyaSuppGuildUser.Roles.Contains(premiumRole))
                        continue;

                    await kaguyaSuppGuildUser.AddRoleAsync(premiumRole);
                    await ConsoleLogger.LogAsync($"Premium Subscriber {socketUser} has had their {premiumRole.Name} role given to them in the Kaguya Support server.", LogLvl.INFO);
                }

                // Check for expired supporter tags.

                var expiredSupporters = await DatabaseQueries.GetAllAsync<SupporterKey>(x => x.Expiration < DateTime.Now.ToOADate());
                var expiredPremiumUsers = await DatabaseQueries.GetAllAsync<PremiumKey>(x => x.Expiration < DateTime.Now.ToOADate());

                foreach (var expSupporter in expiredSupporters)
                {
                    var socketUser = client.GetUser(expSupporter.UserId);
                    if (socketUser == null) continue;

                    if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                        continue;

                    var supporterRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("supporter"));
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
                    if (premiumUsers.Any(x => x.UserId == expPrem.UserId))
                        continue;

                    var socketUser = client.GetUser(expPrem.UserId);
                    if (socketUser == null) continue;

                    if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                        continue;

                    var premRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("premium"));
                    var kaguyaSuppUser = kaguyaSupportServer.GetUser(socketUser.Id);

                    if (premRole is null)
                        throw new NullReferenceException("The Kaguya Premium role may not be null.");

                    if (!kaguyaSuppUser.Roles.Contains(premRole))
                        continue;

                    await kaguyaSuppUser.RemoveRoleAsync(premRole);
                    await ConsoleLogger.LogAsync($"Expired premium subscriber {socketUser} has had their {premRole.Name} role removed from them in the Kaguya Support server.", LogLvl.INFO);
                }
            };
            return Task.CompletedTask;
        }
    }
}
