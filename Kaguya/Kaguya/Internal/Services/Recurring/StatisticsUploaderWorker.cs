using System;
using System.Threading;
using System.Threading.Tasks;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kaguya.Internal.Services.Recurring
{
    public class StatisticsUploaderWorker : BackgroundService, ITimerReceiver
    {
        private readonly ITimerService _timerService;
        private readonly IServiceProvider _serviceProvider;

        public StatisticsUploaderWorker(ITimerService timerService, IServiceProvider serviceProvider)
        {
            _timerService = timerService;
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // First execution
            await _timerService.TriggerAtAsync(DateTime.Now.AddMinutes(1), this);

            if (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var statsRepo = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
                    await statsRepo.PostNewAsync();
                }
            }
        }

        public async Task HandleTimer(object payload)
        {
            await _timerService.TriggerAtAsync(DateTime.Now.AddHours(1), this);
        }
    }
}