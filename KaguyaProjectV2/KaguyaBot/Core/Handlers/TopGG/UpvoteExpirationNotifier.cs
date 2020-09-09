using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG
{
    public class UpvoteExpirationNotifier
    {
        public static async Task Initialize()
        {
            Timer upvoteResetTimer = new Timer(60000);
            upvoteResetTimer.Enabled = true;
            upvoteResetTimer.AutoReset = true;
            upvoteResetTimer.Elapsed += async (sender, e) =>
            {
                var now = DateTime.Now;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
                var monthlyVoters = await DatabaseQueries.GetAllAsync<TopGgWebhook>(x => x.TimeVoted > firstDayOfMonth.ToOADate()
                                                                                         && x.UpvoteType.ToLower() != "test");

                foreach (var voter in monthlyVoters)
                {
                    var votes = monthlyVoters.Where(x => x.UserId == voter.UserId).OrderByDescending(x => x.TimeVoted).ToList();
                    var mostRecentVote = !votes.Any() ? null : votes[0];

                    if (mostRecentVote == null)
                    {
                        await ConsoleLogger.LogAsync($"User {voter.UserId} may now vote again, but " +
                                                     $"their most recent vote was null in the database. " +
                                                     $"No DM has been sent, and nothing has been " +
                                                     $"updated in the database.", LogLvl.DEBUG);
                        continue;
                    }

                    if (mostRecentVote.TimeVoted > DateTime.Now.AddHours(-12).ToOADate())
                        continue;

                    if (mostRecentVote.ReminderSent)
                        continue;

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
                        await ConsoleLogger.LogAsync(
                            $"Tried to DM a user their upvote reminder, but an exception was thrown when " +
                            $"trying to message them.", LogLvl.WARN);
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
                        await ConsoleLogger.LogAsync($"User {voter.UserId} can now upvote Kaguya. DM NOT sent.", LogLvl.DEBUG);
                        continue;
                    }

                    await ConsoleLogger.LogAsync($"User {voter.UserId} has been reminded to upvote Kaguya.", LogLvl.DEBUG);
                }
            };
        }
    }
}
