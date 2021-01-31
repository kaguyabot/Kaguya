using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class EightballRepository : IEightBallRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public EightballRepository(KaguyaDbContext dbContext) { _dbContext = dbContext; }
        
        public async Task<Eightball> GetAsync(string key)
        {
            return await _dbContext.Eightballs.AsQueryable().Where(x => x.Phrase == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(string key)
        {
            Eightball match = await GetAsync(key);

            if (match != null)
            {
                _dbContext.Eightballs.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Eightball value)
        {
            _dbContext.Eightballs.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(Eightball value)
        {
            _dbContext.Eightballs.Add(value);
            await _dbContext.SaveChangesAsync();
        }
    }
}