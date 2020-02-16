using Discord;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using User = KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models.User;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public static class RateLimitService
    {
        public static Task Initialize()
        {
            Timer timer = new Timer(3700); //3.70 seconds
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (sender, e) =>
            {
                var users = await DatabaseQueries.GetAllAsync<User>(x => x.ActiveRateLimit > 0 && x.UserId != 146092837723832320);

                foreach (var registeredUser in users)
                {
                    if (registeredUser.LastRatelimited < DateTime.Now.Add(TimeSpan.FromDays(-30)).ToOADate() &&
                        registeredUser.RateLimitWarnings > 0)
                    {
                        registeredUser.RateLimitWarnings = 0;
                        await ConsoleLogger.LogAsync($"User [ID: {registeredUser.UserId}] has had their Ratelimit Warnings reset " +
                                          $"due to not being ratelimited for 30 days.", LogLvl.INFO);
                    }

                    if (registeredUser.ActiveRateLimit >= 4 && !registeredUser.IsSupporter ||
                        registeredUser.ActiveRateLimit >= 7 && registeredUser.IsSupporter)
                    {
                        registeredUser.LastRatelimited = DateTime.Now.ToOADate();
                        registeredUser.RateLimitWarnings++;
                        if (registeredUser.RateLimitWarnings > 7 && registeredUser.ActiveRateLimit > 0)
                        {
                            var socketUser = ConfigProperties.Client.GetUser(registeredUser.UserId);

                            var _embed = new KaguyaEmbedBuilder(EmbedColor.RED)
                            {
                                Description = "You have exceeded your maximum allotment of ratelimit strikes, therefore " +
                                              "you will be permanently blacklisted."
                            };
                            await socketUser.SendMessageAsync(embed: _embed.Build());

                            var bl = new UserBlacklist
                            {
                                UserId = socketUser.Id,
                                Expiration = DateTime.MaxValue.ToOADate(),
                                Reason = "Ratelimit service: Automatic permanent blacklist for surpassing " +
                                         "7 ratelimit strikes in one month.",
                                User = registeredUser
                            };

                            registeredUser.ActiveRateLimit = 0;
                            await DatabaseQueries.UpdateAsync(registeredUser);
                            await DatabaseQueries.InsertOrReplaceAsync(bl);

                            await ConsoleLogger.LogAsync($"User [Name: {socketUser.Username} | ID: {socketUser.Id} | Supporter: {registeredUser.IsSupporter}] " +
                                                    "has been permanently blacklisted. Reason: Excessive Ratelimiting", LogLvl.WARN);
                            return;
                        }

                        var user = ConfigProperties.Client.GetUser(registeredUser.UserId);

                        string[] durations =
                        {
                            "60s", "5m", "30m",
                            "3h", "12h", "1d",
                            "3d"
                        };

                        List<TimeSpan> timeSpans = durations.Select(RegexTimeParser.ParseToTimespan).ToList();
                        string humanizedTime = timeSpans.ElementAt(registeredUser.RateLimitWarnings - 1).Humanize();

                        var tempBlacklist = new UserBlacklist
                        {
                            UserId = user.Id,
                            Expiration = (DateTime.Now + timeSpans.ElementAt(registeredUser.RateLimitWarnings - 1)).ToOADate(),
                            Reason = "Ratelimit service: Automatic permanent blacklist for surpassing " +
                                     "7 ratelimit strikes in one month.",
                            User = registeredUser
                        };

                        var embed = new KaguyaEmbedBuilder
                        {
                            Description = $"You have been ratelimited for `{humanizedTime}`\n\n" +
                                          $"For this time, you may not use any commands or earn experience points.",
                            Footer = new EmbedFooterBuilder
                            {
                                Text = $"You have {registeredUser.RateLimitWarnings} ratelimit strikes. Receiving " +
                                       $"{durations.Length - registeredUser.RateLimitWarnings} more strikes will result " +
                                       $"in a permanent blacklist."
                            }
                        };
                        embed.SetColor(EmbedColor.RED);

                        bool dm = true;

                        try
                        {
                            await user.SendMessageAsync(embed: embed.Build());
                        }
                        catch (NullReferenceException)
                        {
                            dm = false;
                        }

                        await ConsoleLogger.LogAsync($"User [Name: {user?.Username} | ID: {user?.Id} | Supporter: {registeredUser.IsSupporter}] " +
                                                $"has been ratelimited. Duration: {humanizedTime} Direct Message: {dm}", LogLvl.INFO);
                    }

                    if (registeredUser.ActiveRateLimit > 0)
                    {
                        registeredUser.ActiveRateLimit = 0;
                        await DatabaseQueries.UpdateAsync(registeredUser);
                    }
                }
            };

            return Task.CompletedTask;
        }
    }
}
