using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaApi.Database.Models;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG
{
    public class UpvoteNotifier
    {
        private static readonly BlockingCollection<DatabaseUpvoteWebhook> _voteQueue = new BlockingCollection<DatabaseUpvoteWebhook>();
        private Task _runner;

        public UpvoteNotifier()
        { 
            _runner = Task.Run(async () => await Run());
        }

        private async Task Run()
        {
            foreach (var vote in _voteQueue.GetConsumingEnumerable())
            {
                try
                {
                    var user = await DatabaseQueries.GetOrCreateUserAsync(vote.UserId);
                    var socketUser = ConfigProperties.Client.GetUser(user.UserId);

                    var r = new Random();
                    var points = 750;
                    var exp = 500;

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
                        var dmCh = await socketUser.GetOrCreateDMChannelAsync();

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

                        await dmCh.SendEmbedAsync(embed);
                    }

                    await DatabaseQueries.UpdateAsync(user);
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

        public void Enqueue(DatabaseUpvoteWebhook item)
        {
            _voteQueue.Add(item);
        }
    }
}
