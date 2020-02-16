using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaSupporter
{
    public static class KaguyaSupporterExpirationHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer
            {
                Interval = 300000, //5 minutes
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += async (sender, args) =>
            {
                var allKeys = await DatabaseQueries.GetAllAsync<SupporterKey>(x => x.Expiration < DateTime.Now.ToOADate());
                foreach (var suppKeyObject in allKeys)
                {
                    if (suppKeyObject.Expiration < DateTime.Now.ToOADate() && suppKeyObject.Expiration > 1)
                    {
                        var socketUser = ConfigProperties.Client.GetUser(suppKeyObject.UserId);

                        try
                        {
                            var embed = new KaguyaEmbedBuilder
                            {
                                Title = "Supporter Tag Expiration",
                                Description = $"Hello {socketUser.Username},\n\n" +
                                              $"**Your Kaguya Supporter Tag has just expired.** If you would " +
                                              $"like to continue your subscription, you may purchase another " +
                                              $"tag [at this online store.]({GlobalProperties.KAGUYA_STORE_URL})\n\n" +
                                              $"Thank you for supporting my development while you have!",
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = "As a reminder, supporter keys do stack, so you may buy more than one and extend your subscription!"
                                }
                            };
                            embed.SetColor(EmbedColor.RED);

                            await socketUser.SendMessageAsync(embed: embed.Build());
                            await ConsoleLogger.LogAsync($"User [Name: {socketUser} | ID: {socketUser.Id}] has been notified" +
                                              $" in DM that their supporter tag is now expired.", LogLvl.INFO);
                        }
                        catch (Exception)
                        {
                            if (suppKeyObject.UserId == 0) return;
                            await ConsoleLogger.LogAsync($"I tried to send a DM to User [Name: {socketUser?.Username ?? "USER RETURNED NULL"} " +
                                              $"| ID: {socketUser?.Id}] about their expired supporter tag, " +
                                              $"but their DMs appear to be closed.", LogLvl.WARN);
                        }

                        await DatabaseQueries.DeleteAsync(suppKeyObject);
                    }
                }
            };
            return Task.CompletedTask;
        }
    }
}
