using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
	public class ReactionRoleRepository : RepositoryBase<ReactionRole>, IReactionRoleRepository
	{
		public ReactionRoleRepository(KaguyaDbContext dbContext) : base(dbContext) {}
	}
}