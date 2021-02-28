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

		/// <summary>
		/// Returns the total count of all successfull commands ever executed.
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetSuccessfulCountAsync()
		{
			return await Table.AsNoTracking().Where(x => x.ExecutedSuccessfully).CountAsync();
		}

		/// <summary>
		/// Returns the count of successful command executions that occurred within the difference
		/// between now and the positive offset.
		/// </summary>
		/// <param name="offset">A positive timespan representing the time constraint.</param>
		/// <returns></returns>
		public async Task<int> GetSuccessfulCountAsync(TimeSpan offset)
		{
			DateTimeOffset constraint = DateTimeOffset.Now - offset;

			return await Table.AsNoTracking()
			                  .Where(x => x.ExecutedSuccessfully &&
			                              x.ExecutionTime > constraint)
			                  .CountAsync();
		}
	}
}