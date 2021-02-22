using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Web.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly DiscordShardedClient _client;
        
        private AuthDiscordBotListApi _discordBotListApi;

        public TopGgStatsUpdaterService(ILogger<TopGgStatsUpdaterService> logger,
            IServiceProvider serviceProvider, ITimerService timerService, DiscordShardedClient client)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timerService = timerService;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            
            await _timerService.TriggerAtAsync(DateTimeOffset.Now, this);
        }

        public async Task HandleTimer(object payload)
        {
            if (!_client.AllShardsReady())
            {
                _logger.LogInformation("All shards not ready. Aborting. Retrying in 1 minute");
                await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(1), this);

                return;
            }
            
            await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(15), this);

            _discordBotListApi ??= GetConfguredApi();

            if (_discordBotListApi == null)
            {
                _logger.LogWarning("Could not create a successfull connection to top.gg. Statistics will not be posted. " +
                                   "This warning can be safely ignored by developer contributors");

                return;
            }
            
            var dblBot = await _discordBotListApi.GetMeAsync();
            
            if (dblBot == null)
            {
                _logger.LogWarning("Could not find current bot on top.gg. Statistics will not be posted. " +
                                   "This warning can be safely ignored by developer contributors");

                return;
            }
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var statsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
                var curStats = await statsRepository.GetMostRecentAsync();

                try
                {
                    int guildCount = curStats.ConnectedServers;
                    int shardCount = curStats.Shards;
                    
                    await dblBot.UpdateStatsAsync(guildCount, shardCount);
                    
                    _logger.LogInformation($"top.gg stats updated: {guildCount} servers | {shardCount} shards");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Failed to post stats to top.gg");
                }
            }
        }

        private AuthDiscordBotListApi GetConfguredApi()
        {
            string apiKey = _serviceProvider.GetRequiredService<IOptions<TopGgConfigurations>>().Value.ApiKey;
            return new AuthDiscordBotListApi(_client.CurrentUser.Id, apiKey);
        }
    }
}