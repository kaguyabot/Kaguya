using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class AntiraidConfigRepository : RepositoryBase<AntiRaidConfig>, IAntiraidRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public AntiraidConfigRepository(KaguyaDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AntiRaidConfig> GetAsync(ulong key)
        {
            // todo: Can we inherit from base getasync method?
            return await _dbContext.AntiRaidConfigs.AsQueryable().AsNoTrackingWithIdentityResolution().Where(x => x.ServerId == key).FirstOrDefaultAsync();
        }

        public async Task InsertOrUpdateAsync(AntiRaidConfig config)
        {
            AntiRaidConfig current = await GetAsync(config.ServerId);
            
            if (current == null)
            {
                await InsertAsync(config);
            }
            else
            {
                await UpdateAsync(config);
            }
        }
    }
}