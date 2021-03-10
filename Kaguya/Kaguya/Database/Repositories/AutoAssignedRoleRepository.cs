using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
	public class AutoAssignedRoleRepository : RepositoryBase<AutoAssignedRole>, IAutoAssignedRoleRepository
	{
		public AutoAssignedRoleRepository(KaguyaDbContext dbContext) : base(dbContext) {}
	}
}