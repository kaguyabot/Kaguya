using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;

namespace Kaguya.Database.Interfaces
{
    public interface IFishRepository : IRepository<Fish>
    {
        public Task<IList<Fish>> GetAllForUserAsync(ulong userId);
        public Task<IList<Fish>> GetAllForServerAsync(ulong serverId);
        public Task<int> CountAllNonTrashAsync(ulong userId);
        public Task<IList<Fish>> GetAllOfTypeAsync(ulong userId, FishType fish);
        public Task<IList<Fish>> GetAllOfRarityAsync(ulong userId, FishRarity rarity);
        public Task<IList<Fish>> GetAllNonTrashAsync(ulong userId);
    }
}