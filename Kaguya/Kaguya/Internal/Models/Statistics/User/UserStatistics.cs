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
        private readonly IServiceScope _scope;

        public UserStatistics(KaguyaUser user, IServiceScope scope) : base(user)
        {
            _user = user;
            _scope = scope;
            
            using (scope)
            {
                var client = scope.ServiceProvider.GetRequiredService<DiscordShardedClient>();
                var fishRepository = scope.ServiceProvider.GetRequiredService<FishRepository>();
                var repRepository = scope.ServiceProvider.GetRequiredService<RepRepository>();

                this.RestUser = client.Rest.GetUserAsync(user.UserId).GetAwaiter().GetResult();
                this.AllFish = fishRepository.GetAllNonTrashAsync(user.UserId).GetAwaiter().GetResult();
                this.GrossCoinsFromFishing = this.AllFish.Sum(x => x.CoinValue);
                this.NetCoinsFishing = this.GrossCoinsFromFishing - this.AllFish.Sum(x => x.CostOfPlay);

                List<(FishRarity, int)> rarityCounts = new();
                foreach (FishRarity rarity in Utilities.GetValues<FishRarity>())
                {
                    rarityCounts.Add((rarity, this.AllFish.Count(x => x.Rarity == rarity)));
                }

                this.RaritiesCount = rarityCounts;
                this.RepGiven = repRepository.GetCountRepGivenAsync(user.UserId).GetAwaiter().GetResult();
                this.RepReceived = repRepository.GetCountRepReceivedAsync(user.UserId).GetAwaiter().GetResult();
            }
        }
        
        public override RestUser RestUser { get; }
        public override IList<Fish> AllFish { get; }
        public override int GrossCoinsFromFishing { get; }
        public override int NetCoinsFishing { get; }
        public override IList<(FishRarity rarity, int count)> RaritiesCount { get; }

        public override async Task<bool> HasRecentlyVotedAsync(TimeSpan threshold)
        {
            using (_scope)
            {
                var upvoteRepository = _scope.ServiceProvider.GetRequiredService<UpvoteRepository>();

                return await upvoteRepository.HasRecentlyUpvotedAsync(_user.UserId, threshold);
            }
        }
        public override int RepGiven { get; }
        public override int RepReceived { get; }
    }
}