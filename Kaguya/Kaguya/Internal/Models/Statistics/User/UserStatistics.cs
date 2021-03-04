using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kaguya.Internal.Models.Statistics.User
{
    public class UserStatistics : UserStatisticsBase
    {
        private readonly KaguyaUser _user;
        private readonly IServiceProvider _serviceProvider;

        public UserStatistics(KaguyaUser user, KaguyaServer server, IServiceProvider serviceProvider) : base(user, server)
        {
            _user = user;
            _serviceProvider = serviceProvider;
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<DiscordShardedClient>();
                var fishRepository = scope.ServiceProvider.GetRequiredService<FishRepository>();
                var repRepository = scope.ServiceProvider.GetRequiredService<RepRepository>();
                var commandHistoryRepository = scope.ServiceProvider.GetRequiredService<CommandHistoryRepository>();
                
                this.RestUser = client.Rest.GetUserAsync(user.UserId).GetAwaiter().GetResult();
                this.AllFish = fishRepository.GetAllNonTrashAsync(user.UserId).GetAwaiter().GetResult();
                this.GrossCoinsFromFishing = this.AllFish.Sum(x => x.CoinValue);
                this.NetCoinsFishing = this.GrossCoinsFromFishing - this.AllFish.Sum(x => x.CostOfPlay);

                List<(FishRarity, int, int)> rarityCounts = new();
                foreach (FishRarity rarity in Utilities.GetValues<FishRarity>())
                {
                    rarityCounts.Add((rarity, this.AllFish.Count(x => x.Rarity == rarity), this.AllFish.Where(x => x.Rarity == rarity).Sum(y => y.CoinValue)));
                }
                
                this.RaritiesCount = rarityCounts;
                this.TotalFishAttempts = this.RaritiesCount.Sum(x => x.count);
                this.RepGiven = repRepository.GetCountRepGivenAsync(user.UserId).GetAwaiter().GetResult();
                this.RepReceived = repRepository.GetCountRepReceivedAsync(user.UserId).GetAwaiter().GetResult();

                this.CommandsExecuted = commandHistoryRepository.GetSuccessfulCountAsync(user.UserId).GetAwaiter().GetResult();
                this.CommandsExecutedLastTwentyFourHours = commandHistoryRepository.GetRecentSuccessfulCountAsync(user.UserId, TimeSpan.FromHours(24)).GetAwaiter().GetResult();
                this.MostUsedCommand = commandHistoryRepository.GetFavoriteCommandAsync(user.UserId).GetAwaiter().GetResult();
            }
        }
        
        public override RestUser RestUser { get; }
        public sealed override IList<Fish> AllFish { get; }
        public sealed override int GrossCoinsFromFishing { get; }
        public override int NetCoinsFishing { get; }
        public sealed override IList<(FishRarity rarity, int count, int coinsSum)> RaritiesCount { get; }
        public override int TotalFishAttempts { get; }

        public override async Task<bool> HasRecentlyVotedAsync(TimeSpan threshold)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var upvoteRepository = scope.ServiceProvider.GetRequiredService<UpvoteRepository>();

                return await upvoteRepository.HasRecentlyUpvotedAsync(_user.UserId, threshold);
            }
        }
        public override int RepGiven { get; }
        public override int RepReceived { get; }
        public override int CommandsExecuted { get; }
        public override int CommandsExecutedLastTwentyFourHours { get; }
        public override string MostUsedCommand { get; }
    }
}