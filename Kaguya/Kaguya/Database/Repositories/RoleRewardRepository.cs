using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class RoleRewardRepository : RepositoryBase<RoleReward>, IRoleRewardRepository
	{
		public RoleRewardRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		public async Task<IList<RoleReward>> GetAllAsync(ulong serverId)
		{
			return await Table.AsNoTracking().Where(x => x.ServerId == serverId).ToListAsync();
		}
	}
}