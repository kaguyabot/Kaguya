using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class PremiumKeyRepository : IPremiumKeyRepository
    {
        private readonly ILogger<PremiumKeyRepository> _logger;
        private readonly KaguyaDbContext _dbContext;
        
        public PremiumKeyRepository(ILogger<PremiumKeyRepository> logger, KaguyaDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public async Task<PremiumKey> GetAsync(long key)
        {
            return await _dbContext.PremiumKeys.AsQueryable().Where(x => x.Id == key).FirstOrDefaultAsync();
        }

        public async Task<PremiumKey> GetAsync(string key)
        {
            return await _dbContext.PremiumKeys.AsQueryable().Where(x => x.Key == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(long key)
        {
            var match = await GetAsync(key);

            if (match != null)
            {
                _dbContext.PremiumKeys.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(PremiumKey value)
        {
            _dbContext.PremiumKeys.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(PremiumKey value)
        {
            _dbContext.PremiumKeys.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BulkInsert(IList<PremiumKey> keys)
        {
            _dbContext.PremiumKeys.AddRange(keys);
            await _dbContext.SaveChangesAsync();
        }
    }
}