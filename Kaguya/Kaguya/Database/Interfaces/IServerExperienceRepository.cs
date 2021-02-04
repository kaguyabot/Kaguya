using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IServerExperienceRepository : IRepository<ServerExperience>
    {
        public Task<ServerExperience> GetOrCreateAsync(ulong serverId, ulong userId);
        public Task<IList<ServerExperience>> GetAllExpForServer(ulong serverId);
        public Task Add(ulong serverId, ulong userId, int amount);
        public Task Subtract(ulong serverId, ulong userId, int amount);
        public Task<int> FetchRankAsync(ulong serverId, ulong userId);
    }
}