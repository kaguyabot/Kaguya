using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class FishRepository : IFishRepository
    {
        private readonly KaguyaDbContext _dbContext;
        private readonly ILogger<FishRepository> _logger;
		
        public FishRepository(KaguyaDbContext dbContext, ILogger<FishRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public async Task<Fish> GetAsync(long key)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.FishId == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(long key)
        {
            var match = await GetAsync(key);
            if (match != null)
            {
                _dbContext.Fish.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Fish value)
        {
            _dbContext.Fish.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(Fish value)
        {
            _dbContext.Fish.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<Fish>> GetAllForUserAsync(ulong userId)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllForServerAsync(ulong serverId)
        {
             return await _dbContext.Fish.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfTypeForUserAsync(ulong userId, FishType fish)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId && x.FishType == fish).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfRarityForUserAsync(ulong userId, FishRarity rarity)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId && x.Rarity == rarity).ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _dbContext.Fish.AsQueryable().CountAsync();
        }
    }
}