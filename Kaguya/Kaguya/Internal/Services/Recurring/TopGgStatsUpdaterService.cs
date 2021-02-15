using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Services.Recurring
{
    /// <summary>
    /// Automatically posts stats to Top.GG
    /// </summary>
    public class TopGgStatsUpdaterService : BackgroundService, ITimerReceiver
    {
        private readonly ILogger<TopGgStatsUpdaterService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITimerService _timerService;
        private readonly AuthDiscordBotListApi _discordBotListApi;
        private readonly DiscordShardedClient _client;

        public TopGgStatsUpdaterService(ILogger<TopGgStatsUpdaterService> logger,
            IServiceProvider serviceProvider, ITimerService timerService,
            AuthDiscordBotListApi discordBotListApi, DiscordShardedClient client)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timerService = timerService;
            _discordBotListApi = discordBotListApi;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            
            await _timerService.TriggerAtAsync(DateTime.Now, this);
        }

        public async Task HandleTimer(object payload)
        {
            if (!_client.AllShardsReady())
            {
                _logger.LogDebug("All shards not ready. Aborting. Retrying in 1 minute.");
                await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(1), this);

                return;
            }
            
            await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(15), this);
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var statsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
                var curStats = await statsRepository.GetMostRecentAsync();

                try
                {
                    await _discordBotListApi.UpdateStats(_client.Guilds.Count, curStats.Shards);
                    _logger.LogInformation($"top.gg stats updated: {_client.Guilds.Count} servers | {curStats.Shards} shards");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Failed to post stats to top.gg");
                }
            }
        }
    }
}