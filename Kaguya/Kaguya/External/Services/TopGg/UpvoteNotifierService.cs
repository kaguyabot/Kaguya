using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Microsoft.Extensions.Logging;

namespace Kaguya.External.Services.TopGg
{
    public class UpvoteNotifierService
    {
        private readonly ILogger<UpvoteNotifierService> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly UpvoteRepository _upvoteRepository;
        private readonly DiscordShardedClient _client;
        private static readonly BlockingCollection<Upvote> _voteQueue = new BlockingCollection<Upvote>();
        private Task _runner;

        public const int COINS = 750;
        public const int EXP = 500;

        public UpvoteNotifierService(ILogger<UpvoteNotifierService> logger,
            KaguyaUserRepository kaguyaUserRepository, UpvoteRepository upvoteRepository, 
            DiscordShardedClient client)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _upvoteRepository = upvoteRepository;
            _client = client;
            _runner = Task.Run(async () => await Run());
        }

        private async Task Run()
        {
            foreach (Upvote vote in _voteQueue.GetConsumingEnumerable())
            {
                try
                {
                    var user = await _kaguyaUserRepository.GetOrCreateAsync(vote.UserId);
                    var socketUser = _client.GetUser(user.UserId);

                    int coins = COINS;
                    int exp = EXP;

                    if (vote.IsWeekend)
                    {
                        coins *= 2;
                        exp *= 2;
                    }

                    if (user.IsPremium)
                    {
                        coins *= 2;
                        exp *= 2;
                    }

                    user.AdjustCoins(coins);
                    user.AdjustExperienceGlobal(exp);
                    user.LastUpvoted = DateTime.Now;
                    user.TotalUpvotes++;

                    if (socketUser != null)
                    {
                        IDMChannel dmCh = await socketUser.GetOrCreateDMChannelAsync();

                        string weekendStr = $"Because you voted on the weekend, you have been given " +
                                            $"double coins and double exp!";

                        var embed = new KaguyaEmbedBuilder(KaguyaColors.IceBlue)
                        {
                            Title = "Kaguya Upvote Rewards",
                            Description =
                                $"Thanks for upvoting me on [top.gg]({Global.TopGgUpvoteUrl})! " +
                                $"You have been awarded `{coins:N0} coins` and `{exp:N0} exp`. " +
                                $"{(vote.IsWeekend ? weekendStr : "")}"
                        };

                        try
                        {
                            await dmCh.SendMessageAsync(embed: embed.Build());
                        }
                        catch (Exception e)
                        {
                            _logger.LogDebug(e, $"Failed to DM user {user.UserId} with their " +
                                                      $"Top.GG authorized vote notification.");
                        }
                    }

                    try
                    {
                        await _kaguyaUserRepository.UpdateAsync(user);
                        await _upvoteRepository.InsertAsync(vote);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to insert authorized Top.GG webhook into database " +
                                                        $"for user {user.UserId}.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "An exception occurred inside of the top.gg notifier for-each loop.");
                }
            }
        }

        public void Enqueue(Upvote item) => _voteQueue.Add(item);
    }
}