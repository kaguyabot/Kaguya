using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
    public class WarnConfigurationRepository : RepositoryBase<WarnConfiguration>, IWarnConfigurationRepository
    {
        protected WarnConfigurationRepository(KaguyaDbContext dbContext) : base(dbContext) { }
    }
}