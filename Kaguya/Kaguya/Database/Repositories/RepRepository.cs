using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class RepRepository : RepositoryBase<Rep>, IRepRepository
    {
        private readonly KaguyaDbContext _dbContext;
        
        public RepRepository(KaguyaDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IList<Rep>> GetAllAsync(ulong userId)
        {
            return await _dbContext.Rep.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<Rep> GetMostRecentAsync(ulong userId)
        {
            return await _dbContext.Rep.AsQueryable()
                                        .OrderByDescending(x => x.TimeGiven)
                                        .Where(x => x.UserId == userId)
                                        .FirstOrDefaultAsync();
        }

        public async Task<int> GetCountRepAsync(ulong userId)
        {
            return await _dbContext.Rep.AsQueryable().CountAsync(x => x.UserId == userId);
        }
    }
}