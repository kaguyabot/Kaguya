using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class RepRepository : RepositoryBase<Rep>, IRepRepository
	{
		public RepRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		public async Task<IList<Rep>> GetAllAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
		}

		public async Task<Rep> GetMostRecentAsync(ulong userId)
		{
			return await Table.AsNoTracking().OrderByDescending(x => x.TimeGiven).Where(x => x.UserId == userId).FirstOrDefaultAsync();
		}

		public async Task<int> GetCountRepReceivedAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.UserId == userId).CountAsync();
		}

		public async Task<int> GetCountRepGivenAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.GivenBy == userId).CountAsync();
		}
	}
}