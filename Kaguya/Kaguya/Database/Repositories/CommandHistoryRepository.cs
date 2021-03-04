using System;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
	public class CommandHistoryRepository : RepositoryBase<CommandHistory>, ICommandHistoryRepository
	{
		public CommandHistoryRepository(KaguyaDbContext dbContext) : base(dbContext) { }

		public async Task<int> GetSuccessfulCountAsync()
		{
			return await Table.AsNoTracking().Where(x => x.ExecutedSuccessfully).CountAsync();
		}

		public async Task<int> GetSuccessfulCountAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.ExecutedSuccessfully && x.UserId == userId).CountAsync();
		}

		public async Task<int> GetRecentSuccessfulCountAsync(TimeSpan threshold)
		{
			DateTimeOffset constraint = DateTimeOffset.Now - threshold;

			return await Table.AsNoTracking()
			                  .Where(x => x.ExecutedSuccessfully &&
			                              x.ExecutionTime > constraint)
			                  .CountAsync();
		}

		public async Task<int> GetRecentSuccessfulCountAsync(ulong userId, TimeSpan threshold)
		{
			DateTimeOffset constraint = DateTimeOffset.Now - threshold;

			return await Table.AsNoTracking().Where(x => x.ExecutedSuccessfully &&
			                                             x.UserId == userId &&
			                                             x.ExecutionTime > constraint)
			                  .CountAsync();
		}

		public async Task<string> GetFavoriteCommandAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.UserId == userId && x.ExecutedSuccessfully)
			                  .GroupBy(x => x.CommandName)
			                  .OrderByDescending(x => x.Count())
			                  .Select(x => x.Key)
			                  .FirstOrDefaultAsync();
		}

		public async Task<(string cmdName, int count)> GetMostPopularCommandAsync()
		{
			string name = await Table.AsNoTracking()
			                         .GroupBy(x => x.CommandName)
			                         .OrderByDescending(x => x.Count())
			                         .Select(x => x.Key)
			                         .FirstOrDefaultAsync();
			
			int count = await Table.AsNoTracking()
			                       .GroupBy(x => x.CommandName)
			                       .OrderByDescending(x => x.Count())
			                       .Select(x => x.Count())
			                       .FirstOrDefaultAsync();

			return (name, count);
		}
	}
}