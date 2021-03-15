using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class BlacklistedEntityRepository : RepositoryBase<BlacklistedEntity>, IBlacklistedEntityRepository
	{
		public BlacklistedEntityRepository(KaguyaDbContext dbContext) : base(dbContext) {}
		public async Task<bool> IsBlacklisted(ulong key) { return await Table.AsNoTracking().AnyAsync(x => x.EntityId == key); }
	}
}