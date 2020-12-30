using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Services;

namespace Kaguya.Database.Interfaces
{
    public interface IFishRepository : IRepository<long, Fish>
    {
        public Task<IList<Fish>> GetAllForUserAsync(ulong userId);
        public Task<IList<Fish>> GetAllForServerAsync(ulong serverId);
        public Task<IList<Fish>> GetAllOfTypeForUserAsync(ulong userId, FishType fish);
        public Task<IList<Fish>> GetAllOfRarityForUserAsync(ulong userId, FishRarity rarity);
        public Task<int> GetCountAsync();
    }
}