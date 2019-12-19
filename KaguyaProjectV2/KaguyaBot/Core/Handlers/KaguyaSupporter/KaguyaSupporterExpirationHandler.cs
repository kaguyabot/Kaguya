using Discord;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaSupporter
{
    public static class KaguyaSupporterExpirationHandler
    {
        public static Task Start()
        {
            Timer timer = new Timer
            {
                Interval = 10000, //5 minutes
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += async (sender, args) =>
            {
                foreach (var suppKeyObject in await UtilityQueries.GetAllExpiredSupporterKeys())
                {
                    if (suppKeyObject.Expiration < DateTime.Now.ToOADate() && suppKeyObject.Expiration > 1)
                    {
                        var socketUser = ConfigProperties.client.GetUser(suppKeyObject.UserId);

                        try
                        {
                            var embed = new KaguyaEmbedBuilder
                            {
                                Title = "Supporter Tag Expiration",
                                Description = $"Hello {socketUser.Username},\n\n" +
                                              $"**Your Kaguya Supporter Tag has just expired!** If you would " +
                                              $"like to continue your subscription, you may purchase another " +
                                              $"tag [at this online store.](https://stageosu.selly.store/)\n\n" +
                                              $"Thank you for supporting my development while you have!",
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = "As a reminder, supporter keys do stack, so you may buy more than one and extend your subscription!"
                                }
                            };
                            embed.SetColor(EmbedColor.RED);

                            await socketUser.SendMessageAsync(embed: embed.Build());
                            await ConsoleLogger.Log($"User [Name: {socketUser} | ID: {socketUser.Id}] has been notified" +
                                              $" in DM that their supporter tag is now expired.", LogLevel.DEBUG);
                        }
                        catch (Exception)
                        {
                            if (suppKeyObject.UserId == 0) return;
                            await ConsoleLogger.Log($"I tried to send a DM to User [Name: {socketUser?.Username ?? "USER RETURNED NULL"} " +
                                              $"| ID: {socketUser?.Id}] about their expired supporter tag, " +
                                              $"but their DMs are closed.", LogLevel.DEBUG);
                        }

                        await UtilityQueries.DeleteSupporterKey(suppKeyObject);
                    }
                }
            };
            return Task.CompletedTask;
        }
    }
}
