using Discord.Commands;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Exp
{
	[Module(CommandModule.Exp)]
	[Group("leaderboard")]
	[Alias("lb")]
	public class Leaderboard : KaguyaBase<Leaderboard>
	{
		private static readonly string[] _lbEmojis =
		{
			"🥇",
			"🥈",
			"🥉"
		};
		private readonly FishRepository _fishRepository;
		private readonly KaguyaStatisticsRepository _kaguyaStatisticsRepository;
		private readonly KaguyaUserRepository _kaguyaUserRepository;
		private readonly ServerExperienceRepository _serverExperienceRepository;

		public Leaderboard(ILogger<Leaderboard> logger, KaguyaUserRepository kaguyaUserRepository,
			FishRepository fishRepository, KaguyaStatisticsRepository kaguyaStatisticsRepository,
			ServerExperienceRepository serverExperienceRepository) : base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
			_fishRepository = fishRepository;
			_kaguyaStatisticsRepository = kaguyaStatisticsRepository;
			_serverExperienceRepository = serverExperienceRepository;
		}

		[Command("-coins")]
		[Summary("Displays the top 10 Kaguya coin holders.")]
		public async Task CoinsLeaderboardCommandAsync()
		{
			var topHolders = await _kaguyaUserRepository.GetTopCoinHoldersAsync(50);
			var stats = await _kaguyaStatisticsRepository.GetMostRecentAsync();

			string lbString = await GetTopTenString(topHolders,
				user => { return Task.FromResult($"{user.Coins.ToShorthandFormat()} coins"); });

			int topTenSum = topHolders.Take(10).Sum(x => x.Coins);
			double percentOwnedByTop = ((double) topTenSum / stats.Coins) * 100;

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
			{
				Description = "🤑 Wealth Leaderboard".AsBold() + "\n\n" + lbString
			}.WithFooter(
				$"{stats.Coins.ToShorthandFormat()} total coins owned | {percentOwnedByTop:N1}% owned by top 10");

			await SendEmbedAsync(embed);
		}

		[Command("-exp")]
		[Summary("Displays the top 10 Kaguya exp holders.")]
		public async Task ExpLeaderboardCommandAsync()
		{
			var topHolders = await _kaguyaUserRepository.GetTopExpHoldersAsync(25);
			var stats = await _kaguyaStatisticsRepository.GetMostRecentAsync();

			string lbString = await GetTopTenString(topHolders,
				user =>
				{
					return Task.FromResult(
						$"Level {user.GlobalExpLevel:N0} - {user.GlobalExp.ToShorthandFormat()} EXP");
				});

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
			{
				Description = $"📢 Top Chatters (Out of {stats.Users.ToShorthandFormat()} Users)".AsBold() +
				              "\n\n" +
				              lbString
			};

			await SendEmbedAsync(embed);
		}

		[Command("-serverexp")]
		[Alias("-sxp")]
		[Summary("Displays the top 10 Kaguya exp holders in the current server.")]
		public async Task ServerExpLeaderboardCommandAsync()
		{
			var topHolders = await _serverExperienceRepository.GetTopAsync(Context.Guild.Id);
			int count = await _serverExperienceRepository.GetAllCountAsync(Context.Guild.Id);

			string lbString = await GetTopTenString(topHolders, exp =>
			{
				int level = (int) ExperienceService.CalculateLevel(exp.Exp);
				return Task.FromResult($"Level {level:N0} - {exp.Exp.ToShorthandFormat()} EXP");
			});

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
			{
				Description = $"📢 Top Chatters (Out of {count.ToShorthandFormat()}) [{Context.Guild.Name}]".AsBold() +
				              "\n\n" +
				              lbString
			};

			await SendEmbedAsync(embed);
		}

		[Command("-fish")]
		[Summary("Displays the top 10 Kaguya fish holders.")]
		public async Task FishLeaderboardCommandAsync()
		{
			var topHolders = await _kaguyaUserRepository.GetTopFishHoldersAsync(25);

			string lbString = await GetTopTenString(topHolders, async user =>
			{
				int fishData = await _fishRepository.CountAllNonTrashAsync(user.UserId);
				return $"Level {user.FishLevel:N0} - {fishData:N0} fish";
			});

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
			{
				Description = "🎣 Fishermen's Ladder".AsBold() + "\n\n" + lbString
			};

			await SendEmbedAsync(embed);
		}

		/// <summary>
		///  Gets the corresponding medal emoji based on the current number
		///  of items in the list.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		private static string GetMedallionString(int i)
		{
			string emote = i switch
			{
				0 => _lbEmojis[i],
				1 => _lbEmojis[i],
				2 => _lbEmojis[i],
				var _ => default
			};

			return emote;
		}

		/// <summary>
		///  Iterates through all users, checks to make sure they exist in Discord, and if so,
		///  adds them to the leaderboard.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="predicate">The data to come after the user portion of the line.</param>
		/// <returns></returns>
		private async Task<string> GetTopTenString<T>(IEnumerable<T> collection, Func<T, Task<string>> predicate)
			where T : IUserSearchable
		{
			var sb = new StringBuilder();
			int countSuccess = 0;
			foreach (var element in collection)
			{
				if (countSuccess == 10)
				{
					break;
				}

				var discordUser = Context.Client.GetUser(element.UserId);
				if (discordUser == null || discordUser.IsBot)
				{
					continue;
				}

				string userName = discordUser.Username;

				string emote = GetMedallionString(countSuccess);
				string rankNum = countSuccess < 3 ? emote : emote + $" {countSuccess + 1}.";
				string curLine = $"{rankNum} {userName} - {await predicate.Invoke(element)}";
				if (countSuccess == 0)
				{
					curLine = curLine.AsBold();
				}

				sb.AppendLine(curLine);

				countSuccess++;
			}

			return sb.ToString();
		}
	}
}