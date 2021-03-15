using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IRoleRewardRepository
	{
		public Task<IList<RoleReward>> GetAllAsync(ulong serverId);
	}
}