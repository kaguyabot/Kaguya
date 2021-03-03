using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Models.Statistics.User;
using Kaguya.Options;
using Microsoft.Extensions.Options;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("stats")]
    public class Stats : KaguyaBase<Stats>
    {
        private readonly ILogger<Stats> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly KaguyaStatisticsRepository _statisticsRepository;
        private readonly FishRepository _fishRepository;
        private readonly IOptions<AdminConfigurations> _adminConfig;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        public Stats(ILogger<Stats> logger, IServiceProvider serviceProvider, KaguyaStatisticsRepository statisticsRepository,
            FishRepository fishRepository, IOptions<AdminConfigurations> adminConfig,
            KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _statisticsRepository = statisticsRepository;
            _fishRepository = fishRepository;
            _adminConfig = adminConfig;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        // [Command]
        // [Summary("Displays various bot statistics.")]
        // public async Task StatisticsCommandAsync()
        // {
        //     KaguyaStatistics stats = await _statisticsRepository.GetMostRecentAsync();
        //     
        // }

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
            var userStats = new UserStatistics(user, _serviceProvider);

            var commandStats = userStats as IUserCommandStatistics;
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.LightOrange)
            {
                Description = $"Stats for {userStats.RestUser.Mention}".AsBoldUnderlined(),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "📈 Discord Stats",
                        Value = userStats.GetDiscordStatsString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "🐠 Fishing Stats",
                        Value = userStats.GetFishStatsString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "🔍 Kaguya Stats",
                        Value = userStats.GetKaguyaStatsString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Gambling Stats",
                        Value = userStats.GetGamblingStatsString(),
                        IsInline = true
                    }
                }
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

        // todo: $stats -u to fetch other user's stats OR stats of user
        // todo: $stats -fish to view personal fish stats OR of other users.
    }
}