using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
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
                var allVotes = await DatabaseQueries.GetAllAsync<TopGgWebhook>(x => x.TimeVoted > 0 && !x.ReminderSent
                    && x.UpvoteType.ToLower() != "test");

                foreach (var vote in allVotes)
                {
                    var votes = allVotes.Where(x => x.UserId == vote.UserId).OrderByDescending(x => x.TimeVoted).ToList();
                    var mostRecentVote = !votes.Any() ? null : votes[0];

                    if (mostRecentVote == null)
                    {
                        await ConsoleLogger.LogAsync($"User {vote.UserId} may now vote again, but " +
                                                     $"their most recent vote was null in the database. " +
                                                     $"No DM has been sent, and nothing has been " +
                                                     $"updated in the database.", LogLvl.WARN);
                        continue;
                    }

                    if (mostRecentVote.TimeVoted > DateTime.Now.AddHours(-12).ToOADate())
                        continue;

                    var socketUser = ConfigProperties.Client.GetUser(vote.UserId);
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
                        await ConsoleLogger.LogAsync($"User {vote.UserId} can now upvote Kaguya. DM NOT sent.", LogLvl.DEBUG);
                        continue;
                    }

                    await ConsoleLogger.LogAsync($"User {vote.UserId} has been reminded to upvote Kaguya.", LogLvl.DEBUG);
                }
            };
        }
    }
}
