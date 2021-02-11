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
    public class FishRepository : RepositoryBase<Fish>, IFishRepository
    {
        private readonly KaguyaDbContext _dbContext;
		
        public FishRepository(KaguyaDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<Fish>> GetAllForUserAsync(ulong userId)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllForServerAsync(ulong serverId)
        {
             return await _dbContext.Fish.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfTypeAsync(ulong userId, FishType fish)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId && x.FishType == fish).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfRarityAsync(ulong userId, FishRarity rarity)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId && x.Rarity == rarity).ToListAsync();
        }

        public async Task<int> CountAllNonTrashAsync(ulong userId)
        {
            return await _dbContext.Fish.AsQueryable().Where(x => x.UserId == userId && x.Rarity != FishRarity.Trash).CountAsync();
        }
    }
}