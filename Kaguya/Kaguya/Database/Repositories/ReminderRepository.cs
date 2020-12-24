using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public ReminderRepository(KaguyaDbContext dbContext) { _dbContext = dbContext; }

        public async Task<Reminder> GetAsync(long key)
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.Id == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(long key)
        {
            Reminder match = await GetAsync(key);
            
            if (match != null)
            {
                _dbContext.Reminders.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Reminder value)
        {
            _dbContext.Reminders.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(Reminder value)
        {
            _dbContext.Reminders.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<Reminder>> GetAllForUserAsync(ulong id)
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.UserId == id).ToListAsync();
        }

        public async Task<IList<Reminder>> GetAllToDeliverForUserAsync(ulong id)
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.UserId == id && x.NeedsDelivery).ToListAsync();
        }

        public async Task<IList<Reminder>> GetAllToDeliverAsync()
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.NeedsDelivery).ToListAsync();
        }
    }
}