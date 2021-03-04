using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class FishRepository : RepositoryBase<Fish>, IFishRepository
    {
        public FishRepository(KaguyaDbContext dbContext) : base(dbContext)
        { }

        public async Task<IList<Fish>> GetAllForUserAsync(ulong userId)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllForServerAsync(ulong serverId)
        {
             return await Table.AsNoTracking().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfTypeAsync(ulong userId, FishType fish)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId && x.FishType == fish).ToListAsync();
        }

        public async Task<IList<Fish>> GetAllOfRarityAsync(ulong userId, FishRarity rarity)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId && x.Rarity == rarity).ToListAsync();
        }
        
        public async Task<IList<Fish>> GetAllNonTrashAsync(ulong userId)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId && x.Rarity != FishRarity.Trash).ToListAsync();
        }

        public async Task<int> CountAllOfRarityAsync(FishRarity rarity)
        {
            return await Table.AsNoTracking().Where(x => x.Rarity == rarity).CountAsync();
        }

        public async Task<int> CountAllCoinsEarnedAsync()
        {
            return await Table.AsNoTracking().SumAsync(x => x.CoinValue);
        }

        public async Task<int> CountNetCoinsEarnedAsync()
        {
            int gross = await CountAllCoinsEarnedAsync();
            int loss = await Table.AsNoTracking().SumAsync(x => x.CostOfPlay);

            return gross - loss;
        }

        public async Task<int> CountAllNonTrashAsync(ulong userId)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId && x.Rarity != FishRarity.Trash).CountAsync();
        }
    }
}