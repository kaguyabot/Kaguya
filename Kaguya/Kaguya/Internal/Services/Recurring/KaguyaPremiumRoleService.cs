using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Extensions.DiscordExtensions;
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

        private const ulong SUPPORT_GUILD_ID = 546880579057221644;
        
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
            
            await _timerService.TriggerAtAsync(DateTimeOffset.Now, this);
        }

        public async Task HandleTimer(object payload)
        {
            await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(1), this);

            await CheckNew();
            await CheckExpired();
        }

        private async Task CheckNew()
        {
            if (!_client.AllShardsReady())
            {
                return;
            }
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();
                var activeIds = await userRepository.GetAllActivePremiumAsync();

                foreach (ulong id in activeIds)
                {
                    // Hardcoded intentionally - this service really should not be used 
                    // by other contributors.
                    var socketGuild = _client.GetGuild(SUPPORT_GUILD_ID);

#if !DEBUG
                    if (socketGuild == null)
                    {
                        _logger.LogError("Socket guild was null!!");

                        return;
                    }
#endif
                    var user = socketGuild.GetUser(id);

                    if (user == null)
                    {
                        _logger.LogDebug($"User {id} was not found.");

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
                        _logger.LogTrace($"User {id} already has premium role. Skipping...");

                        continue;
                    }

                    try
                    {
                        await user.AddRoleAsync(role);
                        _logger.LogInformation($"Successfully added role {role.Name} to user {id}.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Failed to add role {role.Name} to user {id}.");
                    }
                }
            }
        }

        private async Task CheckExpired()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();
                var expiredIds = await userRepository.GetAllExpiredPremiumAsync(30);

                foreach (ulong id in expiredIds)
                {
                    // Hardcoded intentionally - this service really should not be used 
                    // by other contributors.
                    var socketGuild = _client.GetGuild(SUPPORT_GUILD_ID);

                    if (socketGuild == null)
                    {
                        _logger.LogError("Socket guild was null!!");

                        return;
                    }

                    var user = socketGuild.GetUser(id);

                    if (user == null)
                    {
                        _logger.LogDebug($"User {id} was not found.");

                        continue;
                    }

                    var role = socketGuild.Roles.FirstOrDefault(x => x.Name.Contains("premium", StringComparison.OrdinalIgnoreCase));

                    if (role == null)
                    {
                        _logger.LogWarning("Premium role not found");

                        return;
                    }

                    if (!user.Roles.Contains(role))
                    {
                        _logger.LogTrace($"User {id} does not have the premium role. Skipping...");
                    }
                    else
                    {
                        try
                        {
                            await user.RemoveRoleAsync(role);
                            _logger.LogInformation($"Successfully removed role {role.Name} from expired user {id}.");
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, $"Failed to remove role {role.Name} from expired user {id}.");
                        }
                    }
                }
            }
        }
    }
}