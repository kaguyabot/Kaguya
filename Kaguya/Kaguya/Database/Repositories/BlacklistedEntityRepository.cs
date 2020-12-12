using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class BlacklistedEntityRepository : IBlacklistedEntityRepository
    {
        private readonly KaguyaDbContext _dbContext;
        private readonly ILogger<BlacklistedEntityRepository> _logger;
		
        public BlacklistedEntityRepository(KaguyaDbContext dbContext, ILogger<BlacklistedEntityRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public async Task<BlacklistedEntity> GetAsync(ulong key)
        {
            return await _dbContext.BlacklistedEntities.AsQueryable().FirstOrDefaultAsync(x => x.EntityId == key);
        }

        public async Task DeleteAsync(ulong key)
        {
            var match = await GetAsync(key);
            
            if (match != null)
                _dbContext.BlacklistedEntities.Remove(match);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(BlacklistedEntity value)
        {
            _dbContext.BlacklistedEntities.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(BlacklistedEntity value)
        {
            _dbContext.BlacklistedEntities.Add(value);
            await _dbContext.SaveChangesAsync();
        }
    }
}