using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kaguya.Internal.Services.Recurring
{
    /// <summary>
    /// Reverts various timed admin actions users may configure.
    ///
    /// i.e $mute (user) 30 minutes..after 30 minutes, the user must be unmuted.
    /// This sort of task is what this service is responsible for.
    /// </summary>
    public class RevertAdminActionService : BackgroundService, ITimerReceiver
    {
        private readonly ITimerService _timerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;

        public RevertAdminActionService(ITimerService timerService, IServiceProvider serviceProvider,
            DiscordShardedClient client)
        {
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

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var adminActionRepository = scope.ServiceProvider.GetRequiredService<AdminActionRepository>();
                var users = await adminActionRepository
            }
        }
    }
}