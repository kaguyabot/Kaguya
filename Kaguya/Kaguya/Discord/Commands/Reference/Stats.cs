using Discord;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Models.Statistics;
using Kaguya.Internal.Models.Statistics.Bot;
using Kaguya.Internal.Models.Statistics.User;
using Kaguya.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("stats")]
	public class Stats : KaguyaBase<Stats>
	{
		private readonly IOptions<AdminConfigurations> _adminConfig;
		private readonly KaguyaServerRepository _kaguyaServerRepository;
		private readonly KaguyaUserRepository _kaguyaUserRepository;
		private readonly IServiceProvider _serviceProvider;

		public Stats(ILogger<Stats> logger, IServiceProvider serviceProvider, IOptions<AdminConfigurations> adminConfig,
			KaguyaUserRepository kaguyaUserRepository, KaguyaServerRepository kaguyaServerRepository) : base(logger)
		{
			_serviceProvider = serviceProvider;
			_adminConfig = adminConfig;
			_kaguyaUserRepository = kaguyaUserRepository;
			_kaguyaServerRepository = kaguyaServerRepository;
		}

		[Command]
		[Summary("Displays various bot statistics.")]
		public async Task StatisticsCommandAsync()
		{
			var stats = new BotStatistics(_serviceProvider);

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Tan)
			{
				Fields = GetStatsFields(stats)
			};

			await SendEmbedAsync(embed);
		}

		[Priority(0)]
		[Command("-u")]
		[Summary("Displays various user statistics.")]
		public async Task UserStatisticsCommandAsync(params string[] args)
		{
			ulong? id = null;
			if (args.Any())
			{
				if (ulong.TryParse(args[0], out ulong userId))
				{
					if (Context.User.Id != _adminConfig.Value.OwnerId && userId != Context.User.Id)
					{
						await SendBasicErrorEmbedAsync("Sorry, only Kaguya Administration can view " +
						                               "statistics on other users.");

						return;
					}

					id = userId;
				}
			}

			id ??= Context.User.Id;

			var user = await _kaguyaUserRepository.GetOrCreateAsync(id.Value);
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var userStats = new UserStatistics(user, server, _serviceProvider);

			var embed = new KaguyaEmbedBuilder(KaguyaColors.LightOrange)
			{
				Description = $"Stats for {userStats.RestUser.Mention}".AsBoldUnderlined(),
				Fields = GetStatsFields(userStats)
			};

			await SendEmbedAsync(embed);
		}

		[Priority(1)]
		[Command("-u")]
		public async Task UserStatisticsCommandAsync(IUser user = null)
		{
			ulong id = user?.Id ?? Context.User.Id;
			await UserStatisticsCommandAsync(id.ToString());
		}

		private List<EmbedFieldBuilder> GetStatsFields(IDisplayableStats displayableStats)
		{
			return new()
			{
				new EmbedFieldBuilder
				{
					Name = "📈 Discord Stats",
					Value = displayableStats.GetDiscordStatsString(),
					IsInline = true
				},
				new EmbedFieldBuilder
				{
					Name = "🐠 Fishing Stats",
					Value = displayableStats.GetFishingStatsString(),
					IsInline = true
				},
				new EmbedFieldBuilder
				{
					Name = "🔍 Kaguya Stats",
					Value = displayableStats.GetKaguyaStatsString(),
					IsInline = true
				},
				new EmbedFieldBuilder
				{
					Name = "🎲 Gambling Stats",
					Value = displayableStats.GetGamblingStatsString(),
					IsInline = true
				},
				new EmbedFieldBuilder
				{
					Name = "📢 Command Stats",
					Value = displayableStats.GetCommandStatsString(),
					IsInline = true
				}
			};
		}
	}
}