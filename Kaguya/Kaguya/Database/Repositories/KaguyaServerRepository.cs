using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class KaguyaServerRepository : RepositoryBase<KaguyaServer>, IKaguyaServerRepository
	{
		private readonly ILogger<KaguyaServerRepository> _logger;

		public KaguyaServerRepository(KaguyaDbContext dbContext, ILogger<KaguyaServerRepository> logger) : base(dbContext)
		{
			_logger = logger;
		}

		public async Task<KaguyaServer> GetOrCreateAsync(ulong id)
		{
			var server = await GetAsync(id);
			if (server != null)
			{
				return server;
			}

			server = Table.Add(new KaguyaServer
			              {
				              ServerId = id,
				              DateFirstTracked = DateTimeOffset.Now
			              })
			              .Entity;

			await DbContext.SaveChangesAsync();

			_logger.LogDebug($"Server created: {id}");
			return server;
		}
	}
}