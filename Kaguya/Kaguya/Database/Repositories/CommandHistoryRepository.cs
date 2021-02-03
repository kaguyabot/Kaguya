using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
	public class CommandHistoryRepository : RepositoryBase<CommandHistory>, ICommandHistoryRepository
	{
		public CommandHistoryRepository(KaguyaDbContext dbContext) : base(dbContext) { }
	}
}