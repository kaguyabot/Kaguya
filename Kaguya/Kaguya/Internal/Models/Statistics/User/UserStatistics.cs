using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kaguya.Internal.Models.Statistics.User
{
    public class UserStatistics : UserStatisticsBase
    {
        private readonly KaguyaUser _user;
        private readonly KaguyaServer _server;
        private readonly IServiceProvider _serviceProvider;

        public UserStatistics(KaguyaUser user, KaguyaServer server, IServiceProvider serviceProvider) : base(user)
        {
            _user = user;
            _server = server;
            _serviceProvider = serviceProvider;
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<DiscordShardedClient>();
                var fishRepository = scope.ServiceProvider.GetRequiredService<FishRepository>();
                var repRepository = scope.ServiceProvider.GetRequiredService<RepRepository>();
                var commandHistoryRepository = scope.ServiceProvider.GetRequiredService<CommandHistoryRepository>();
                var serverExpRepository = scope.ServiceProvider.GetRequiredService<ServerExperienceRepository>();

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

        public string GetDiscordStatsString()
        {
            var sb = new StringBuilder();
            var r = this.RestUser; // Shorthand
            
            sb.AppendLine("Username:".AsBold() + $" {r} | ID: {r.Id}");
            sb.AppendLine("Status:".AsBold() + $" {r.Status.Humanize()}");
            sb.AppendLine("Account Created:".AsBold() + $" {r.CreatedAt.Humanize()}");
            sb.AppendLine("Flags:".AsBold() + $" {r.PublicFlags.Humanize(LetterCasing.Sentence)}");

            return sb.ToString();
        }

        public string GetFishStatsString()
        {
            var sb = new StringBuilder();

            foreach (var rarity in RaritiesCount)
            {
                sb.AppendLine($"{rarity.rarity.Humanize().AsBold()}: {rarity.count:N0}x (+{rarity.coinsSum:N0} coins)");
            }

            sb.AppendLine("Plays:".AsBold() + " " + this.TotalFishAttempts.ToString("N0"));
            sb.AppendLine("Fish Exp:".AsBold() + " " + this.FishExp.ToString("N0"));

            char sign = this.NetCoinsFishing >= 0 ? '+' : '-';
            sb.AppendLine("Coins Earned:".AsBold() + $" {this.GrossCoinsFromFishing:N0} (Net: {sign}{this.NetCoinsFishing:N0})");
            return sb.ToString();
        }

        public string GetKaguyaStatsString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Global Level:".AsBold() + $" {this._user.GlobalExpLevel:N0}");
            sb.AppendLine("Global Exp:".AsBold() + $" {this._user.GlobalExp:N0}");
            sb.AppendLine("Coins:".AsBold() + $" {this.Coins:N0}");

            if (this.DaysPremium > 0)
            {
                sb.AppendLine("Premium Subscription:".AsBold() + $" {this.DaysPremium:N0} days");
            }
            
            sb.AppendLine("Rep Given:".AsBold() + $" {this.RepGiven}");
            sb.AppendLine("Rep Received:".AsBold() + $" {this.RepReceived}");
            sb.AppendLine("Total Upvotes (top.gg):".AsBold() + $" {this.TotalVotesTopGg}");
            sb.AppendLine("Eligible to Vote?".AsBold() + $" {(this.EligibleToVote ? "Yes".AsBlueCode(Global.TopGgUpvoteUrl) : "No")}");
            sb.AppendLine("Is Premium?".AsBold() + $" {(this._user.IsPremium ? "Yes" : "No")}");
            
            return sb.ToString();
        }

        public string GetGamblingStatsString()
        {
            IUserGambleStatistics gambleStats = this;
            
            var sb = new StringBuilder();

            sb.AppendLine("Total Gambles:".AsBold() + $" {gambleStats.TotalGambles} ({gambleStats.TotalGambleWins} wins / {gambleStats.TotalGambleLosses} losses)");
            sb.AppendLine("Win %:".AsBold() + $" {(double.IsNaN(gambleStats.PercentWinGambling) ? "N/A" : (gambleStats.PercentWinGambling * 100).ToString("N0"))}");
            sb.AppendLine("Winnings:".AsBold() + $" {gambleStats.TotalCoinsGambled:N0} (Net: {gambleStats.NetCoinsGambling})");
            
            return sb.ToString();
        }

        public string GetCommandStatsString()
        {
            IUserCommandStatistics commandStats = this;

            var sb = new StringBuilder();
            
            sb.AppendLine("Favorite Command:".AsBold() + $" {_server.CommandPrefix}{commandStats.MostUsedCommand}");
            sb.AppendLine("Commands Executed:".AsBold() + $" {commandStats.CommandsExecuted:N0}");
            sb.AppendLine("Commands Executed (Last 24H):".AsBold() + $" {commandStats.CommandsExecutedLastTwentyFourHours:N0}");
            
            return sb.ToString();
        }
    }
}