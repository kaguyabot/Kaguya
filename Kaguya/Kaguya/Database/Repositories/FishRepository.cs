using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Kaguya.Services;
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
            return await _dbContext.Fish.AsQueryable().FirstOrDefaultAsync(x => x.FishId == key);
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
            var collection = _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId);

            return await collection.ToListAsync();
        }

        public async Task<IList<Fish>> GetAllForServerAsync(ulong serverId)
        {
            var collection = _dbContext.Fish.AsQueryable().Where(x => x.ServerId == serverId);

            return await collection.ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfTypeAsync(FishType fish)
        {
            var collection = _dbContext.Fish.AsQueryable().Where(x => x.FishType == fish);

            return await collection.ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfRarityAsync(FishRarity rarity)
        {
            var collection = _dbContext.Fish.AsQueryable().Where(x => x.Rarity == rarity);

            return await collection.ToListAsync();
        }

        public async Task<IList<Fish>> GetAllAsync()
        {
            return await _dbContext.Fish.AsQueryable().ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _dbContext.Fish.AsQueryable().CountAsync();
        }
    }
}