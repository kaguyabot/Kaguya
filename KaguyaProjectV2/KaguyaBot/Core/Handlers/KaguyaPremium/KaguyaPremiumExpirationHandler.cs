using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaPremium
{
    public static class KaguyaPremiumExpirationHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer
            {
                Interval = 300000, // 5 minutes
                Enabled = true,
                AutoReset = true
            };

            //todo: Rewrite the timer to check for when the individual server or user's premium expiration time has 
            //todo: been passed by the current time.
            // timer.Elapsed += async (sender, args) =>
            // {
            //     var allPremKeys = await DatabaseQueries.GetAllAsync<PremiumKey>(x => x.Expiration < DateTime.Now.ToOADate());
            //     foreach (var premKey in allPremKeys)
            //     {
            //         if (premKey.Expiration < DateTime.Now.ToOADate() && premKey.Expiration > 1)
            //         {
            //             if (await (await DatabaseQueries.GetOrCreateUserAsync(premKey.UserId)).IsPremium)
            //                 continue;
            //             var socketUser = ConfigProperties.Client.GetUser(premKey.UserId);
            //
            //             try
            //             {
            //                 var embed = new KaguyaEmbedBuilder
            //                 {
            //                     Title = "Kaguya Premium Expiration",
            //                     Description = $"Hello {socketUser.Username},\n\n" +
            //                                   $"**The Kaguya Premium key you redeemed for " +
            //                                   $"`{ConfigProperties.Client.GetGuild(premKey.ServerId).Name}` has " +
            //                                   $"just expired.** If you would " +
            //                                   $"like to continue this subscription, you may purchase another " +
            //                                   $"key [at this online store.]({ConfigProperties.KaguyaStore})\n\n" +
            //                                   $"Thank you for supporting my development while you have!",
            //                     Footer = new EmbedFooterBuilder
            //                     {
            //                         Text = "As a reminder, supporter and premium keys do stack, " +
            //                                "so you may buy more than one and extend your subscription!"
            //                     }
            //                 };
            //                 embed.SetColor(EmbedColor.RED);
            //
            //                 await socketUser.SendMessageAsync(embed: embed.Build());
            //                 await ConsoleLogger.LogAsync($"User [Name: {socketUser} | ID: {socketUser.Id}] has been notified" +
            //                                   $" in DM that their Kaguya Premium key has just expired for " +
            //                                   $"guild {premKey.ServerId}", LogLvl.INFO);
            //             }
            //             catch (Exception)
            //             {
            //                 if (premKey.UserId == 0) return;
            //                 await ConsoleLogger.LogAsync($"I tried to send a DM to User [Name: {socketUser?.Username ?? "USER RETURNED NULL"} " +
            //                                   $"| ID: {socketUser?.Id}] about their expired Kaguya Premium key for " +
            //                                   $"guild {premKey.ServerId}, but their DMs appear to be closed.", LogLvl.WARN);
            //             }
            //
            //             await DatabaseQueries.DeleteAsync(premKey);
            //         }
            //     }
            // };
            return Task.CompletedTask;
        }
    }
}
