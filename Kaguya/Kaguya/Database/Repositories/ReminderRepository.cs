using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class ReminderRepository : RepositoryBase<Reminder>, IReminderRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public ReminderRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }

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