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
			return await Table.AsQueryable().Where(x => x.ExecutedSuccessfully).CountAsync();
		}
	}
}