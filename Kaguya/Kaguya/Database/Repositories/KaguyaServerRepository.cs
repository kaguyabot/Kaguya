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
            if (server is not null)
            {
                return server;
            }

            server = _dbContext.Servers.Add(new KaguyaServer
            {
                ServerId = id
            }).Entity;

            await _dbContext.SaveChangesAsync();
            
            _logger.LogDebug($"Server created: {id}");
            return server;
        }

        public async Task<KaguyaServer> GetAsync(ulong key)
        {
            return await _dbContext.Servers.AsQueryable().FirstOrDefaultAsync(x => x.ServerId == key);
        }

        public async Task DeleteAsync(ulong key)
        {
            var match = await GetAsync(key);

            if (match is null)
            {
                return;
            }

            _dbContext.Servers.Remove(match);
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
	        
	        var updated = _dbContext.Servers.Update(value).Entity;
	        await _dbContext.SaveChangesAsync();

	        return updated;
        }

        public async Task<KaguyaServer> UpdateAsync(KaguyaServer value) => await UpdateAsync(value.ServerId, value);
    }
}