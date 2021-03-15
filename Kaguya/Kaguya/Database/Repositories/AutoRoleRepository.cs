using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class AutoRoleRepository : RepositoryBase<AutoRole>, IAutoRoleRepository
	{
		public AutoRoleRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		public async Task<AutoRole> GetAsync(ulong roleId)
		{
			return await Table.AsNoTracking().Where(x => x.RoleId == roleId).FirstOrDefaultAsync();
		}

		public async Task<int> CountAsync(ulong serverId)
		{
			return await Table.AsNoTracking().Where(x => x.ServerId == serverId).CountAsync();
		}

		public async Task<IList<AutoRole>> GetAllAsync(ulong serverId)
		{
			return await Table.AsNoTracking().Where(x => x.ServerId == serverId).ToListAsync();
		}

		public async Task ClearAllAsync(ulong serverId)
		{
			var matches = await GetAllAsync(serverId);
			
			Table.RemoveRange(matches);
			await DbContext.SaveChangesAsync();
		}

		public async Task DeleteByRoleIdAsync(ulong roleId)
		{
			var match = await GetAsync(roleId);
			await DeleteAsync(match.Id);
		}
	}
}