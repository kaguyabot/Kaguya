using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Services.Recurring
{
    public class KaguyaPremiumRoleService : BackgroundService, ITimerReceiver
    {
        private readonly ILogger<KaguyaPremiumRoleService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITimerService _timerService;
        private readonly DiscordShardedClient _client;

        public KaguyaPremiumRoleService(ILogger<KaguyaPremiumRoleService> logger, IServiceProvider serviceProvider,
            ITimerService timerService, DiscordShardedClient client)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
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
            await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(5), this);

            using (var scope = _serviceProvider.CreateScope())
            {
                var premiumKeyService = scope.ServiceProvider.GetRequiredService<PremiumKeyRepository>();
                var activeKeyHolders = await premiumKeyService.GetAllActiveKeyholdersAsync();
                
                foreach (ulong keyHolderId in activeKeyHolders)
                {
                    // Hardcoded intentionally - this service really should not be used 
                    // by other contributors.
                    var socketGuild = _client.GetGuild(546880579057221644);

                    if (socketGuild == null)
                    {
                        _logger.LogError("Socket guild was null!!");
                        return;
                    }

                    var user = socketGuild.GetUser(keyHolderId);

                    if (user == null)
                    {
                        _logger.LogDebug($"User {keyHolderId} was not found.");
                        continue;
                    }

                    var role = socketGuild.Roles.FirstOrDefault(x => x.Name.Contains("premium", StringComparison.OrdinalIgnoreCase));

                    if (role == null)
                    {
                        _logger.LogWarning("Premium role not found");
                        return;
                    }

                    if (user.Roles.Contains(role))
                    {
                        _logger.LogTrace($"User {keyHolderId} already has premium role. Skipping...");
                        continue;
                    }
                
                    try
                    {
                        await user.AddRoleAsync(role);
                        _logger.LogInformation($"Successfully added role {role.Name} to user {keyHolderId}.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Failed to add role {role.Name} to user {keyHolderId}.");
                    }
                }
            }
        }
    }
}