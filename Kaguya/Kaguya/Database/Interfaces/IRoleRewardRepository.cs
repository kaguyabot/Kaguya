using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IRoleRewardRepository
    {
        public Task GetAsync(ulong serverId, ulong roleId);
        public Task DeleteAsync(RoleReward key);
        public Task UpdateAsync(RoleReward value);
        public Task InsertAsync(RoleReward value);
        public Task<IList<RoleReward>> GetAllForServerAsync(ulong serverId);
    }
}