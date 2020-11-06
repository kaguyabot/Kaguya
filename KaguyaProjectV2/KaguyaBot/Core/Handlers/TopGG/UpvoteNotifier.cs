using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaApi.Database.Models;
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
    public class UpvoteNotifier
    {
        private static readonly BlockingCollection<DatabaseUpvoteWebhook> _voteQueue = new BlockingCollection<DatabaseUpvoteWebhook>();
        private Task _runner;
        public UpvoteNotifier() { _runner = Task.Run(async () => await Run()); }

        private async Task Run()
        {
            foreach (DatabaseUpvoteWebhook vote in _voteQueue.GetConsumingEnumerable())
            {
                try
                {
                    User user = await DatabaseQueries.GetOrCreateUserAsync(vote.UserId);
                    SocketUser socketUser = ConfigProperties.Client.GetUser(user.UserId);

                    int points = 750;
                    int exp = 500;

                    if (vote.IsWeekend)
                    {
                        points *= 2;
                        exp *= 2;
                    }

                    if (user.IsPremium)
                    {
                        points *= 2;
                        exp *= 2;
                    }

                    user.Points += points;
                    user.Experience += exp;
                    user.TotalUpvotes++;

                    if (socketUser != null)
                    {
                        IDMChannel dmCh = await socketUser.GetOrCreateDMChannelAsync();

                        string weekendStr = $"Because you voted on the weekend, you have been given " +
                                            $"double points and double exp!";

                        var embed = new KaguyaEmbedBuilder(EmbedColor.PINK)
                        {
                            Title = "Kaguya Upvote Rewards",
                            Description =
                                $"Thanks for upvoting me on [top.gg](https://top.gg/bot/538910393918160916/vote)! " +
                                $"You have been awarded `{points:N0} points` and `{exp:N0} exp`. " +
                                $"{(vote.IsWeekend ? weekendStr : "")}"
                        };

                        try
                        {
                            await dmCh.SendEmbedAsync(embed);
                            
                        }
                        catch (Exception e)
                        {
                            await ConsoleLogger.LogAsync(e, $"Failed to DM user {user.UserId} with their " +
                                                            $"Top.GG authorized vote notification.", LogLvl.WARN);
                        }
                    }

                    try
                    {
                        await DatabaseQueries.UpdateAsync(user);
                    }
                    catch (Exception e)
                    {
                        await ConsoleLogger.LogAsync(e, "Failed to insert authorized Top.GG webhook into database " +
                                                        $"for user {user.UserId}.");
                    }
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"An exception occurred inside of the top.gg notifier for-each loop in " +
                                                 $"UpvoteNotifier.cs\n\n" +
                                                 $"Exception Message: {e.Message}\n" +
                                                 $"Stack Trace: {e.StackTrace}", LogLvl.ERROR);
                }
            }
        }

        public void Enqueue(DatabaseUpvoteWebhook item) => _voteQueue.Add(item);
    }
}