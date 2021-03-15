using Discord.Rest;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Internal.Models.Statistics.User
{
	public abstract class UserStatisticsBase : IUserStatistics
	{
		private readonly KaguyaServer _server;
		private readonly KaguyaUser _user;

		protected UserStatisticsBase(KaguyaUser user, KaguyaServer server)
		{
			_user = user;
			_server = server;
		}

		public abstract RestUser RestUser { get; }
		public abstract IList<Fish> AllFish { get; }
		public abstract int GrossCoinsFromFishing { get; }
		public abstract int NetCoinsFishing { get; }
		public abstract IList<(FishRarity rarity, int count, int coinsSum)> RaritiesCount { get; }
		public abstract int TotalFishAttempts { get; }
		public abstract Task<bool> HasRecentlyVotedAsync(TimeSpan threshold);
		public abstract int RepGiven { get; }
		public abstract int RepReceived { get; }
		public abstract int CommandsExecuted { get; }
		public abstract int CommandsExecutedLastTwentyFourHours { get; }
		public abstract string MostUsedCommand { get; }
		public int FishExp => _user.FishExp;
		public int TotalGambles => _user.TotalGambles;
		public int TotalCoinsEarnedGambling => _user.TotalGambleWins;
		public int TotalCoinsLostGambling => _user.TotalGambleLosses;
		public int TotalCoinsGambled => _user.TotalCoinsGambled;
		public int TotalGambleWins => _user.TotalGambleWins;
		public int TotalGambleLosses => _user.TotalGambleLosses;
		public int NetCoinsGambling => _user.NetGambleCoingEarnings;
		public double PercentWinGambling => _user.TotalGambleWins / (double) _user.TotalGambleLosses;
		public bool EligibleToVote => _user.CanUpvote;
		public int TotalVotesTopGg => _user.TotalUpvotesTopGg;
		public int TotalVotesDiscordBoats => _user.TotalUpvotesDiscordBoats;
		public int Coins => _user.Coins;
		public int DaysPremium => _user.TotalDaysPremium;

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

		public string GetFishingStatsString()
		{
			var sb = new StringBuilder();

			foreach (var rarity in this.RaritiesCount)
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

			sb.AppendLine("Global Level:".AsBold() + $" {_user.GlobalExpLevel:N0}");
			sb.AppendLine("Global Exp:".AsBold() + $" {_user.GlobalExp:N0}");
			sb.AppendLine("Coins:".AsBold() + $" {this.Coins:N0}");

			if (this.DaysPremium > 0)
			{
				sb.AppendLine("Premium Subscription:".AsBold() + $" {this.DaysPremium:N0} days");
			}

			sb.AppendLine("Rep Given:".AsBold() + $" {this.RepGiven}");
			sb.AppendLine("Rep Received:".AsBold() + $" {this.RepReceived}");
			sb.AppendLine("Total Upvotes (top.gg):".AsBold() + $" {this.TotalVotesTopGg}");
			sb.AppendLine("Eligible to Vote?".AsBold() + $" {(this.EligibleToVote ? "Yes".AsBlueCode() : "No")}");

			sb.AppendLine("Is Premium?".AsBold() + $" {(_user.IsPremium ? "Yes" : "No")}");

			return sb.ToString();
		}

		public string GetGamblingStatsString()
		{
			IUserGambleStatistics gambleStats = this;

			var sb = new StringBuilder();

			sb.AppendLine("Total Gambles:".AsBold() +
			              $" {gambleStats.TotalGambles} ({gambleStats.TotalGambleWins} wins / {gambleStats.TotalGambleLosses} losses)");

			sb.AppendLine("Win %:".AsBold() +
			              $" {(double.IsNaN(gambleStats.PercentWinGambling) ? "N/A" : (gambleStats.PercentWinGambling * 100).ToString("N0"))}");

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