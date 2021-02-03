using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{ 
    public class BlacklistedEntityRepository : RepositoryBase<BlacklistedEntity>, IBlacklistedEntityRepository
    {
        private readonly KaguyaDbContext _dbContext;
		
        public BlacklistedEntityRepository(KaguyaDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<bool> IsBlacklisted(ulong key)
        {
            return await _dbContext.BlacklistedEntities.AsQueryable().AnyAsync(x => x.EntityId == key);
        }
    }
}