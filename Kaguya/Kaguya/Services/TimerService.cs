using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kaguya.Services
{
	public interface ITimerReceiver
	{
		public Task HandleTimer(object payload);
	}

	public interface ITimerService
	{
		Task<bool> TriggerAt(DateTime when, ITimerReceiver receiver, object payload);
		Task<bool> TriggerAt(DateTime when, ITimerReceiver receiver);
	}

	public interface ITimerInternal
	{
		public Channel<(DateTime when, ITimerReceiver receiver, object payload)> GetChannel();
	}

	public class TimerService : ITimerService, ITimerInternal
	{
		private readonly ILogger<TimerService> _logger;

		private static readonly Channel<(DateTime when, ITimerReceiver receiver, object payload)> TimerChannel =
			Channel.CreateUnbounded<(DateTime when, ITimerReceiver receiver, object payload)>();

		public TimerService(ILogger<TimerService> logger)
		{
			_logger = logger;
		}

		public Channel<(DateTime when, ITimerReceiver receiver, object payload)> GetChannel()
		{
			return TimerChannel;
		}

		public async Task<bool> TriggerAt(DateTime when, ITimerReceiver receiver, object payload)
		{
			try
			{
				await TimerChannel.Writer.WriteAsync((when, receiver, payload));
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to write to timer channel, got exception");
				return false;
			}
		}

		public async Task<bool> TriggerAt(DateTime when, ITimerReceiver receiver)
		{
			return await TriggerAt(when, receiver, new { });
		}
	}
}