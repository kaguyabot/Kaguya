using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
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

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class TopGGUpvoteHandler
    {
        public static async Task Initialize()
        {
            var api = ConfigProperties.TopGGApi;

            Timer timer = new Timer(15000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (sender, args) =>
            {
                var voters = await api.GetVotersAsync();
                
                foreach (var voter in voters)
                {
                    var user = await DatabaseQueries.GetOrCreateUserAsync(voter.Id);

                    if (user.LastUpvoted < DateTime.Now.AddHours(-24).ToOADate())
                    {
                        var socketUser = ConfigProperties.Client.GetUser(voter.Id);

                        var r = new Random();
                        var points = r.Next(150, 600);
                        var xp = r.Next(50, 275);

                        string nsfwStr = user.HasRecentlyUsedNSFWCommands() ? "Your NSFW image cooldown has also been refreshed." : "";

                        var embed = new KaguyaEmbedBuilder
                        {
                            Title = $"Upvote Rewards",
                            Description = $"Thanks for upvoting Kaguya on [top.gg](https://top.gg/bot/538910393918160916/vote)!\n" +
                                          $"You've been rewarded with `{points:N0} points` and `{xp:N0} global exp`! {nsfwStr}",
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "You may earn rewards again in 24 hours."
                            }
                        };

                        try
                        {
                            var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
                            await dmChannel.SendEmbedAsync(embed);
                        }
                        catch (Exception)
                        {
                            //
                        }

                        user.LastUpvoted = DateTime.Now.ToOADate();
                        user.TotalNSFWImages = 12;
                        user.Points += points;
                        user.Experience += xp;
                        user.UpvoteReminderSent = false;
                        user.TotalUpvotes++;
                        await DatabaseQueries.UpdateAsync(user);

                        await ConsoleLogger.LogAsync($"User {voter.Id} has successfully upvoted Kaguya.", LogLvl.DEBUG);
                    }
                }
            };

            Timer upvoteResetTimer = new Timer(20000);
            upvoteResetTimer.Enabled = true;
            upvoteResetTimer.AutoReset = true;
            upvoteResetTimer.Elapsed += async (sender, e) =>
            {
                var curVoters = await api.GetVotersAsync();
                var previousVoters = await DatabaseQueries.GetAllAsync<User>(x =>
                    x.LastUpvoted < DateTime.Now.AddHours(-24).ToOADate() && 
                    !x.UpvoteReminderSent);

                var matches = new List<User>();

                foreach (var voter in curVoters)
                {
                    var user = previousVoters.FirstOrDefault(x => x.UserId == voter.Id);
                    if (user != null)
                    {
                        matches.Add(user);
                    }
                }

                foreach (var voter in matches)
                {
                    var socketUser = ConfigProperties.Client.GetUser(voter.UserId);
                    if (socketUser == null) goto UpdateInDB;

                    var embed = new KaguyaEmbedBuilder(EmbedColor.PINK)
                    {
                        Title = "Kaguya Upvote Reminder",
                        Description = $"You may now vote for Kaguya on [top.gg](https://top.gg/bot/538910393918160916/vote) " +
                                      $"for bonus rewards!"
                    };

                    try
                    {
                        var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
                        await dmChannel.SendEmbedAsync(embed);
                    }
                    catch (Exception)
                    {
                        //
                    }

                    UpdateInDB:
                    voter.UpvoteReminderSent = true;
                    await DatabaseQueries.UpdateAsync(voter);

                    if (socketUser == null)
                    {
                        await ConsoleLogger.LogAsync($"User {voter.UserId} can now upvote Kaguya. DM NOT sent.", LogLvl.DEBUG);
                        return;
                    }

                    await ConsoleLogger.LogAsync($"User {voter.UserId} has been reminded to upvote Kaguya.", LogLvl.DEBUG);
                }
            };
        }
    }
}
