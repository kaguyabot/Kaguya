using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Services
{
	public interface ITimerReceiver
	{
		public Task HandleTimer(object payload);
	}

	public interface ITimerService
	{
		Task<bool> TriggerAtAsync(DateTimeOffset when, ITimerReceiver receiver, object payload);
		Task<bool> TriggerAtAsync(DateTimeOffset when, ITimerReceiver receiver);
	}

	public interface ITimerInternal
	{
		public Channel<(DateTimeOffset when, ITimerReceiver receiver, object payload)> GetChannel();
	}

	public class TimerService : ITimerService, ITimerInternal
	{
		private readonly ILogger<TimerService> _logger;

		private static readonly Channel<(DateTimeOffset when, ITimerReceiver receiver, object payload)> _timerChannel =
			Channel.CreateUnbounded<(DateTimeOffset when, ITimerReceiver receiver, object payload)>();

		public TimerService(ILogger<TimerService> logger)
		{
			_logger = logger;
		}

		public Channel<(DateTimeOffset when, ITimerReceiver receiver, object payload)> GetChannel()
		{
			return _timerChannel;
		}

		public async Task<bool> TriggerAtAsync(DateTimeOffset when, ITimerReceiver receiver, object payload)
		{
			try
			{
				await _timerChannel.Writer.WriteAsync((when, receiver, payload));
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to write to timer channel, got exception");
				return false;
			}
		}

		public async Task<bool> TriggerAtAsync(DateTimeOffset when, ITimerReceiver receiver)
		{
			return await TriggerAtAsync(when, receiver, new { });
		}
	}
}