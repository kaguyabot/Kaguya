using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.Services
{
    public class StatusRotationService : BackgroundService, ITimerReceiver
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<StatusRotationService> _logger;
        private readonly ITimerService _timerService;
        private readonly IServiceProvider _serviceProvider;

        private static readonly Action<ILogger, string, Exception> _statusSwapLog =
	        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(), "Changed status to {Status}");

        private int _rotationIndex;
        private bool _triggeredOnce;
        
        public StatusRotationService(ILogger<StatusRotationService> logger, ITimerService timerService, DiscordShardedClient client, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _timerService = timerService;
            _client = client;
            _serviceProvider = serviceProvider;
        }
        
        public async Task HandleTimer(object payload)
        {
            try
            {
                _logger.LogDebug("Status rotation timer triggered.");

                if (Global.ShardsReady.Count == _client.Shards.Count)
                {
                    var statusInfo = await GetStatusAsync();
                    await _client.SetGameAsync(statusInfo.statusText, null, statusInfo.activityType);

                    if (!_triggeredOnce)
                    {
                        _triggeredOnce = true;
                    }
                    
                    _logger.LogDebug($"Set status to {statusInfo.activityType} {statusInfo.statusText}");
                }
            }
            catch (Exception e)
            {
                _rotationIndex = 0;
                _logger.LogError(e, "Exception encountered within the status rotation service.");
            }

            // Puts ourself back in the queue...
            if (_triggeredOnce)
            {
                await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(15), this);
            }
            else
            {
                await _timerService.TriggerAtAsync(DateTime.Now.AddSeconds(10), this);   
            }
        }

        private async Task<(string statusText, ActivityType activityType)> GetStatusAsync()
        {
            string text;
            switch (_rotationIndex)
            {
                default:
                    _rotationIndex = 1;
                    
                    text = Global.Version;
                    
                    _statusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Playing);
                case 1:
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        _rotationIndex++;
                        var kaguyaUserRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();

                        text = $"{await kaguyaUserRepository.GetCountOfUsersAsync():N0} users";
                        
                        _statusSwapLog(_logger, text, default!);
                        return (text, ActivityType.Watching);
                    }
                case 2:
                    _rotationIndex++;

                    text = $"{_client.Guilds.Count:N0} servers";
                    
                    _statusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
                case 3:
                    _rotationIndex++;

                    text = "$help | @Kaguya help";
                    
                    _statusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Listening);
                case 4:
                    _rotationIndex++;

                    text = "$vote for bonuses!";
                    
                    _statusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
                case 5:
                    _rotationIndex++;

                    text = "$premium for rewards!";
                    
                    _statusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // It takes about 2 minutes for all shards to log in.
            await _timerService.TriggerAtAsync(DateTime.Now, this);
        }
    }
}