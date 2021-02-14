using System;
using System.Threading;
using System.Threading.Tasks;
using Kaguya.Database.Repositories;
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

        public TopGgStatsUpdaterService(ILogger<TopGgStatsUpdaterService> logger,
            IServiceProvider serviceProvider, ITimerService timerService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timerService = timerService;
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
            await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(15), this);

            using (var scope = _serviceProvider.CreateScope())
            {
                var statsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
                var curStats = await statsRepository.GetMostRecentAsync();
                
                // todo: continue
            }
        }
    }
}