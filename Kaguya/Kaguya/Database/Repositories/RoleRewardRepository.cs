using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class RoleRewardRepository : RepositoryBase<RoleReward>, IRoleRewardRepository
    {
        private readonly KaguyaDbContext _dbContext;
        protected RoleRewardRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }

        public async Task<IList<RoleReward>> GetAllForServerAsync(ulong serverId)
        {
            return await _dbContext.RoleRewards.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }
    }
}