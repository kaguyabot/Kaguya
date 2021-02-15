using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Options;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Options;
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

            _discordBotListApi ??= GetConfguredApi();
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var statsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
                var curStats = await statsRepository.GetMostRecentAsync();

                try
                {
                    var dblBot = await _discordBotListApi.GetMeAsync();
                    
                    var guildCount = _client.Guilds.Count;
                    var shardCount = _client.Shards.Count;
                    var shards = _client.Shards.Select(x => x.ShardId).ToArray();
                    
                    await dblBot.UpdateStatsAsync(guildCount, shardCount, shards);
                    
                    _logger.LogInformation($"top.gg stats updated: {_client.Guilds.Count} servers | {curStats.Shards} shards");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Failed to post stats to top.gg");
                }
            }
        }

        private AuthDiscordBotListApi GetConfguredApi()
        {
            string token = _serviceProvider.GetRequiredService<IOptions<DiscordConfigurations>>().Value.BotToken;
            return new AuthDiscordBotListApi(_client.CurrentUser.Id, token);
        }
    }
}