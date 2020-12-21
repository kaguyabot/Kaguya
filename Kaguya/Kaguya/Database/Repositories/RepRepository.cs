using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class RepRepository : IRepRepository
    {
        private readonly ILogger<RepRepository> _logger;
        private readonly KaguyaDbContext _dbContext;
        
        public RepRepository(ILogger<RepRepository> logger, KaguyaDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public async Task<Rep> GetAsync(long key)
        {
            return await _dbContext.Rep.AsQueryable().FirstOrDefaultAsync(x => x.Id == key);
        }

        public async Task DeleteAsync(long key)
        {
            var match = await GetAsync(key);
            if (match != null)
            {
                _dbContext.Rep.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Rep value)
        {
            _dbContext.Rep.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(Rep value)
        {
            _dbContext.Rep.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<Rep>> GetAllForUserAsync(ulong userId)
        {
            var matches = _dbContext.Rep.AsQueryable().Where(x => x.UserId == userId);

            return await matches.ToListAsync();
        }

        public async Task<Rep> GetMostRecentForUserAsync(ulong userId)
        {
            return await _dbContext.Rep.AsQueryable().OrderByDescending(x => x.TimeGiven).FirstOrDefaultAsync();
        }
    }
}