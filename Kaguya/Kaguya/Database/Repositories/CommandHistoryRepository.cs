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
		
		public async Task<int> GetRecentSuccessfulCountAsync(TimeSpan offset)
		{
			DateTimeOffset constraint = DateTimeOffset.Now - offset;

			return await Table.AsNoTracking()
			                  .Where(x => x.ExecutedSuccessfully &&
			                              x.ExecutionTime > constraint)
			                  .CountAsync();
		}
	}
}