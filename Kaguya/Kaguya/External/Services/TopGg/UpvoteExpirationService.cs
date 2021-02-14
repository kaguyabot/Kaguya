using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.External.Services.TopGg
{
    /// <summary>
    /// Class that handles DMing users who are able to earn new rewards for upvoting.
    /// </summary>
    public class UpvoteExpirationService : BackgroundService, ITimerReceiver
    {
        private readonly ILogger<UpvoteExpirationService> _logger;
        private readonly ITimerService _timerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;

        public UpvoteExpirationService(ILogger<UpvoteExpirationService> logger,
            ITimerService timerService, IServiceProvider serviceProvider, DiscordShardedClient client)
        {
            _logger = logger;
            _timerService = timerService;
            _serviceProvider = serviceProvider;
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
            await _timerService.TriggerAtAsync(DateTime.Now.AddSeconds(15), this);

            if (!_client.AllShardsReady())
            {
                _logger.LogDebug("Shards not ready, aborting");
                return;
            }
            
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var upvoteRepository = scope.ServiceProvider.GetRequiredService<UpvoteRepository>();
                var kaguyaUserRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();
                
                var votes = await upvoteRepository.GetAllUpvotesForNotificationServiceAsync();

                if (!votes.Any())
                {
                    return;
                }

                foreach (Upvote vote in votes)
                {
                    SocketUser socketUser = _client.GetUser(vote.UserId);
                    KaguyaUser user = await kaguyaUserRepository.GetOrCreateAsync(vote.UserId);

                    if (socketUser != null)
                    {
                        var embed = new KaguyaEmbedBuilder(Color.Blue)
                        {
                            Description = $"🛎️ ⬆️ Top.GG Rewards Notification".AsBold() + "\n" +
                                          $"You may now vote for Kaguya on [top.gg]({Global.TopGgUpvoteUrl}) " +
                                          $"for bonus rewards!"
                        };

                        try
                        {
                            var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
                            await dmChannel.SendMessageAsync(embed: embed.Build());
                            
                            _logger.LogDebug($"Notified {user.UserId} about their vote cooldown expiration.");
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning($"Failed to notify user {user.UserId} about their " +
                                               $"vote cooldown expiration. Reason: {e.Message}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"SocketUser object returned null for upvote user ID: {user.UserId}.");
                    }

                    vote.ReminderSent = true;
                    await upvoteRepository.UpdateAsync(vote);
                }
            }
        }
    }
}