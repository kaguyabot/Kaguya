using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
    public class LogConfigurationRepository : RepositoryBase<LogConfiguration>, ILogConfigurationRepository
    {
        public LogConfigurationRepository(KaguyaDbContext dbContext) : base(dbContext)
        { }
        
        public async Task<LogConfiguration> GetOrCreateAsync(ulong key)
        {
            var config = await GetAsync(key);
            if (config is not null)
            {
                return config;
            }

            config = Table.Add(new LogConfiguration
            {
                ServerId = key
            }).Entity;

            await DbContext.SaveChangesAsync();
            
            return config;
        }
    }
}