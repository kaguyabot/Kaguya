using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IRoleRewardRepository
    {
        public Task<IList<RoleReward>> GetAllAsync(ulong serverId);
    }
}