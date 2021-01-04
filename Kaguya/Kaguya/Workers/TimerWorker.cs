using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using Kaguya.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Kaguya.Workers
{

	public class TimerWorker : BackgroundService
	{
		private readonly ITimerInternal _timerService;
		private readonly ILogger<TimerWorker> _logger;
		private const double TIMER_RESOLUTION = 1000;
		private Timer _timer;

		private readonly object _locker = new();
		private readonly SortedList<DateTime, List<(ITimerReceiver receiver, object payload)>> _events = new();
		
		public TimerWorker(ITimerService timerService, ILogger<TimerWorker> logger)
		{
			_timerService = timerService as ITimerInternal;
			_logger = logger;
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Task.CompletedTask;
			_timer = new Timer(TIMER_RESOLUTION);
			_timer.AutoReset = false;
			_timer.Elapsed += HandleTimer;
			_timer.Start();

			while (!stoppingToken.IsCancellationRequested)
			{
				await _timerService.GetChannel().Reader.WaitToReadAsync(stoppingToken);

				var (when, receiver, payload) = await _timerService.GetChannel().Reader.ReadAsync(stoppingToken);
				lock (_locker)
				{
					if (!_events.ContainsKey(when))
					{
						_events.Add(when, new List<(ITimerReceiver receiver, object payload)> {(receiver, payload)});
					}
					else
					{
						_events[when].Add((receiver, payload));
					}
				}
			}
		}

		private void HandleTimer(object sender, ElapsedEventArgs e)
		{
			lock (_locker)
			{
				while (_events.Count > 0 && _events.Keys[0] <= DateTime.Now)
				{
					foreach (var (receiver, payload) in _events[_events.Keys[0]])
					{
						// fire and forget yay!
						receiver.HandleTimer(payload);
					}

					_events.RemoveAt(0);
				}
			}

			_timer.Start();
		}
	}
}