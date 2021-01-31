using System;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class KaguyaServerRepository : IKaguyaServerRepository
    {
        private readonly KaguyaDbContext _dbContext;
        private readonly ILogger<KaguyaServerRepository> _logger;

        public KaguyaServerRepository(KaguyaDbContext dbContext, ILogger<KaguyaServerRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        public async Task<KaguyaServer> GetOrCreateAsync(ulong id)
        {
            var server = await GetAsync(id);
            if (server != null)
            {
                return server;
            }

            server = _dbContext.KaguyaServers.Add(new KaguyaServer
            {
                ServerId = id,
                DateFirstTracked = DateTime.Now
            }).Entity;

            await _dbContext.SaveChangesAsync();
            
            _logger.LogDebug($"Server created: {id}");
            return server;
        }

        public async Task<KaguyaServer> GetAsync(ulong key)
        {
            return await _dbContext.KaguyaServers.AsQueryable().Where(x => x.ServerId == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(ulong key)
        {
            var match = await GetAsync(key);

            if (match is null)
            {
                return;
            }

            _dbContext.KaguyaServers.Remove(match);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogDebug($"Server deleted: {key}");
        }

        public async Task<KaguyaServer> UpdateAsync(ulong id, KaguyaServer value)
        {
	        var current = await GetAsync(id);

	        if (current is null)
	        {
		        return null;
	        }
	        
	        var updated = _dbContext.KaguyaServers.Update(value).Entity;
	        await _dbContext.SaveChangesAsync();

	        return updated;
        }

        public async Task InsertAsync(KaguyaServer value)
        {
	        _dbContext.KaguyaServers.Add(value);
	        await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(KaguyaServer value)
        {
	        var current = await _dbContext.KaguyaServers
                                          .AsQueryable()
                                          .Where(x => x.ServerId == value.ServerId)
                                          .FirstOrDefaultAsync();

	        if (current.Equals(value))
		        return;
	        
	        _dbContext.KaguyaServers.Update(value);
	        await _dbContext.SaveChangesAsync();
        }
    }
}