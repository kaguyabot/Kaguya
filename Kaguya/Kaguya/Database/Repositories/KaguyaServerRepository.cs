using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class KaguyaServerRepository
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
            var server = await _dbContext.Servers.AsQueryable().FirstOrDefaultAsync(x => x.ServerId == id);
            if (server is not null)
            {
                return server;
            }

            server = (await _dbContext.Servers.AddAsync(new KaguyaServer
            {
                ServerId = id
            })).Entity;

            await _dbContext.SaveChangesAsync();
            
            _logger.Log(LogLevel.Debug, $"New server created. ID: {id}.");
            return server;
        }
    }
}