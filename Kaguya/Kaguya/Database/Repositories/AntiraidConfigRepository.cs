using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class AntiraidConfigRepository : IAntiraidRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public AntiraidConfigRepository(KaguyaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AntiRaidConfig> GetAsync(ulong key)
        {
            return await _dbContext.AntiRaidConfigs.AsQueryable().Where(x => x.ServerId == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(ulong key)
        {
            var match = await GetAsync(key);

            if (match != null)
            {
                _dbContext.AntiRaidConfigs.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(AntiRaidConfig value)
        {
            _dbContext.AntiRaidConfigs.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(AntiRaidConfig value)
        {
            _dbContext.AntiRaidConfigs.Add(value);
            await _dbContext.SaveChangesAsync();
        }
    }
}