using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using MoreLinq;
using System;
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
                    var curMonthlyVotes = voters.Count(x => x.Id == voter.Id);
                    var dbVotes = (await DatabaseQueries.GetAllForUserAsync<Upvote>(voter.Id))?.OrderByDescending(x => x.Time).ToList();
                    var mostRecentVote = dbVotes == null || dbVotes.Count == 0 ? null : dbVotes[0];

                    // If they are owed at least one vote based on how many votes the api reports back to us
                    // compared to how many votes are in the database for this user.
                    if (dbVotes == null || curMonthlyVotes > dbVotes.Count)
                    {
                        if (mostRecentVote == null || mostRecentVote.Time < DateTime.Now.AddHours(-12).ToOADate() && mostRecentVote.ReminderSent)
                        {
                            var socketUser = ConfigProperties.Client.GetUser(voter.Id);

                            var r = new Random();
                            var points = r.Next(150, 600);
                            var xp = r.Next(50, 275);

                            string nsfwStr = user.HasRecentlyUsedNSFWCommands() ? "Your NSFW image cooldown has also been refreshed." : "";

                            var embed = new KaguyaEmbedBuilder(EmbedColor.GOLD)
                            {
                                Title = $"Upvote Rewards",
                                Description = $"Thanks for upvoting Kaguya on [top.gg](https://top.gg/bot/538910393918160916/vote)!\n" +
                                              $"You've been rewarded with `{points:N0} points` and `{xp:N0} global exp`! {nsfwStr}",
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = "You may earn rewards again in 12 hours."
                                }
                            };

                            try
                            {
                                var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
                                await dmChannel.SendEmbedAsync(embed);
                            }
                            catch (Exception)
                            {
                                await ConsoleLogger.LogAsync(
                                    $"Tried to DM a user their upvote rewards, but an exception was thrown when " +
                                    $"trying to message them.", LogLvl.DEBUG);
                            }

                            var vote = new Upvote
                            {
                                VoteId = Guid.NewGuid().ToString(),
                                UserId = voter.Id,
                                Time = DateTime.Now.ToOADate(),
                                PointsAwarded = points,
                                ExpAwarded = xp,
                                ReminderSent = false
                            };

                            user.TotalNSFWImages = 12;
                            user.Points += points;
                            user.Experience += xp;
                            user.TotalUpvotes++;

                            await DatabaseQueries.UpdateAsync(user);
                            await DatabaseQueries.InsertAsync(vote);

                            await ConsoleLogger.LogAsync($"User {voter.Id} has successfully upvoted Kaguya.", LogLvl.DEBUG);
                        }
                    }
                }
            };

            Timer upvoteResetTimer = new Timer(60000);
            upvoteResetTimer.Enabled = true;
            upvoteResetTimer.AutoReset = true;
            upvoteResetTimer.Elapsed += async (sender, e) =>
            {
                var voters = (await api.GetVotersAsync()).DistinctBy(x => x.Id).ToList();

                foreach (var voter in voters)
                {
                    var votes = (await DatabaseQueries.GetAllForUserAsync<Upvote>(voter.Id))?.OrderByDescending(x => x.Time).ToList();
                    var mostRecentVote = votes == null || votes.Count == 0 ? null : votes[0];

                    if (mostRecentVote == null)
                    {
                        await ConsoleLogger.LogAsync($"User {voter.Id} may now vote again, but " +
                                                     $"their most recent vote was null in the database. " +
                                                     $"No DM has been sent, and nothing has been " +
                                                     $"updated in the database.", LogLvl.WARN);
                        return;
                    }

                    //if (mostRecentVote.Time > DateTime.Now.AddHours(-12).ToOADate())
                    //    return;

                    if (mostRecentVote.ReminderSent)
                        return;

                    var socketUser = ConfigProperties.Client.GetUser(voter.Id);
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
                        await ConsoleLogger.LogAsync(
                            $"Tried to DM a user their upvote reminder, but an exception was thrown when " +
                            $"trying to message them.", LogLvl.DEBUG);
                    }

                    UpdateInDB:
                    mostRecentVote.ReminderSent = true;

                    /*
                     * The reason for creating a brand new upvote and deleting the old one from the database
                     * is due to the fact that Upvote objects don't have a primary key. There can be multiple
                     * upvotes in the database belonging to the same UserId. Therefore, we can't simply
                     * update the object.
                     */

                    await DatabaseQueries.UpdateAsync(mostRecentVote);

                    if (socketUser == null)
                    {
                        await ConsoleLogger.LogAsync($"User {voter.Id} can now upvote Kaguya. DM NOT sent.", LogLvl.DEBUG);
                        return;
                    }

                    await ConsoleLogger.LogAsync($"User {voter.Id} has been reminded to upvote Kaguya.", LogLvl.DEBUG);
                }
            };
        }
    }
}
