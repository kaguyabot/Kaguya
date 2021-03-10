using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;

namespace Kaguya.Internal.Models.Statistics.Bot
{
	/// <summary>
	///  Serves as a main class for all bot-related statistics, primarily used in the $stats command.
	/// </summary>
	public class BotStatistics : IDisplayableStats, IBotFishingStatistics, IBotDiscordStatistics, IBotCommandStatistics,
		IBotGamblingStatistics
	{
		public BotStatistics(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var client = scope.ServiceProvider.GetRequiredService<DiscordShardedClient>();

				var kaguyaStatisticsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
				var fishRepository = scope.ServiceProvider.GetRequiredService<FishRepository>();
				var commandHistoryRepository = scope.ServiceProvider.GetRequiredService<CommandHistoryRepository>();
				var gambleHistoryRepository = scope.ServiceProvider.GetRequiredService<GambleHistoryRepository>();

				this.DbStats = kaguyaStatisticsRepository.GetMostRecentAsync().GetAwaiter().GetResult();
				this.RestUser = client.Rest.CurrentUser;

				this.CountTrash = fishRepository.CountAllOfRarityAsync(FishRarity.Trash).GetAwaiter().GetResult();
				this.CountCommon = fishRepository.CountAllOfRarityAsync(FishRarity.Common).GetAwaiter().GetResult();
				this.CountUncommon = fishRepository.CountAllOfRarityAsync(FishRarity.Uncommon).GetAwaiter().GetResult();
				this.CountRare = fishRepository.CountAllOfRarityAsync(FishRarity.Rare).GetAwaiter().GetResult();
				this.CountUltraRare =
					fishRepository.CountAllOfRarityAsync(FishRarity.UltraRare).GetAwaiter().GetResult();

				this.CountLegendary =
					fishRepository.CountAllOfRarityAsync(FishRarity.Legendary).GetAwaiter().GetResult();

				this.PlayCount = fishRepository.GetCountAsync().GetAwaiter().GetResult();
				this.GrossCoinsEarned = fishRepository.CountAllCoinsEarnedAsync().GetAwaiter().GetResult();
				this.NetCoinsEarned = fishRepository.CountNetCoinsEarnedAsync().GetAwaiter().GetResult();

				this.SuccessfulCommandCount =
					commandHistoryRepository.GetSuccessfulCountAsync().GetAwaiter().GetResult();

				this.SuccessfulCommandCountLast24Hours = commandHistoryRepository
				                                         .GetRecentSuccessfulCountAsync(TimeSpan.FromHours(24))
				                                         .GetAwaiter()
				                                         .GetResult();

				this.MostPopularCommand =
					commandHistoryRepository.GetMostPopularCommandAsync().GetAwaiter().GetResult();

				this.TotalGambleWins = gambleHistoryRepository.TotalGambleWins().GetAwaiter().GetResult();
				this.TotalGambleLosses = gambleHistoryRepository.TotalGambleLosses().GetAwaiter().GetResult();
				this.TotalGambleWinsCoins = gambleHistoryRepository.TotalGambleWinsCoins().GetAwaiter().GetResult();
				this.TotalGambleLossesCoins = gambleHistoryRepository.TotalGambleLossesCoins().GetAwaiter().GetResult();
				this.TotalGambleWinPercent =
					this.TotalGambleWins / ((double) this.TotalGambleWins + this.TotalGambleLosses);
			}
		}

		/// <summary>
		///  Stats that are pre-loaded from the most recent <see cref="KaguyaStatistics" /> object in the DB.
		/// </summary>
		public KaguyaStatistics DbStats { get; }
		public int SuccessfulCommandCount { get; }
		public int SuccessfulCommandCountLast24Hours { get; }
		public (string cmdName, int count) MostPopularCommand { get; }
		public RestSelfUser RestUser { get; }
		public int CountTrash { get; }
		public int CountCommon { get; }
		public int CountUncommon { get; }
		public int CountRare { get; }
		public int CountUltraRare { get; }
		public int CountLegendary { get; }
		public int PlayCount { get; }
		public long GrossCoinsEarned { get; }
		public long NetCoinsEarned { get; }
		public int TotalGambleWins { get; }
		public int TotalGambleLosses { get; }
		public long TotalGambleWinsCoins { get; }
		public long TotalGambleLossesCoins { get; }
		public double TotalGambleWinPercent { get; }

		public string GetDiscordStatsString()
		{
			var sb = new StringBuilder();

			var stats = (IBotDiscordStatistics) this;
			sb.AppendLine("Username:".AsBold() + $" {stats.RestUser.Mention}");
			sb.AppendLine("ID:".AsBold() + $" {stats.RestUser.Id}");
			sb.AppendLine("Status:".AsBold() + $" {stats.RestUser.Status}");
			sb.AppendLine("Account Created:".AsBold() + $" {stats.RestUser.CreatedAt.Humanize()}");
			sb.AppendLine("Flags:".AsBold() + $" {stats.RestUser.Flags.Humanize(LetterCasing.Sentence)}");

			return sb.ToString();
		}

		public string GetFishingStatsString()
		{
			var sb = new StringBuilder();

			var stats = (IBotFishingStatistics) this;
			sb.AppendLine("Legendary:".AsBold() + $" {stats.CountLegendary:N0}x");
			sb.AppendLine("Ultra Rare:".AsBold() + $" {stats.CountUltraRare:N0}x");
			sb.AppendLine("Rare:".AsBold() + $" {stats.CountRare:N0}x");
			sb.AppendLine("Uncommon:".AsBold() + $" {stats.CountUncommon:N0}x");
			sb.AppendLine("Common:".AsBold() + $" {stats.CountCommon:N0}x");
			sb.AppendLine("Trash:".AsBold() + $" {stats.CountTrash:N0}x");
			sb.AppendLine("Play Count:".AsBold() + $" {stats.PlayCount:N0} plays");
			sb.AppendLine("Coins Earned:".AsBold() + $" {stats.GrossCoinsEarned:N0}");

			return sb.ToString();
		}

		public string GetKaguyaStatsString()
		{
			var sb = new StringBuilder();

			sb.AppendLine("Version:".AsBold() + $" {this.DbStats.Version}");
			sb.AppendLine("Shards:".AsBold() + $" {this.DbStats.Shards}");
			sb.AppendLine("Latency:".AsBold() + $" {this.DbStats.LatencyMilliseconds}ms");
			sb.AppendLine("RAM:".AsBold() + $" {this.DbStats.RamUsageMegabytes:N2}MB");
			sb.AppendLine("Coins:".AsBold() + $" {this.DbStats.Coins.ToShorthandFormat()}");
			sb.AppendLine("Accounts:".AsBold() + $" {this.DbStats.Users.ToShorthandFormat()}");
			sb.AppendLine("Servers:".AsBold() + $" {this.DbStats.ConnectedServers:N0}");
			sb.AppendLine("Retention %:".AsBold() +
			              $" {(this.DbStats.ConnectedServers / (double) this.DbStats.Servers) * 100:N2}");

			return sb.ToString();
		}

		public string GetCommandStatsString()
		{
			var sb = new StringBuilder();

			var stats = (IBotCommandStatistics) this;

			sb.AppendLine("Total Commands:".AsBold() + $" {stats.SuccessfulCommandCount.ToShorthandFormat()}");
			sb.AppendLine("Total Commands (24H):".AsBold() + $" {stats.SuccessfulCommandCountLast24Hours:N0}");
			sb.AppendLine("Most Popular Command:".AsBold() +
			              $" {stats.MostPopularCommand.cmdName} ({stats.MostPopularCommand.count.ToShorthandFormat()})");

			return sb.ToString();
		}

		public string GetGamblingStatsString()
		{
			var sb = new StringBuilder();

			var stats = (IBotGamblingStatistics) this;

			double gambleWinPercent =
				(stats.TotalGambleWins / ((double) stats.TotalGambleWins + stats.TotalGambleLosses)) * 100;

			string winPercentDisp = Double.IsNaN(gambleWinPercent) ? "N/A" : gambleWinPercent.ToString("N2");

			sb.AppendLine("Total Wins:".AsBold() + $" {stats.TotalGambleWins:N0}");
			sb.AppendLine("Total Losses:".AsBold() + $" {stats.TotalGambleLosses:N0}");
			sb.AppendLine("Average Win %:".AsBold() + $" {winPercentDisp}");
			sb.AppendLine("Total Winnings (Coins):".AsBold() +
			              $" {stats.TotalGambleWinsCoins.ToShorthandFormat()} (Net: {(stats.TotalGambleWinsCoins - stats.TotalGambleLossesCoins).ToShorthandFormat()})");

			return sb.ToString();
		}
	}
}