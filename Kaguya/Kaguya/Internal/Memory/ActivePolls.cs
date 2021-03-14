using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaguya.Internal.Memory
{
	public class ActivePolls
	{
		private static readonly SynchronizedCollection<ulong> _currentlyActivePolls = new();

		public ActivePolls(ILogger<ActivePolls> logger, IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var pollRepository = scope.ServiceProvider.GetRequiredService<PollRepository>();
				var activePolls = pollRepository.GetAllOngoingAsync().GetAwaiter().GetResult();

				foreach (var p in activePolls)
				{
					InsertId(p.ServerId);
				}

				logger.LogInformation($"Active polls initialized. There are currently {_currentlyActivePolls.Count} " +
				                      $"active polls.");
			}
		}
		
		public static int CountActivePolls(ulong serverId)
		{
			return _currentlyActivePolls.Count(x => x == serverId);
		}

		public static void InsertId(ulong serverId)
		{
			_currentlyActivePolls.Add(serverId);
		}

		public static void RemoveId(ulong serverId)
		{
			_currentlyActivePolls.Remove(serverId);
		}

		public SynchronizedCollection<ulong> GetCollection()
		{
			return _currentlyActivePolls;
		}
	}
}