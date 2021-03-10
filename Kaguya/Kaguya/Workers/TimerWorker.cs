using Kaguya.Internal.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Kaguya.Workers
{
	public class TimerWorker : BackgroundService
	{
		private const double TIMER_RESOLUTION = 1000;
		private readonly SortedList<DateTimeOffset, List<(ITimerReceiver receiver, object payload)>> _events = new();
		private readonly object _locker = new();
		private readonly ITimerInternal _timerService;
		private System.Timers.Timer _timer;
		public TimerWorker(ITimerService timerService) { _timerService = timerService as ITimerInternal; }

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Task.CompletedTask;
			_timer = new System.Timers.Timer(TIMER_RESOLUTION);
			_timer.AutoReset = false;
			_timer.Elapsed += HandleTimer;
			_timer.Start();

			while (!stoppingToken.IsCancellationRequested)
			{
				await _timerService.GetChannel().Reader.WaitToReadAsync(stoppingToken);

				(var when, var receiver, var payload) =
					await _timerService.GetChannel().Reader.ReadAsync(stoppingToken);

				lock (_locker)
				{
					if (!_events.ContainsKey(when))
					{
						_events.Add(when, new List<(ITimerReceiver receiver, object payload)>
						{
							(receiver, payload)
						});
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
				while (_events.Count > 0 && _events.Keys[0] <= DateTimeOffset.Now)
				{
					foreach ((var receiver, var payload) in _events[_events.Keys[0]])
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