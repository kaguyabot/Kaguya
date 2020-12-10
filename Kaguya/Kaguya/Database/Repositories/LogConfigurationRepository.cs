using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class LogConfigurationRepository : ILogConfigurationRepository
    {
        private readonly KaguyaDbContext _dbContext;
        private readonly ILogger<AdminActionRepository> _logger;

        public LogConfigurationRepository(KaguyaDbContext dbContext, ILogger<AdminActionRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        public async Task<LogConfiguration> GetAsync(ulong key)
        {
            return await _dbContext.LogConfigurations.AsQueryable().FirstOrDefaultAsync(x => x.ServerId == key);
        }
        
        
        public async Task<LogConfiguration> GetOrCreateAsync(ulong key)
        {
            var config = await GetAsync(key);
            if (config is not null)
            {
                return config;
            }

            config = _dbContext.LogConfigurations.Add(new LogConfiguration
            {
                ServerId = key
            }).Entity;

            await _dbContext.SaveChangesAsync();
            
            return config;
        }

        public async Task DeleteAsync(ulong key)
        {
            var match = await GetAsync(key);

            if (match != null)
            {
                _dbContext.LogConfigurations.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(LogConfiguration value)
        {
            _dbContext.LogConfigurations.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(LogConfiguration value)
        {
            LogConfiguration match = await GetAsync(value.ServerId);

            if (match == null)
            {
                _dbContext.LogConfigurations.Add(value);
            }
        }
    }
}