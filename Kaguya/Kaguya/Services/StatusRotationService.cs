﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.Services
{
    public class StatusRotationService : BackgroundService, ITimerReceiver
    {
        private readonly ILogger<StatusRotationService> _logger;
        private readonly ITimerService _timerService;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _serviceProvider;

        private static readonly Action<ILogger, string, Exception> StatusSwapLog =
	        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(), "Changed status to {Status}");

        private int _rotationIndex;
        
        public StatusRotationService(ILogger<StatusRotationService> logger, ITimerService timerService, DiscordShardedClient client, IServiceProvider serviceProvider)
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
                var statusInfo = await GetStatusAsync();
                await _client.SetGameAsync(statusInfo.statusText, null, statusInfo.activityType);
            }
            catch (Exception e)
            {
                _rotationIndex = 0;
                _logger.LogError(e, "Exception encountered within the status rotation service.");
            }

            // Puts ourself back in-queue...
            await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(15), this);
        }

        private async Task<(string statusText, ActivityType activityType)> GetStatusAsync()
        {
            string text;
            switch (_rotationIndex)
            {
                default:
                    _rotationIndex = 1;
                    
                    text = Global.Version;
                    
                    StatusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Playing);
                case 1:
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        _rotationIndex++;
                        var kaguyaUserRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();

                        text = $"{await kaguyaUserRepository.GetCountOfUsersAsync():N0} users";
                        
                        StatusSwapLog(_logger, text, default!);
                        return (text, ActivityType.Watching);
                    }
                case 2:
                    _rotationIndex++;

                    text = $"{_client.Guilds.Count:N0} servers";
                    
                    StatusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
                case 3:
                    _rotationIndex++;

                    text = "$help | @Kaguya help";
                    
                    StatusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Listening);
                case 4:
                    _rotationIndex++;

                    text = "$vote for bonuses!";
                    
                    StatusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
                case 5:
                    _rotationIndex++;

                    text = "$premium for rewards!";
                    
                    StatusSwapLog(_logger, text, default!);
                    return (text, ActivityType.Watching);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _timerService.TriggerAtAsync(DateTime.Now, this);
        }
    }
}